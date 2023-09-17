using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor
{
    /// <summary>
    ///     BuildScript 类，实现了具体的打包逻辑
    /// </summary>
    public static class BuildScript
    {
        public static Action<Manifest> preprocessBuildBundles { get; set; }
        public static Action<Manifest> postprocessBuildBundles { get; set; }

        /// <summary>
        ///     构建资源
        /// </summary>
        public static void BuildBundles()
        {
            var settings = Settings.GetDefaultSettings();
            var manifests = new List<string>();
            foreach (var manifest in settings.manifests)
            {
                manifests.Add(AssetDatabase.GetAssetPath(manifest));
            }
            foreach (var manifest in manifests)
            {
                BuildBundles(manifest);
            }
        }
        
        public static void BuildBundles(List<string> manifestNames)
        {
            var settings = Settings.GetDefaultSettings();
            var manifests = new List<string>();
            foreach (var manifest in settings.manifests)
            {
                if (manifestNames.Contains(manifest.name))
                {
                    manifests.Add(AssetDatabase.GetAssetPath(manifest));
                }
            }
            foreach (var manifest in manifests)
            {
                Debug.Log($"[Publish] Build manifest {manifest}");
                BuildBundles(manifest);
            }
        }

        public static void SaveManifestVersion()
        {
            var settings = Settings.GetDefaultSettings();
            var manifests = settings.manifests.ConvertAll(AssetDatabase.GetAssetPath);
            
            // 合并manifest.version
            var builder = new StringBuilder();
            foreach (var manifest in manifests)
            {
                var m = EditorUtility.GetOrCreateAsset<Manifest>(manifest);
                var versionPath = Settings.GetBuildPath($"{m.name.ToLower()}.version");
                if (File.Exists(versionPath))
                {
                    builder.AppendLine($"{m.name},{File.ReadAllText(versionPath).Trim()}");
                }
            }

            var bundleVersion = GetBundleVersion();
            string manifestVersionPath = Settings.GetBuildPath(ManifestFile.GetManifestVersion(bundleVersion, ""));
            File.WriteAllText(manifestVersionPath, builder.ToString());
            string manifestVersionPathGM = Settings.GetBuildPath(ManifestFile.GetManifestVersion(bundleVersion, "_gm"));
            File.WriteAllText(manifestVersionPathGM, builder.ToString());
        }

        public static void BuildBundles(string manifestPath)
        {
            var asset = EditorUtility.GetOrCreateAsset<Manifest>(manifestPath);
            BuildBundles(asset);
        }

        public static void BuildBundles(Manifest manifest)
        {
            if (manifest == null)
            {
                return;
            }

            List<Asset> assets = null;
            Dictionary<string, List<Asset>> dependencyWithAssets = null;
            
            Logger.T(() =>
            {
                if (preprocessBuildBundles != null)
                {
                    preprocessBuildBundles.Invoke(manifest);
                }

                var builds = manifest.BuildGroups(out var bundleBuilds, out var rawBundleBuilds, out assets, out dependencyWithAssets);
                UnityEditor.EditorUtility.SetDirty(manifest);
                Debug.LogFormat("{0} build bundle count {1}", manifest.name, builds.Length);
                if (builds.Length > 0)
                {
                    var assetPath = AssetDatabase.GetAssetPath(manifest);
                    var platform = EditorUserBuildSettings.activeBuildTarget;
                    var outputFolder = EditorUtility.PlatformBuildPath;
                    BuildAssetBundleOptions bundleOptions = manifest.buildAssetBundleOptions |
                                                            BuildAssetBundleOptions.DisableWriteTypeTree |
                                                            BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                                            BuildAssetBundleOptions.DeterministicAssetBundle |
                                                            BuildAssetBundleOptions.StrictMode;
                    var assetBundleManifest =
                        BuildPipeline.BuildAssetBundles(outputFolder, builds, bundleOptions, platform);
                    // 重新获取之前的版本文件，因为打包后，之前的内存数据会被 Unity 清空
                    manifest = EditorUtility.GetOrCreateAsset<Manifest>(assetPath);
                    if (assetBundleManifest == null)
                    {
                        var msg = "Failed to build {0} with bundles, because assetBundleManifest == null.";
                        Logger.E(msg, manifest.name);
                        throw new Exception(msg);
                        return;
                    }

                    manifest.CreateVersions(assetBundleManifest, bundleBuilds);
                }
                else
                {
                    var msg = "Nothing to build for " + manifest.name;
                    Debug.LogErrorFormat("Nothing to build for {0}.", manifest.name);
                    throw new Exception(msg);
                    return;
                    if (rawBundleBuilds.Count <= 0 && manifest.GetBuild().GetBundles().Count == bundleBuilds.Count)
                    {
                        Debug.LogFormat("Nothing to build for {0}.", manifest.name);
                    }
                    manifest.CreateVersions(null, bundleBuilds);
                }
                if (postprocessBuildBundles != null)
                {
                    postprocessBuildBundles.Invoke(manifest);
                }
            }, $"Build Bundles for {manifest.name}");

            if (!CheckAssetsReference(manifest, assets, dependencyWithAssets, out _))
            {
                //throw new Exception("Manifest 资源存在相互引用");
            }
        }

        /// <summary>
        ///     检测Manifest资源引用错误
        /// </summary>
        public static void CheckManifestRefs()
        {
            var settings = Settings.GetDefaultSettings();
            foreach (var manifest in settings.manifests)
            {
                List<Asset> assets = null;
                Dictionary<string, List<Asset>> dependencyWithAssets = null;
                
                Logger.T(() => manifest.BuildGroups(out _, out _, out assets, out dependencyWithAssets), $"检测资源引用 {manifest.name}");

                CheckAssetsReference(manifest, assets, dependencyWithAssets, out var logFile);
                if (!string.IsNullOrEmpty(logFile))
                {
                    UnityEditor.EditorUtility.OpenWithDefaultApp(logFile);
                }
            }
        }

        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }
        
        public static string GetBundleVersion()
        {
            return UnityEditor.PlayerSettings.bundleVersion;
        }

        /// <summary>
        ///     获取打包播放器的目标名字
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetBuildTargetName(BuildTarget target)
        {
            var productName = "xc" + "-v" + UnityEditor.PlayerSettings.bundleVersion + ".";
            var targetName = $"/{productName}-{GetTimeForNow()}";
            switch (target)
            {
                case BuildTarget.Android:
                    return targetName + ".apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }

        /// <summary>
        ///     打包播放器
        /// </summary>
        public static void BuildPlayer()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Build");
            if (path.Length == 0)
            {
                return;
            }

            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    levels.Add(scene.path);
                }
            }

            if (levels.Count == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetName = GetBuildTargetName(buildTarget);
            if (buildTargetName == null)
            {
                return;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = path + buildTargetName,
                target = buildTarget,
                options = EditorUserBuildSettings.development
                    ? BuildOptions.Development
                    : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static void Clear()
        {
            Settings.GetDefaultSettings().Clear();
        }

        public static void CopyToStreamingAssets()
        {
            Settings.GetDefaultSettings().CopyToStreamingAssets();
        }

        public static void ClearHistory()
        {
            var settings = Settings.GetDefaultSettings();
            var usedFiles = new List<string>
            {
                EditorUtility.GetPlatformName(),
                EditorUtility.GetPlatformName() + ".manifest"
            };
            foreach (var manifest in settings.manifests)
            {
                var build = manifest.GetBuild();
                usedFiles.Add(manifest.name + ".json");
                usedFiles.Add(manifest.name.ToLower());
                usedFiles.Add(VEngine.Manifest.GetVersionFile(manifest.name.ToLower()));
                foreach (var bundle in build.bundles)
                {
                    usedFiles.Add(bundle.nameWithAppendHash);
                    usedFiles.Add(bundle.name + ".manifest");
                }
            }

            var files = Directory.GetFiles(EditorUtility.PlatformBuildPath);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (usedFiles.Contains(name))
                {
                    continue;
                }
                File.Delete(file);
                Logger.I("Delete {0}", file);
            }
        }


        private static bool CheckAssetsReference(Manifest manifest, List<Asset> assets, Dictionary<string, List<Asset>> dependencyWithAssets, out string logFile)
        {
            logFile = null;
            if (manifest == null || assets == null || dependencyWithAssets == null)
                return true;
            
            bool succ = true;
            
            if (manifest.name == "GameRes")
            {
                int index = 0;
                var builder = new StringBuilder();
                
                builder.AppendLine(manifest.name);
                
                foreach (var i in assets)
                {
                    if (i.path.StartsWith("Assets/Download") || i.path.StartsWith("Assets/PackageRes"))
                    {
                        index++;
                        succ = false;
                        builder.AppendLine($"[{index}] {i.path}@{i.bundle}");
                        if (dependencyWithAssets.TryGetValue(i.path, out var refs))
                        {
                            for (var j = 0; j < refs.Count; j++)
                            {
                                var r = refs[j];
                                builder.AppendLine($"  - {j} {r.path}@{r.bundle}");
                            }
                        }
                    }
                }

                logFile = $"{EditorUtility.PlatformBuildPath}/RefErr_{manifest.name}.txt";
                File.WriteAllText(logFile, builder.ToString());
                return succ;
            }
            else if (manifest.name == "Download" || manifest.name == "PackageRes")
            {
                int index = 0;
                var builder = new StringBuilder();
                
                builder.AppendLine(manifest.name);
                
                foreach (var i in assets)
                {
                    if (!i.path.StartsWith($"Assets/{manifest.name}") && !i.path.StartsWith("Packages"))
                    {
                        index++;
                        succ = false;
                        builder.AppendLine($"[{index}] {i.path}@{i.bundle}");
                        if (dependencyWithAssets.TryGetValue(i.path, out var refs))
                        {
                            for (var j = 0; j < refs.Count; j++)
                            {
                                var r = refs[j];
                                builder.AppendLine($"  - {j} {r.path}@{r.bundle}");
                            }
                        }
                    }
                }
                logFile = $"{EditorUtility.PlatformBuildPath}/RefErr_{manifest.name}.txt";
                File.WriteAllText(logFile, builder.ToString());
                return succ;
            }

            return true;
        }
        
        /// <summary>
        /// 检测资源重复打包到多个bundle
        /// </summary>
        public static void CheckAssetDuplicate()
        {
            var assetNameToBundles = new Dictionary<string, List<string>>();
            var settings = Settings.GetDefaultSettings();
            foreach (var manifest in settings.manifests)
            {
                var build = manifest.GetBuild();
                foreach (var bundle in build.bundles)
                {
                    var b = AssetBundle.LoadFromFile(Settings.GetBuildPath(bundle.nameWithAppendHash));
                    if (b == null)
                    {
                        continue;
                    }
                    
                    var allAssetNames = b.GetAllAssetNames();
                    foreach (var a in allAssetNames)
                    {
                        if (assetNameToBundles.TryGetValue(a, out var list))
                        {
                            list.Add(bundle.nameWithAppendHash);
                        }
                        else
                        {
                            assetNameToBundles.Add(a, new List<string>{bundle.nameWithAppendHash});
                        }
                    }
                
                    b.Unload(true);
                }
            }

            var builder = new StringBuilder();
            foreach (var i in assetNameToBundles)
            {
                if (i.Value.Count > 1)
                {
                    builder.AppendLine(i.Key);
                    builder.Append("\t").AppendLine(string.Join("\n\t", i.Value));
                }
            }
            File.WriteAllText(Path.Combine(EditorUtility.PlatformBuildPath, "asset_duplicate.txt"), builder.ToString());
        }
    }
}