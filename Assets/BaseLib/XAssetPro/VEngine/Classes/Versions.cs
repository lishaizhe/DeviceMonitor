using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using UnityEngine;

namespace VEngine
{
    /// <summary>
    ///     Versions 类，持有包内和包外的版本信息，并提供版本内容的 初始化，更新，检查，下载等接口。
    /// </summary>
    public static class Versions
    {
        /// <summary>
        ///     运行时 API 的版本
        /// </summary>
        public const string APIVersion = "6.1.5";

        /// <summary>
        ///     运行时的清单文件，服务器的
        /// </summary>
        public static readonly List<Manifest> Manifests = new List<Manifest>();

        /// <summary>
        ///     按路径缓存的清单记录
        /// </summary>
        private static readonly Dictionary<string, Manifest> NameWithManifests = new Dictionary<string, Manifest>();

        /// <summary>
        ///     bundle 的加载地址缓存，可以优化 gc
        /// </summary>
        internal static readonly Dictionary<string, string> BundleWithPathOrUrLs = new Dictionary<string, string>();

        /// <summary>
        ///     短连接缓存
        /// </summary>
        private static readonly Dictionary<string, string> NameWithPaths = new Dictionary<string, string>();

        /// <summary>
        ///     跳过更新，开启后，只会在本地加载资源。
        /// </summary>
        public static bool SkipUpdate;

        public static bool IsSimulation;

        public static bool CheckWhiteList;

        /// <summary>
        ///     自定义文件下载地址代理，实现后，可以按需根据文件名返回与之对应的下载地址，返回空则使用默认地址。用法参考
        ///     <see>
        ///         <cref>VEngine.Example.CustomDownloadURL</cref>
        ///     </see>
        /// </summary>
        public static Func<string, string> getDownloadURL;

        public static Func<string, Type, Asset> FuncCreateAsset { get; set; }
        public static Func<string, bool, Scene> FuncCreateScene { get; set; }
        public static Func<string, bool, ManifestFile> FuncCreateManifest { get; set; }
        public static Func<string, bool> FuncIsAssetDownloaded { get; set; }

        /// <summary>
        ///     清单版本
        /// </summary>
        public static string ManifestsVersion
        {
            get
            {
                var sb = new StringBuilder();
                for (var index = 0; index < Manifests.Count; index++)
                {
                    var manifest = Manifests[index];
                    sb.Append(manifest.version);
                    if (index < Manifests.Count - 1)
                    {
                        sb.Append(".");
                    }
                }

                return sb.ToString();
            }
        }


        /// <summary>
        ///     包体内的资源目录
        /// </summary>
        public static string PlayerDataPath { get; set; }

        /// <summary>
        ///     资源下载路径
        /// </summary>
        public static string DownloadURL { get; set; }
        
        public static string CheckVersionURL { get; set; }

        /// <summary>
        ///     资源下载后保存的数据目录
        /// </summary>
        public static string DownloadDataPath { get; set; }

        /// <summary>
        ///     使用 UnityWebRequest 加载本地文件的时候的协议，在不同平台有不同的定义
        /// </summary>
        internal static string LocalProtocol { get; set; }

        public static string PlatformName { get; set; }

        /// <summary>
        ///     短连接代理用法参考
        ///     <see>
        ///         <cref>VEngine.Example.AddressableByName</cref>
        ///     </see>
        /// </summary>
        public static Func<string, string> customLoadPath { get; set; }

        private static List<string> PlayerAssets { get; set; }
        
        private static List<string> WhiteList { get; set; }

        public static readonly List<string> WhiteListFailed = new List<string>();

        /// <summary>
        ///     获取模块清单。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Manifest GetManifest(string name)
        {
            if (NameWithManifests.TryGetValue(name, out var manifest))
            {
                return manifest;
            }
            return null;
        }

        public static Asset CreateAsset(string path, Type type)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return FuncCreateAsset(path, type);
        }

