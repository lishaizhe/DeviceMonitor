using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using UnityEngine;

namespace VEngine
{
    /// <summary>
    ///     资源清单，记录了所有要加载的资源的寻址信息和依赖关系。
    /// </summary>
    public class Manifest
    {
        private const string key_version = "[Version]";
        private const string key_app_version = "[AppVersion]";
        private const string key_groups = "[Groups]";
        private const string key_paths = "[Paths]";
        private const string key_directories = "[Directories]";
        private const string key_bundles = "[Bundles]";
        private const string key_assets = "[Assets]";

        private static readonly HashSet<string> all_keys = new HashSet<string>
        {
            key_version,
            key_app_version,
            key_groups,
            key_paths,
            key_directories,
            key_bundles,
            key_assets
        };

        public Action<string> onReadAsset;

        /// <summary>
        ///     所有资源路径
        /// </summary>
        internal readonly List<string> allAssetPaths = new List<string>(16000);

        /// <summary>
        ///     所有资源的目录
        /// </summary>
        private readonly List<string> directories = new List<string>(1500);

        /// <summary>
        ///     按 bundle 名字关联运行时信息
        /// </summary>
        private readonly Dictionary<string, BundleInfo> nameWithBundles = new Dictionary<string, BundleInfo>(2000);

        /// <summary>
        ///     按 group 名字关联运行时信息
        /// </summary>
        private readonly Dictionary<string, GroupInfo> nameWithGroups = new Dictionary<string, GroupInfo>();

        /// <summary>
        ///     按 asset 名字关联运行时信息
        /// </summary>
        internal readonly Dictionary<string, AssetInfo> pathWithAssets = new Dictionary<string, AssetInfo>(16000);

        /// <summary>
        ///     所有 asset 的运行时信息
        /// </summary>
        public List<AssetInfo> assets = new List<AssetInfo>(16000);

        /// <summary>
        ///     所有 bundle 的运行时信息
        /// </summary>
        public List<BundleInfo> bundles = new List<BundleInfo>(2000);

        /// <summary>
        ///     所有 group 的运行时信息
        /// </summary>
        public List<GroupInfo> groups = new List<GroupInfo>();

        /// <summary>
        ///     版本号
        /// </summary>
        public int version;

        public string appVersion;

        public string name { get; set; }

        public int id { get; set; }

        /// <summary>
        ///     所有版本内的资源路径
        /// </summary>
        public string[] AllAssetPaths => allAssetPaths.ToArray();
        
        // 这里做一个lastAsset的缓存
        private AssetInfo lastAssetInfo = null;
        private int lastAssetInfoBundlesLength = 0;
        // 这里做一个全局sb！
        private StringBuilder sb = null;

