using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VEngine.Editor
{
    /// <summary>
    ///     参与打包的资源，可以是单个文件，或文件夹
    /// </summary>
    [Serializable]
    public class Asset : IEquatable<Asset>, IComparable<Asset>
    {
        [SerializeField] private Object _target;

        /// <summary>
        ///     资源的 label，用来生成 bundle 的名字
        /// </summary>
        public string label;

        /// <summary>
        ///     资源的所有依赖
        /// </summary>
        public string[] dependencies = new string[0];

        /// <summary>
        ///     是否是只读的内容，例如 children
        /// </summary>
        public bool readOnly;

        /// <summary>
        ///     资源的路径
        /// </summary>
        public string path = string.Empty;

        /// <summary>
        ///     跟节点的路径
        /// </summary>
        public string rootPath;

        /// <summary>
        ///     打包的分组
        /// </summary>
        public Group parentGroup;

        /// <summary>
        ///     获取 Bundle 的名字
        /// </summary>
        public string bundle;

        /// <summary>
        ///     自定义打包器，可以按自己的喜好为资源打包，相同名字的资源会打包到一起。
        /// </summary>
        public static Func<Asset, string> customPacker { get; set; }

        /// <summary>
        ///     包含依赖的大小
        /// </summary>
        public ulong size { get; set; }


        /// <summary>
        ///     在 Assets 下的目标对象
        /// </summary>
        public Object target
        {
            get
            {
                if (_target == null) _target = AssetDatabase.LoadAssetAtPath<Object>(path);
                return _target;
            }
        }

        /// <summary>
        ///     是否是文件夹，文件夹需要采集子文件，但本身不参与打包。
        /// </summary>
        public bool isFolder => Directory.Exists(path);

        /// <summary>
        ///     资源是否已经修改
        /// </summary>
        public bool dirty { get; set; }

        public string metaPath => AssetDatabase.GetTextMetaFilePathFromAssetPath(path);

        public int CompareTo(Asset other)
        {
            return string.Compare(path, other.path, StringComparison.Ordinal);
        }

        public bool Equals(Asset other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (ReferenceEquals(this, other)) return true;

            return path == other.path;
        }

        public string PackWithBundleMode()
        {
            if (parentGroup.bundleMode == BundleMode.PackByRaw) return path;
            if (customPacker != null) return $"{customPacker(this)}{Settings.GetDefaultSettings().bundleExtension}";
            return PackWithDefault() + Settings.GetDefaultSettings().bundleExtension;
        }

        private string PackWithDefault()
        {
            if (parentGroup == null) return "invalid";
            var groupName = $"{parentGroup.manifest.name}_{parentGroup.name}";
            var isScene = path.EndsWith(".unity") || path.EndsWith(".lighting");
            if (isScene)
            {
                var assetName = Path.GetFileNameWithoutExtension(path);
                return $"{groupName}_scenes_{assetName}".ToLower();
            }

            var bundleName = string.Empty;
            if (isFolder) return bundleName;

            switch (parentGroup.bundleMode)
            {
                case BundleMode.PackTogether:
                    bundleName = PackTogether(groupName);
                    break;
                case BundleMode.PackByEntry:
                    bundleName = PackByEntry(groupName);
                    break;
                case BundleMode.PackByLabel:
                    bundleName = PackByLabel(groupName);
                    break;
                case BundleMode.PackByTopSubDirectoryOnly:
                    bundleName = PackByTopSubDirectoryOnly(groupName);
                    break;
                case BundleMode.PackByDirectory:
                    bundleName = PackByDirectory(groupName);
                    break;
                case BundleMode.PackByFile:
                    bundleName = PackByFile(groupName);
                    break;
                default:
                    bundleName = $"{groupName}_default";
                    break;
            }

            if (string.IsNullOrEmpty(bundleName)) return "invalid";
            bundleName = bundleName.Replace("\\", "/").Replace("/", "_").Replace(".", "_").Replace(" ", "_");
            bundleName = bundleName.ToLower();
            return bundleName;
        }

        /// <summary>
        ///     是否 需要排除 path 对应的文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Exclude(string path)
        {
            if (path.EndsWith(".csv"))
            {
                return false;
            }

            return Array.Exists(Settings.GetDefaultSettings().excludeFiles,
                match: path.Contains);
        }

        public static string[] GetChildren(string dir)
        {
            if (!Directory.Exists(dir)) return new string[0];

            var files = new List<string>(Directory.GetFiles(dir, "*", SearchOption.AllDirectories));
            for (var index = 0; index < files.Count; index++)
            {
                var file = files[index].Replace("\\", "/");
                files[index] = file;
                if (Exclude(file) || Directory.Exists(file))
                {
                    files.RemoveAt(index);
                    index--;
                }
            }

            return files.ToArray();
        }

        private static string PackTogether(string groupName)
        {
            return $"{groupName}_all_assets";
        }

        private string PackByEntry(string groupName)
        {
            var assetName = readOnly ? rootPath : path;
            if (Directory.Exists(assetName))
            {
                var info = new DirectoryInfo(assetName);
                assetName = info.Name;
            }
            else
            {
                assetName = Path.GetFileNameWithoutExtension(assetName);
            }

            return $"{groupName}_{assetName}";
        }

        private string PackByLabel(string groupName)
        {
            if (string.IsNullOrEmpty(label)) return $"{groupName}_default";
            return $"{groupName}_{label}";
        }

        private string PackByDirectory(string groupName)
        {
            var assetName = Path.GetFileName(rootPath);
            var directoryPath = !string.IsNullOrEmpty(rootPath) ? path.Substring(rootPath.Length + 1) : path;
            var directoryName = Path.GetDirectoryName(directoryPath);
            if (string.IsNullOrEmpty(directoryName)) return $"{groupName}_{assetName}";
            if (string.IsNullOrEmpty(assetName)) return $"{groupName}_{directoryName}";
            return $"{groupName}_{assetName}_{directoryName}";
        }

        private string PackByTopSubDirectoryOnly(string groupName)
        {
            if (string.IsNullOrEmpty(rootPath)) return $"{groupName}_all_assets";
            var assetName = Path.GetFileName(rootPath);
            var directoryName = Path.GetDirectoryName(path.Substring(rootPath.Length + 1));
            if (string.IsNullOrEmpty(directoryName)) return $"{groupName}_{assetName}";
            directoryName = directoryName.Replace("\\", "/");
            var pos = directoryName.IndexOf("/", StringComparison.Ordinal);
            if (pos != -1) directoryName = directoryName.Substring(0, pos);

            return $"{groupName}_{assetName}_{directoryName}";
        }

        private string PackByFile(string groupName)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                var directoryName = Path.GetDirectoryName(path);
                directoryName = $"{directoryName}_{Path.GetFileNameWithoutExtension(path)}";
                return $"{groupName}_{directoryName}";
            }
            else
            {
                var assetName = Path.GetFileName(rootPath);
                var directoryName = Path.GetDirectoryName(path.Substring(rootPath.Length + 1));
                directoryName = $"{directoryName}_{Path.GetFileNameWithoutExtension(path)}";
                return $"{groupName}_{assetName}_{directoryName}";
            }
        }

        public static Asset Create(string path, Group group, string label = null,
            string rootPath = null)
        {
            var asset = new Asset
            {
                label = label,
                path = path,
                parentGroup = group,
                rootPath = rootPath
            };
            asset.bundle = asset.PackWithBundleMode();
            return asset;
        }


        /// <summary>
        ///     获取资源的所有依赖，不包括自己
        /// </summary>
        /// <returns></returns>
        public string[] GetDependencies()
        {
            var items = new List<string>();
            items.AddRange(isFolder
                ? AssetDatabase.GetDependencies(GetChildren(), true)
                : AssetDatabase.GetDependencies(path, true));
            for (var index = 0; index < items.Count; index++)
            {
                var dependency = items[index];
                if (dependency == path
                    || Directory.Exists(dependency)
                    || Exclude(dependency) ||
                    isFolder &&
                    isChild(dependency))
                {
                    items.RemoveAt(index);
                    index--;
                }
            }

            items.Sort();
            dependencies = items.ToArray();
            return dependencies;
        }

        public bool isChild(string file)
        {
            return file.Contains(path);
        }

        /// <summary>
        ///     获取子文件-递归。
        /// </summary>
        /// <returns></returns>
        public string[] GetChildren()
        {
            return GetChildren(path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == GetType() && Equals((Asset) obj);
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
    }
}