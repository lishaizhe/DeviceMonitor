using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor
{
    [Serializable]
    public class GroupBuild
    {
        public string[] bundles = new string[0];
        public string name;
    }

    [Serializable]
    public class AssetBuild
    {
        public string path;
        public string bundle;
        public long time;
        public long metaTime;
        public string[] bundles = new string[0];
        public string metaPath;
        public string[] dependencies = new string[0];
        public int id { get; set; }

        public bool dirty => time != Settings.GetLastWriteTime(path) || metaTime != Settings.GetLastWriteTime(metaPath);

        public AssetInfo GetInfo(Dictionary<string, BundleBuild> buildBundles)
        {
            var mainId = -1;
            if (buildBundles.TryGetValue(bundle, out var main))
            {
                mainId = main.id;
            }
            else
            {
                Logger.E("Bundle not found {0} with {1}.", bundle, path);
            }
            var ids = new List<int>();
            foreach (var item in bundles)
            {
                if (!buildBundles.TryGetValue(item, out var dep))
                {
                    Logger.E("Bundle not found {0} with {1}.", item, path);
                    continue;
                }

                ids.Add(dep.id);
            }

            return new AssetInfo
            {
                id = id,
                bundle = mainId,
                bundles = ids.ToArray()
            };
        }

        public void AfterBuild()
        {
            time = Settings.GetLastWriteTime(path);
            metaTime = Settings.GetLastWriteTime(metaPath);
        }
    }

    [Serializable]
    public class BundleBuild
    {
        public string name;
        public List<AssetBuild> assets = new List<AssetBuild>();
        public ulong size;
        public uint crc;
        public string nameWithAppendHash;
        public int id;
        public string[] deps = new string[0];

        public BundleInfo GetInfo(Dictionary<string, BundleBuild> buildBundles)
        {
            var ids = new int[assets.Count];
            for (var index = 0; index < assets.Count; index++)
            {
                var asset = assets[index];
                ids[index] = asset.id;
            }

            var info = new BundleInfo
            {
                id = id,
                assets = ids,
                name = nameWithAppendHash,
                crc = crc,
                size = size,
                deps = Array.ConvertAll(deps, input =>
                {
                    if (buildBundles.TryGetValue(input, out var dep))
                    {
                        return dep.id;
                    }

                    return -1;
                })
            };
            return info;
        }

        public void AfterBuild()
        {
            var file = Settings.GetBuildPath(nameWithAppendHash);
            if (File.Exists(file))
            {
                using (var stream = File.OpenRead(file))
                {
                    size = (ulong) stream.Length;
                    crc = Utility.ComputeCRC32(stream);
                }
            }

            foreach (var asset in assets)
            {
                asset.AfterBuild();
            }
        }

        public AssetBundleBuild GetBuild()
        {
            var build = new AssetBundleBuild
            {
                assetBundleName = name,
                assetNames = Array.ConvertAll(assets.ToArray(), input => input.path)
            };
            return build;
        }

        public void GetDependencies(Dictionary<string, string> assetWithBundles)
        {
            // Generate bundles for each entry
            foreach (var asset in assets)
            {
                var bundles = new HashSet<string>();
                foreach (var dependency in asset.dependencies)
                {
                    string bundle;
                    if (assetWithBundles.TryGetValue(dependency, out bundle))
                    {
                        bundles.Add(bundle);
                    }
                }

                asset.bundles = bundles.ToArray();
            }
        }
    }

    /// <summary>
    ///     打包后的缓存数据
    /// </summary>
    public class Build : ScriptableObject
    {
        // public string size;
        public List<string> newFiles = new List<string>();
        public List<BundleBuild> bundles = new List<BundleBuild>();
        public List<GroupBuild> groups = new List<GroupBuild>();
        public int version;

        public Dictionary<string, BundleBuild> GetBundles()
        {
            var dictionary = new Dictionary<string, BundleBuild>();
            foreach (var bundle in bundles)
            {
                dictionary[bundle.name] = bundle;
            }
            return dictionary;
        }

        public Dictionary<string, AssetBuild> GetAssets()
        {
            var dictionary = new Dictionary<string, AssetBuild>();
            foreach (var bundle in bundles)
            foreach (var asset in bundle.assets)
            {
                dictionary[asset.path] = asset;
            }
            return dictionary;
        }

        public Dictionary<string, GroupBuild> GetGroups()
        {
            var dictionary = new Dictionary<string, GroupBuild>();
            foreach (var group in groups)
            {
                dictionary[group.name] = group;
            }
            return dictionary;
        }

        public void Clear()
        {
            groups.Clear();
            bundles.Clear();
            newFiles.Clear();
            version = 0;
        }

        public string CreateManifest()
        {
            var assetNames = new List<string>();
            var manifest = new VEngine.Manifest();
            var filename = $"{name}".ToLower();
            var savePath = Settings.GetBuildPath(filename);
            var buildBundles = new Dictionary<string, BundleBuild>();
            var assetsInBuild = new List<AssetBuild>();
            for (var index = 0; index < bundles.Count; index++)
            {
                var bundle = bundles[index];
                bundle.id = index;
                buildBundles[bundle.name] = bundle;
                foreach (var asset in bundle.assets)
                {
                    asset.id = assetsInBuild.Count;
                    assetsInBuild.Add(asset);
                    assetNames.Add(asset.path);
                }
            }

            manifest.groups = groups.ConvertAll(ConverterGroup(buildBundles));
            manifest.assets = assetsInBuild.ConvertAll(input => input.GetInfo(buildBundles));
            manifest.bundles = bundles.ConvertAll(input => input.GetInfo(buildBundles));
            manifest.SetAllAssetPaths(assetNames.ToArray());
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    manifest.appVersion = UnityEditor.PlayerSettings.Android.bundleVersionCode +
                                          UnityEditor.PlayerSettings.bundleVersion;
                    break;
                case BuildTarget.iOS:
                    manifest.appVersion = UnityEditor.PlayerSettings.iOS.buildNumber +
                                          UnityEditor.PlayerSettings.bundleVersion;
                    break;
                default:
                    manifest.appVersion = UnityEditor.PlayerSettings.bundleVersion;
                    break;
            }
            manifest.version = version;
            return manifest.Save(savePath);
        }

        private static Converter<GroupBuild, GroupInfo> ConverterGroup(Dictionary<string, BundleBuild> buildBundles)
        {
            return input =>
            {
                var groupInfo = new GroupInfo
                {
                    name = input.name,
                    bundles = Array.ConvertAll(input.bundles, s =>
                    {
                        if (buildBundles.TryGetValue(s, out var value))
                        {
                            return value.id;
                        }
                        Logger.W("Bundle {0} not find", s);
                        return -1;
                    })
                };
                return groupInfo;
            };
        }
    }
}