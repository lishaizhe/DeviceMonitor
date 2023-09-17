using System.Collections.Generic;
using UnityEngine;

namespace VEngine.Editor
{
    /// <summary>
    ///     打包模式
    /// </summary>
    public enum BundleMode
    {
        /// <summary>
        ///     分组中的除了场景外的所有资源按分组的名字一起打包，每个场景会按 分组名字 + 场景名字 进行打包。
        /// </summary>
        PackTogether,

        /// <summary>
        ///     资源按分组名字 + 资源一级节点名字打包
        /// </summary>
        PackByEntry,

        /// <summary>
        ///     资源按目录打包。
        /// </summary>
        PackByDirectory,

        /// <summary>
        ///     同一个分组相同 label 的资源会打包到一个 AssetBundle
        /// </summary>
        PackByLabel,

        /// <summary>
        ///     按原始格式分组
        /// </summary>
        PackByRaw,

        /// <summary>
        ///     资源按文件。
        /// </summary>
        PackByFile,

        /// <summary>
        ///     资源按顶级目录及其一级子目录归集 。
        /// </summary>
        PackByTopSubDirectoryOnly,
    }

    /// <summary>
    ///     打包资源的自定义分组
    /// </summary>
    public class Group : ScriptableObject
    {
        /// <summary>
        ///     分组的打包模式，决定了分组资源的打包粒度
        /// </summary>
        public BundleMode bundleMode = BundleMode.PackTogether;

        /// <summary>
        ///     分组所在的清单
        /// </summary>
        public Manifest manifest;

        /// <summary>
        ///     分组中的所有资源
        /// </summary>
        [Tooltip("所有打包的资源，可以是文件加或文件")] public List<Asset> assets = new List<Asset>();

        /// <summary>
        ///     是否只读，自动分组默认会开启这个选项
        /// </summary>
        [Tooltip("分组是否只读，自动分组默认会开启这个选项")] public bool readOnly;

        /// <summary>
        ///     分组资源打包后包含依赖的字节大小
        /// </summary>
        public ulong size { get; set; }
    }
}