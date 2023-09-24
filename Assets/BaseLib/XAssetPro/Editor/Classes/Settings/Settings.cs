﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VEngine.Editor
{
    /// <summary>
    ///     编辑器运行模式，主要包含: 1.Simulation 可以跳过打包快速运行；2.Preload 需要打包，但是不触发更新；3.Incremental 需要打包会触发更新，资源要复制到 StreamingAssets
    /// </summary>
    public enum ScriptPlayMode
    {
        /// <summary>
        ///     仿真模式，使用 AssetDatabase 加载资源
        /// </summary>
        Simulation,

        /// <summary>
        ///     预加载模式，始终是最新资源。
        /// </summary>
        Preload,

        /// <summary>
        ///     增量模式，从 StreamingAssets 加载已经打包后的资源，会触发更新
        /// </summary>
        Incremental
    }

    /// <summary>
    ///     所有资源的配置内容。
    /// </summary>
    public sealed class Settings : ScriptableObject
    {
        /// <summary>
        ///     设置相关文件的数据目录
        /// </summary>
        public const string dataPath = Constants.EDITOR_DATA_PATH;

        /// <summary>
        ///     编辑器分组数据的保存目录
        /// </summary>
        public static readonly string groupsDataPath = GetDataPath("Groups");

        /// <summary>
        ///     不参与打包的文件
        /// </summary>
        [Tooltip("不参与打包的文件")] public string[] excludeFiles =
        {
            ".cs", ".dll", ".spriteatlas", ".giparams", "LightingData.asset", ".meta"
        };

        /// <summary>
        ///     AssetBundle 打包扩展名
        /// </summary>
        [Tooltip("AssetBundle 打包扩展名")] public string bundleExtension = ".bundle";

        /// <summary>
        ///     播放器程序的版本号，可以手动填写
        /// </summary>
        [Tooltip("播放器程序的版本号，可以手动填写")] public int version;

        /// <summary>
        ///     所有清单，分布式打包的基础配置
        /// </summary>
        [Tooltip("所有清单")] public List<Manifest> manifests = new List<Manifest>();

        /// <summary>
        ///     此清单中的资源，在登陆时不下载更新，进入游戏在后台下载
        /// </summary>
        [Tooltip("后台下载清单")] public Manifest bkgroundManifest;
        
                
        /// <summary>
        ///     此清单中的资源，随包更新，不热更
        /// </summary>
        [Tooltip("不热更的清单")] public Manifest packageResManifest;
        
        /// <summary>
        ///     渠道资源分组配置
        /// </summary>
        [Tooltip("播放器资源分组配置")] public List<PlayerGroups> playerGroups = new List<PlayerGroups>();

        /// <summary>
        ///     当前打包播放器的分组索引
        /// </summary>
        [Tooltip("当前打包播放器的分组索引")] public int buildPlayerGroupsIndex;

        /// <summary>
        ///     编辑器运行模式，主要包含: 1.Simulation 可以跳过打包快速运行；2.Preload 需要打包，但是不触发更新；3.Incremental 需要打包会触发更新，资源要复制到 StreamingAssets
        /// </summary>
        [Tooltip(
            "编辑器运行模式，主要包含: 1.Simulation 可以跳过打包快速运行；2.Preload 需要打包，但是不触发更新；3.Incremental 需要打包会触发更新，资源要复制到 StreamingAssets")]
        public ScriptPlayMode scriptPlayMode = ScriptPlayMode.Simulation;

        /// <summary>
        ///     开启后不递归显示子节点
        /// </summary>
        [Tooltip("开启后不递归显示子节点")]
        public bool showEntryWithTopOnly;

        /// <summary>
        ///     获取文件相对打包目录的路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetBuildPath(string file)
        {
            return $"{EditorUtility.PlatformBuildPath}/{file}";
        }


        /// <summary>
        ///     将版本的资源根据配置复制到 StreamingAssets
        /// </summary>
        public void CopyToStreamingAssets()
        {
            var destinationDir = EditorUtility.BuildPlayerDataPath;
            if (Directory.Exists(destinationDir))
            {
                Directory.Delete(destinationDir, true);
            }
            Directory.CreateDirectory(destinationDir);
            
            var bundleAssets = new List<string>();
            var buildPath = EditorUtility.PlatformBuildPath;
            var bundles = Directory.GetFiles(buildPath, "*" + GetDefaultSettings().bundleExtension);
            if (bundles != null && bundles.Length > 0)
            {
                bundleAssets.AddRange(bundles.Select(i => Path.GetFileName(i)));
            }

            foreach (var manifest in manifests)
            {
                var manifestName = manifest.name.ToLower();
                var manifestVersionName = VEngine.Manifest.GetVersionFile(manifestName);
                bundleAssets.Add(manifestName);
                bundleAssets.Add(manifestVersionName);
            }

            var config = GetPlayerSettings();
            config.assets.Clear();
            foreach (var bundle in bundleAssets)
            {
                var destFile = Path.Combine(EditorUtility.BuildPlayerDataPath, bundle);
                var srcFile = GetBuildPath(bundle);
                if (!File.Exists(srcFile))
                {
                    Logger.E("Bundle not found: {0}", bundle);
                    continue;
                }

                var dir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(srcFile, destFile, true);
                if (Application.isBatchMode)
                {
                    Debug.Log($"CopyToStreamingAssets: Copy {srcFile} to {destFile}");
                }
                config.assets.Add(bundle);
            }
            
            config.manifests = manifests.ConvertAll(m => m.name);
            EditorUtility.SaveAsset(config);
        }

        public string playerSettingsResources = "Assets/Resources/PlayerSettings.asset";

        public PlayerSettings GetPlayerSettings()
        {
            return EditorUtility.GetOrCreateAsset<PlayerSettings>(playerSettingsResources);
        }

        /// <summary>
        ///     获取默认的版本数据
        /// </summary>
        /// <returns></returns>
        public static Settings GetDefaultSettings()
        {
            return EditorUtility.GetOrCreateAsset<Settings>(GetDataPath("DefaultSettings.asset"));
        }

        /// <summary>
        ///     获取 file 的数据路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetDataPath(string file)
        {
            return $"{dataPath}/{file}";
        }

        /// <summary>
        ///     获取文件的修改时间
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static long GetLastWriteTime(string file)
        {
            if (Directory.Exists(file))
            {
                return Directory.GetLastWriteTime(file).Ticks;
            }

            return File.Exists(file) ? File.GetLastWriteTime(file).Ticks : 0;
        }


        /// <summary>
        ///     保存序列化文件
        /// </summary>
        public void Save(bool clear = false)
        {
            if (clear)
            {
                if (Directory.Exists(EditorUtility.PlatformBuildPath))
                {
                    Directory.Delete(EditorUtility.PlatformBuildPath, true);
                }
            }

            foreach (var manifest in manifests)
            {
                manifest.Save(clear);
            }

            EditorUtility.SaveAsset(this);
        }

        /// <summary>
        ///     清理缓存数据
        /// </summary>
        public void Clear()
        {
            Save(true);
        }

        /// <summary>
        ///     获取文件相对分组数据的路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetGroupsDataPath(string file)
        {
            return $"{groupsDataPath}/{file}";
        }


        public int AddManifest(string path)
        {
            var asset = EditorUtility.GetOrCreateAsset<Manifest>(path);
            asset.settings = this;
            manifests.Add(asset);
            return manifests.Count - 1;
        }
    }
}