        public static bool IsAssetDownloaded(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }
            return FuncIsAssetDownloaded(path);
        }

        public static Scene CreateScene(string path, bool additive)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            GetActualPath(ref path);
            return FuncCreateScene(path, additive);
        }

        public static ManifestFile CreateManifest(string name, bool builtin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            return FuncCreateManifest(name.ToLower(), builtin);
        }

        public static void OnReadAsset(string assetPath)
        {
            // 实现 addressableByName 为资源自动生成短连接映射
            if (customLoadPath == null)
            {
                return;
            }
            var loadPath = CustomLoadPath(assetPath);
            if (string.IsNullOrEmpty(loadPath))
            {
                return;
            }
            if (!NameWithPaths.TryGetValue(loadPath, out var address))
            {
                NameWithPaths[loadPath] = assetPath;
            }
            else // 有名字冲突
            {
                if (!address.Equals(assetPath))
                {
                    Logger.W($"{loadPath} already exist {address}");
                }
            }
        }

        public static void Override(Manifest target)
        {
            var key = target.name;
            if (NameWithManifests.TryGetValue(key, out var value))
            {
                if (value.version < target.version)
                {
                    target.id = value.id;
                    NameWithManifests[key] = target;
                    Manifests[value.id] = target;
                }
                return;
            }

            target.id = Manifests.Count;
            Manifests.Add(target);
            NameWithManifests.Add(key, target);
        }

        public static void GetActualPath(ref string path)
        {
            if (NameWithPaths.TryGetValue(path, out var value))
            {
                path = value;
            }
        }

        private static string CustomLoadPath(string assetPath)
        {
            return customLoadPath(assetPath);
        }

        /// <summary>
        ///     获取指定的 path 相对 <see cref="DownloadDataPath" /> 的完整路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetDownloadDataPath(string file)
        {
            return $"{DownloadDataPath}/{file}";
        }

        /// <summary>
        ///     获取指定的 path 相对 <see cref="DownloadDataPath" /> 的完整路径 由系统路径拼接
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetDownloadDataSystemPath(string file)
        {
            return Path.Combine(DownloadDataPath, file);
        }

        /// <summary>
        ///     获取 path 相对包体目录的用来给 UnityWebRequest 使用的 url
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetPlayerDataURL(string file)
        {
            return $"{LocalProtocol}{PlayerDataPath}/{file}";
        }

        /// <summary>
        ///     获取文件相对包体目录的路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetPlayerDataPath(string file)
        {
            return $"{PlayerDataPath}/{file}";
        }

        /// <summary>
        ///     获取 path 对应的 的下载路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetDownloadURL(string file)
        {
            if (getDownloadURL != null)
            {
                var url = getDownloadURL(file);
                if (!string.IsNullOrEmpty(url))
                {
                    return url;
                }
            }

            return $"{DownloadURL}{PlatformName}/{file}";
        }

        /// <summary>
        ///     获取临时路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetTemporaryPath(string file)
        {
            var ret = $"{Application.temporaryCachePath}/{file}";
            var dir = Path.GetDirectoryName(ret);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return ret;
        }

        /// <summary>
        ///     清理版本内容，目前只有测试的时候用到
        /// </summary>
        public static void ClearDownloadData()
        {
            if (Directory.Exists(DownloadDataPath))
            {
                Directory.Delete(DownloadDataPath, true);
                Directory.CreateDirectory(DownloadDataPath);
            }

            BundleWithPathOrUrLs.Clear();
        }

        /// <summary>
        ///     清理旧的版本内容，下载目录不在版本中的文件将被删除。
        /// </summary>
        /// <returns></returns>
        public static ClearVersions ClearAsync()
        {
            var clearAsync = new ClearVersions();
            clearAsync.Start();
            return clearAsync;
        }

        /// <summary>
        ///     运行时自动初始化，无需主动调用
        /// </summary>
        public static void InitializeOnLoad()
        {
            if (FuncCreateAsset == null)
            {
                FuncCreateAsset = BundledAsset.Create;
            }

            if (FuncIsAssetDownloaded == null)
            {
                FuncIsAssetDownloaded = BundledAsset.IsAssetDownloaded;
            }
            
            if (FuncCreateScene == null)
            {
                FuncCreateScene = BundledScene.Create;
            }

            if (FuncCreateManifest == null)
            {
                FuncCreateManifest = ManifestFile.Create;
            }

            if (Application.platform != RuntimePlatform.OSXEditor &&
                Application.platform != RuntimePlatform.OSXPlayer &&
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    LocalProtocol = "file:///";
                }
                else
                {
                    LocalProtocol = string.Empty;
                }
            }
            else
            {
                LocalProtocol = "file://";
            }

            if (string.IsNullOrEmpty(PlatformName))
            {
                PlatformName = Utility.GetPlatformName();
            }

            // 包体内的资源路径
            if (string.IsNullOrEmpty(PlayerDataPath))
            {
                PlayerDataPath = $"{Application.streamingAssetsPath}/{Utility.buildPath}";
            }

            // 更新下载路径
            if (string.IsNullOrEmpty(DownloadDataPath))
            {
                DownloadDataPath = $"{Application.persistentDataPath}/{Utility.buildPath}";
            }

            if (!Directory.Exists(DownloadDataPath))
            {
                Directory.CreateDirectory(DownloadDataPath);
            }
        }

        /// <summary>
        ///     加载包体的清单文件
        /// </summary>
        /// <returns></returns>
        public static InitializeVersions InitializeAsync()
        {
            Log.Debug("InitializeAsync");
            var settings = Resources.Load<PlayerSettings>("PlayerSettings");
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PlayerSettings>();
            }
            PlayerAssets = settings.assets;
            WhiteList = settings.whiteList;
            
            InitializeOnLoad();
            
            var operation = new InitializeVersions
            {
                manifests = settings.manifests.ToArray(),
            };
            operation.Start();
            return operation;
        }

        /// <summary>
        ///     更新版本的异步操作，可以通过协程或者 completed 事件等待更新的结果返回。
        /// </summary>
        /// <param name="manifests"></param>
        /// <returns></returns>
        public static UpdateVersions UpdateAsync(params string[] manifests)
        {
            var operation = new UpdateVersions
            {
                manifests = manifests
            };
            operation.Start();
            return operation;
        }

        /// <summary>
        ///     根据自定义分组检查更新，支持多个分组，不传默认检查所有文件的更新状态。此方法可以通过协程返回结果。
        /// </summary>
        /// <param name="groupNames">需要检查的分组名字</param>
        /// <returns>检查版本的对象，可以获取检查的进度和完成状态以及结果</returns>
        // public static GetDownloadSize GetDownloadSizeAsync(params string[] groupNames)
        // {
        //     return GetDownloadSizeAsync(GetDownloadSizeMode.Groups, groupNames);
        // }

        // public static GetDownloadSize GetDownloadSizeWithAssetsAsync(params string[] assetNames)
        // {
        //     return GetDownloadSizeAsync(GetDownloadSizeMode.Assets, assetNames);
        // }

        public static GetDownloadSize GetDownloadSizeAsync(GetDownloadSizeMode mode, VEngine.Manifest[] manifests, params string[] items)
        {
            var check = new GetDownloadSize
            {
                items = items,
                mode = mode,
                manifests = manifests
            };
            check.Start();
            return check;
        }

        /// <summary>
        ///     批量下载一组资源
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static DownloadVersions DownloadAsync(DownloadInfo[] groups)
        {
            var download = new DownloadVersions
            {
                groups = groups
            };
            download.Start();
            return download;
        }

        public static bool IsDownloaded(BundleInfo bundle)
        {
            if (SkipUpdate || PlayerAssets.Contains(bundle.name))
            {
                return true;
            }
            var file = new FileInfo(GetDownloadDataPath(bundle.name));
            return file.Exists && (ulong) file.Length == bundle.size;
        }

        public static bool IsChanged(string manifest)
        {
            var file = ManifestVersionFile.Load(GetDownloadDataPath(Manifest.GetVersionFile(manifest)));
            var find = Manifests.Find(m => manifest.Contains(m.name));
            if (find != null)
            {
                return find.version < file.version;
            }
            return true;
        }

        public static bool IsInWhiteList(string bundleName)
        {
            return WhiteList.Contains(bundleName);
        }

        internal static void SetBundlePathOrURl(string assetBundleName, string url)
        {
            BundleWithPathOrUrLs[assetBundleName] = url;
        }

        internal static string GetBundlePathOrURL(BundleInfo info)
        {
            var assetBundleName = info.name;
            // 看缓存中是否有
            if (BundleWithPathOrUrLs.TryGetValue(assetBundleName, out var path))
            {
                return path;
            }

            // 看包内是否有
            if (SkipUpdate || PlayerAssets.Contains(assetBundleName))
            {
                path = GetPlayerDataPath(assetBundleName);
                BundleWithPathOrUrLs[assetBundleName] = path;
                return path;
            }
           
            // 看包外是否有
            if (IsDownloaded(info))
            {
                path = GetDownloadDataPath(assetBundleName);
                BundleWithPathOrUrLs[assetBundleName] = path;
                return path;
            }
            
            Log.Error("bundle {0} not find in local return path {1}", assetBundleName, path);
            return path;
        }

        public static AssetInfo GetAsset(ref string path)
        {
            GetActualPath(ref path);

            foreach (var item in NameWithManifests)
            {
                var asset = item.Value.GetAsset(path);
                if (asset != null)
                {
                    return asset;
                }
            }

            return null;
        }

        public static bool GetDependencies(string assetPath, out BundleInfo bundle, out BundleInfo[] bundles)
        {
            for (var index = Manifests.Count - 1; index >= 0; index--)
            {
                var manifest = Manifests[index];
                var asset = manifest.GetAsset(assetPath);
                if (asset != null)
                {
                    bundle = manifest.GetBundle(asset.bundle);
                    bundles = manifest.GetDependencies(bundle);
                    return true;
                }
            }
            bundle = null;
            bundles = null;
            return false;
        }

        public static List<BundleInfo> GetBundlesWithAssets(VEngine.Manifest[] manifests, string[] assetNames)
        {
            var bundles = new List<BundleInfo>();
            if (manifests != null)
            {
                foreach (var manifest in manifests)
                {
                    foreach (var assetName in assetNames)
                    {
                        var asset = manifest.GetAsset(assetName);
                        var updateBundles = manifest.GetBundles(asset);
                        foreach (var bundle in updateBundles)
                        {
                            if (PlayerAssets.Contains(bundle.name))
                            {
                                continue;
                            }

                            bundles.Add(bundle);
                        }
                    }
                }
            }

            return bundles;
        }

        public static List<BundleInfo> GetBundlesWithGroups(VEngine.Manifest[] manifests, string[] groupsNames)
        {
            var bundles = new List<BundleInfo>();
            if (manifests != null)
            {
                foreach (var manifest in manifests)
                {
                    var updateBundles = manifest.GetBundlesWithGroups(groupsNames);
                    foreach (var bundle in updateBundles)
                    {
                        if (PlayerAssets.Contains(bundle.name))
                        {
                            continue;
                        }
                        bundles.Add(bundle);
                    }
                }
            }

            return bundles;
        }

        /// <summary>
        ///     获取所有资源路径
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllAssetPaths()
        {
            var set = new HashSet<string>();
            foreach (var manifest in Manifests)
            {
                set.UnionWith(manifest.AllAssetPaths);
            }

            return set.ToArray();
        }
    }
}