        public void Load(string path)
        {
            pathWithAssets.Clear();
            nameWithBundles.Clear();
            nameWithGroups.Clear();
            allAssetPaths.Clear();
            assets.Clear();
            bundles.Clear();
            groups.Clear();
            directories.Clear();

            if (!File.Exists(path))
            {
                return;
            }

            ParseManifest(path);
            return;

            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                string line;
                var parseType = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (all_keys.Contains(line))
                    {
                        parseType = line;
                        continue;
                    }

                    switch (parseType)
                    {
                        case key_version:
                            ReadVersion(line);
                            break;
                        case key_app_version:
                            ReadAppVersion(line);
                            break;
                        case key_paths:
                            ReadPath(line);
                            break;
                        case key_bundles:
                            ReadBundle(line);
                            break;
                        case key_assets:
                            ReadAsset(line);
                            break;
                        case key_directories:
                            ReadDirectory(line);
                            break;
                        case key_groups:
                            ReadGroups(line);
                            break;
                    }
                }
            }
        }
        
           // 解析；快速且不产生GC
        private void ParseManifest(string path)
        {
            var parseType = string.Empty;
            sb = new StringBuilder(256);
            
            using (StreamReader sr = File.OpenText(path))
            {
                // https://adamsitnik.com/Array-Pool/
                var samePool = ArrayPool<char>.Shared;
                char[] fileBuffer = samePool.Rent(8192*3);

                try
                {
                    foreach (ReadOnlySpan<char> line in sr.SplitLines(fileBuffer))
                    {
                        //we're just testing read speeds
                        ParseManifestLine(line, ref parseType);
                    }
                }
                finally
                {
                    samePool.Return(fileBuffer);   
                }
            }
            
            sb = null;
            
            // #if UNITY_EDITOR
            // Log.Error("read ok!!!!");
            // #endif
            return;
        }

        private void ParseManifestLine(ReadOnlySpan<char> line, ref string parseType)
        {
            try
            {
                if (line.IsEmpty || line[0] == '#')
                {
                    return;
                }

                if (line.Length >= 2 && line[0] == '/' && line[1] == '/')
                {
                    return;
                }

                // 文件中是没有多余空格的，他之前的判断也是原始字符串直接判断的
                if (line[0] == '[')
                {
                    ReadOnlySpan<char> ok_line = line.Trim();

                    bool found = false;
                    foreach (var k in all_keys)
                    {
                        // 注意readonlyspan和字符串比较，不能直接使用==；必须使用如下形式
                        if (k.AsSpan().SequenceEqual(ok_line))
                        {
                            parseType = k;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        return;
                    }

                    // 什么情况能走到这里？
#if UNITY_EDITOR
                    Log.Error("what a happened for run here?!");
#endif
                }

                switch (parseType)
                {
                    case key_version:
                        ReadVersion(line);
                        break;
                    case key_app_version:
                        ReadAppVersion(line);
                        break;
                    case key_paths:
                        ReadPath(line);
                        break;
                    case key_bundles:
                        ReadBundle(line);
                        break;
                    case key_assets:
                        ReadAsset(line);
                        break;
                    case key_directories:
                        ReadDirectory(line);
                        break;
                    case key_groups:
                        ReadGroups(line);
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Error("parser error!!!!");
            }

            return;
        }

        private void ReadAppVersion(string line)
        {
            appVersion = line;
        }
        private void ReadAppVersion(ReadOnlySpan<char> line)
        {
            appVersion = line.ToString();
        }

        private void ReadVersion(string line)
        {
            version = line.IntValue();
        }
        private void ReadVersion(ReadOnlySpan<char> line)
        {
            version = line.ToInt();
        }

        private void ReadPath(string line)
        {
            var fields = line.Split(',');
            var dir = fields[1].IntValue();
            var file = fields[2];
            if (dir >= 0 && dir < directories.Count)
            {
                allAssetPaths.Add($"{directories[dir]}/{file}");
            }
            else
            {
                allAssetPaths.Add(file);
            }
        }
        private void ReadPath(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            ReadOnlySpan<char> fields_2;
            line.Split_to_spans(',', out fields_0, out fields_1, out fields_2);

            var dir = fields_1.ToInt();
            // var file = fields_2.ToString();
            if (dir >= 0 && dir < directories.Count)
            {
                sb.Clear();
                sb.Append(directories[dir]);
                sb.Append("/");
                // 因为目前Unity版本的C#不支持加入readonlyspan（官方已支持）；所以暂时如此
                for (int i = 0; i < fields_2.Length; ++i)
                {
                    sb.Append(fields_2[i]);
                }
                allAssetPaths.Add(sb.ToString());
                // allAssetPaths.Add($"{directories[dir]}/{file}");
            }
            else
            {
                var file = fields_2.ToString();
                allAssetPaths.Add(file);
            }
        }

        private void ReadGroups(string line)
        {
            var group = new GroupInfo();
            group.Deserialize(line);
            groups.Add(group);
            nameWithGroups.Add(group.name, group);
        }
        private void ReadGroups(ReadOnlySpan<char> line)
        {
            var group = new GroupInfo();
            group.Deserialize(line);
            groups.Add(group);
            nameWithGroups.Add(group.name, group);
        }
        
        private void ReadDirectory(string line)
        {
            directories.Add(line.Split(new[]
            {
                ','
            }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        private void ReadDirectory(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            line.Split_to_spanspan(',', out fields_0, out fields_1);
            directories.Add(fields_1.ToString());
        }

        private void ReadBundle(string line)
        {
            var bundle = new BundleInfo();
            bundle.Deserialize(line);
            nameWithBundles[bundle.name] = bundle;
            bundles.Add(bundle);
        }
        private void ReadBundle(ReadOnlySpan<char> line)
        {
            var bundle = new BundleInfo();
            bundle.Deserialize(line);
            nameWithBundles[bundle.name] = bundle;
            bundles.Add(bundle);
        }

        private void ReadAsset(string line)
        {
            var asset = new AssetInfo();
            asset.Deserialize(line);
            
            assets.Add(asset);
            var assetPath = allAssetPaths[asset.id];
            pathWithAssets[assetPath] = asset;
            if (onReadAsset != null)
            {
                onReadAsset(assetPath);
            }
        }
        private void ReadAsset(ReadOnlySpan<char> line)
        {
            var asset = new AssetInfo();

            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            ReadOnlySpan<char> fields_2;
            var fields = line.Split_to_spans(',', out fields_0, out fields_1, out fields_2);
            asset.id = fields_0.ToInt();
            asset.bundle = fields_1.ToInt();

            // 读取asset这地方处理可以做一个优化
//             if (lastAssetInfo != null && lastAssetInfo.bundle == asset.bundle && lastAssetInfoBundlesLength == fields_2.Length)
//             {
//                 asset.bundles = lastAssetInfo.bundles;
// // #if UNITY_EDITOR
// //                 int[] b = fields_2.Split_to_IntArray('|');
// //                 if (!Enumerable.SequenceEqual(asset.bundles, b))
// //                 {
// //                     int ok = 0;
// //                 }
// // #endif
//             }
            // else
            {
                if (fields_2.IsEmpty)
                {
                    asset.bundles = Utility.IntArrayEmpty;
                }
                else
                {
                    asset.bundles = fields_2.Split_to_IntArray('|');
                }
            }

            lastAssetInfo = asset;
            lastAssetInfoBundlesLength = fields_2.Length;
            
            // asset.Deserialize(line);
            assets.Add(asset);
            var assetPath = allAssetPaths[asset.id];
            pathWithAssets[assetPath] = asset;
            if (onReadAsset != null)
            {
                onReadAsset(assetPath);
            }
        }

        public void AddAsset(string path)
        {
            var asset = new AssetInfo
            {
                id = assets.Count
            };
            assets.Add(asset);
            allAssetPaths.Add(path);
            pathWithAssets[path] = asset;
            if (onReadAsset != null)
            {
                onReadAsset(path);
            }
        }

        public string Save(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var writer = new StreamWriter(File.OpenWrite(path)))
            {
                WriteVersion(writer);
                //WriteAppVersion(writer);
                WriteDirectories(writer);
                WriteAssetPaths(writer);
                WriteAssets(writer);
                WriteBundles(writer);
                WriteGroups(writer);
            }

            return SaveVersion(path);
        }

        private void WriteAssetPaths(TextWriter writer)
        {
            directories.Clear();
            writer.WriteLine(key_paths);
            foreach (var assetPath in allAssetPaths)
            {
                writer.WriteLine(assetPath);
            }

            writer.WriteLine();
        }

        private void WriteVersion(StreamWriter writer)
        {
            writer.WriteLine(key_version);
            writer.WriteLine(version);
            writer.WriteLine();
        }

        // private void WriteAppVersion(StreamWriter writer)
        // {
        //     writer.WriteLine(key_app_version);
        //     writer.WriteLine(appVersion);
        //     writer.WriteLine();
        // }

        private void WriteDirectories(StreamWriter writer)
        {
            writer.WriteLine(key_directories);
            directories.Clear();
            var directoryWithIDs = new Dictionary<string, int>();
            for (var index = 0; index < allAssetPaths.Count; index++)
            {
                var assetPath = allAssetPaths[index];
                var directoryName = Path.GetDirectoryName(assetPath);
                var file = Path.GetFileName(assetPath);
                var dir = -1;
                if (string.IsNullOrEmpty(directoryName))
                {
                    allAssetPaths[index] = $"{index},{dir},{file}";
                    continue;
                }

                directoryName = directoryName.Replace('\\', '/');
                if (!directoryWithIDs.TryGetValue(directoryName, out dir))
                {
                    dir = directories.Count;
                    directoryWithIDs.Add(directoryName, dir);
                    directories.Add(directoryName);
                    writer.WriteLine($"{dir},{directoryName}");
                }

                allAssetPaths[index] = $"{index},{dir},{file}";
            }

            writer.WriteLine();
        }

        private void WriteAssets(StreamWriter writer)
        {
            writer.WriteLine(key_assets);
            foreach (var asset in assets)
            {
                writer.WriteLine(asset.Serialize());
            }

            writer.WriteLine();
        }

        private void WriteGroups(StreamWriter writer)
        {
            writer.WriteLine(key_groups);
            foreach (var info in groups)
            {
                writer.WriteLine(info.Serialize());
            }
        }

        private void WriteBundles(StreamWriter writer)
        {
            writer.WriteLine(key_bundles);
            foreach (var bundle in bundles)
            {
                writer.WriteLine(bundle.Serialize());
            }

            writer.WriteLine();
        }

        public static string GetVersionFile(string name)
        {
            return $"{name}.version";
        }

        private string SaveVersion(string path)
        {
            var file = Path.GetFileName(path);
            var outputFolder = Path.GetDirectoryName(path);
            string newName;
            using (var stream = File.OpenRead(path))
            {
                var crc = Utility.ComputeCRC32(stream);
                var versionFile = $"{outputFolder}/{GetVersionFile(file)}";
                if (File.Exists(versionFile))
                {
                    var text = File.ReadAllText(versionFile);
                    var fields = text.Split(',');
                    var lastCRC = fields[2].UIntValue();
                    if (lastCRC.Equals(crc))
                    {
                        Logger.I("Version not changed.");
                    }
                    File.Delete(versionFile);
                }
                newName = $"{file}_v{version}";
                var content = $"{version},{stream.Length},{crc}";
                File.WriteAllText(versionFile, content);
                //File.Copy(versionFile, versionFile.Replace(file, newName), true);
                //File.Copy(path, path.Replace(file, newName), true);
            }
            return newName;
        }

        public BundleInfo GetBundle(string assetBundleName)
        {
            nameWithBundles.TryGetValue(assetBundleName, out var bundle);
            return bundle;
        }

        public string GetBundleNameAppendHash(string nameWithoutHash)
        {
            var info = bundles.Find(b => b.name.StartsWith(nameWithoutHash));
            if (info != null)
                return info.name;
            return string.Empty;
        }

        public bool ContainsBundle(string assetBundleName)
        {
            return nameWithBundles.ContainsKey(assetBundleName);
        }

        public IEnumerable<BundleInfo> GetBundlesWithGroups(params string[] groupNames)
        {
            if (groupNames == null || groupNames.Length == 0)
            {
                return bundles.ToArray();
            }

            var set = new HashSet<BundleInfo>();
            foreach (var groupName in groupNames)
            {
                if (nameWithGroups.TryGetValue(groupName, out var groupInfo))
                {
                    foreach (var item in groupInfo.bundles)
                    {
                        var bundle = GetBundle(item);
                        if (bundle != null)
                        {
                            set.Add(bundle);
                        }
                    }
                }
            }

            return set;
        }

        public BundleInfo GetBundle(int bundleId)
        {
            if (bundleId >= 0 && bundleId < bundles.Count)
            {
                return bundles[bundleId];
            }

            return null;
        }

        public AssetInfo GetAsset(string path)
        {
            pathWithAssets.TryGetValue(path, out var asset);
            return asset;
        }

        public void SetAllAssetPaths(IEnumerable<string> assetPaths)
        {
            allAssetPaths.Clear();
            allAssetPaths.AddRange(assetPaths);
        }

        public BundleInfo[] GetBundles(AssetInfo info)
        {
            return Array.ConvertAll(info.bundles, GetBundle);
        }

        public BundleInfo[] GetDependencies(BundleInfo info)
        {
            return Array.ConvertAll(info.deps, GetBundle);
        }
    }
}