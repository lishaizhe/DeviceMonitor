﻿

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using System.Text;
using GameFramework;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using UnityGameFramework.Runtime;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GameKit.Base
{
    public enum MemeryHold
    {
        Normal, // 自己调用释放
        Always, // 在关闭游戏的时候释放
    }
    public class ResourceManager : SingletonBehaviour<ResourceManager>
    {
//         [System.Serializable]
//         public class AssetCache
//         {
//             public string assetName;            // AssetBundle中的资源名字
//             public string assetbundleName;      // AssetBundle的名字
//             public string assetbundleVariant;   // 实际加载的AssetBundle的变体的名字
//             public System.Type type;            // 资源类型
//             public MemeryHold hold;             // 内存持久类型
//             public int refCount;                // 被引用次数
//
//             public object obj;                  // 缓存中的资源对象
//
//             public void Release()
//             {
//                 if (obj is GameObject prefab)
//                 {
//                     prefab.RecycleAll();
//                     prefab.DestroyPooled();
//                 }
//
//                 obj = null;
//
//                 if (!string.IsNullOrEmpty(assetbundleVariant))
//                     AssetBundleManager.UnloadAssetBundle(assetbundleVariant);
//             }
//         }
//
//         private const string AssetKeyFormat = "{0}@{1}@{2}";
//
//         private bool IsInited;               
//         private AssetBundleLoadManifestOperation m_AssetBundleLoadManifestOperation;    // Manifest读取进程
//
//         public delegate void OnLoadComplete(string key, object asset, string err);
//
// #if ODIN_INSPECTOR
//         [ShowInInspector, ShowIf("showOdinInfo")]
// #endif
//         public int RemoveAssetDelay
//         {
//             get;
//             //set;
//         } = 10;
//
//         #region release begin
//         // 资源释放结构体（自动释放和延迟释放共用）
//         // 自动释放指每次判断，如果数据为null自动删除
//         // 延迟释放指接口调用后几秒后删除
//         class ReleaseInfo
//         {
//             public string key;
//             public int delay;
//             public GameObject go;  // 这个地方必须是GameObject，需要依赖Unity的判空规则
//         }
//
//         // 资源释放结构体池（使用pool，否则gc太多）
//         private List<ReleaseInfo> m_releaseInfoPool = new List<ReleaseInfo>(50);
//      
//         // 延迟释放数组，设置一个桶装结构，这样可能会少遍历一些元素
//         private List<ReleaseInfo>[] m_DelayRemoveList = new List<ReleaseInfo>[10];
//         private int m_DelayTick = 0;
//         private float m_UpdateTick = 0;
//         #endregion
//
//         // 自动释放数组
//         private List<ReleaseInfo> m_autoRemoveList = new List<ReleaseInfo>(20);
//
//
// #if ODIN_INSPECTOR
//         [ShowInInspector, ShowIf("showOdinInfo"), ListDrawerSettings(IsReadOnly = true)]
// #endif
//         private List<AssetBundleLoadAssetOperation> m_InProgressOperations = new List<AssetBundleLoadAssetOperation>();
// #if ODIN_INSPECTOR
//         [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
// #endif
//         private readonly Dictionary<string, List<System.Delegate>> m_CallbackStack = new Dictionary<string, List<System.Delegate>>();
// #if ODIN_INSPECTOR
//         [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
// #endif
//         private readonly Dictionary<string, AssetCache> m_AssetCaches = new Dictionary<string, AssetCache>();
//
//    
// #if ODIN_INSPECTOR
//         [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
// #endif
//         private readonly Dictionary<object, string> m_ObjectKeyMap = new Dictionary<object, string>();
//
//         private readonly List<string> m_CallbackTemp = new List<string>();
//
//
//         ResourceManager()
//         {
//             for (int i=0; i<m_DelayRemoveList.Length; ++i)
//             {
//                 m_DelayRemoveList[i] = new List<ReleaseInfo>(20);
//             }
//         }
//
//         public override void Release()
//         {
//             base.Release();
//             UnloadUnusedAssets();
//         }
//
//         private static StringBuilder m_SB = new StringBuilder(256);
//         public static string AssetKeyLower(string assetBundle, string assetName, System.Type type)
//         {
//             if (string.IsNullOrEmpty(assetBundle))
//             {
//                 GameFramework.Log.Error("AssetBundleName is Null");
//                 return string.Empty;
//             }
//
//             assetBundle = assetBundle.ToLowerInvariant();
//
//             return m_SB.Clear().AppendFormat(AssetKeyFormat, assetName ?? "", assetBundle, type).ToString();
//         }
//         
//         public static void ParseAssetPath(string assetPath, bool isSingleBundle, out string assetName, out string assetBundle)
//         {
//             if (string.IsNullOrEmpty(assetPath))
//             {
//                 assetName = "";
//                 assetBundle = "";
//                 return;
//             }
//
//             assetName = Path.GetFileNameWithoutExtension(assetPath);
//             int length = assetPath.LastIndexOf(isSingleBundle ? '.' : '/');
//             if (length != -1)
//                 assetBundle = assetPath.Substring(0, length).ToLowerInvariant();
//             else
//                 assetBundle = assetPath.ToLowerInvariant();
//         }
//
//
//         //新cg逻辑有个时序问题 太困了 先暂时糊一版
//         private bool StartInitialize = false;
//         public void Initialize(string localAssetBundlePath, string remoteAssetBundlePath, OnLoadComplete callback, AssetBundleManager.LoadMode loadMode = AssetBundleManager.LoadMode.Local, AssetBundleManager.LogMode logMode = AssetBundleManager.LogMode.JustErrors)
//         {
//             StartInitialize = true;
//             if (!AssetBundleManager.IsInited)
//             {
//
//                 AssetBundleManager.loadMode = loadMode;
//                 AssetBundleManager.logMode = logMode;
//
//                 AssetBundleManager.SetLocalAssetBundleDirectory(localAssetBundlePath);
//                 AssetBundleManager.SetRemoteAssetBundleURL(remoteAssetBundlePath);
//                 RegistCallback(AssetBundles.Utility.GetPlatformName(), callback);
//                 m_AssetBundleLoadManifestOperation = AssetBundleManager.Initialize();
//             }
//             else
//             {
//                 StartCoroutine(_YieldCallback(callback));
//             }
//         }
//
//         private static WaitForEndOfFrame WaitFor = new WaitForEndOfFrame();
//         IEnumerator _YieldCallback(OnLoadComplete callback)
//         {
//             yield return WaitFor;
//             callback(AssetBundles.Utility.GetPlatformName(), AssetBundleManager.AssetBundleManifestObject, null);
//         }
//
//
//         /// <summary>
//         /// Loads the asset async.
//         /// </summary>
//         /// <param name="assetBundle">Asset bundle.</param>
//         /// <param name="assetName">Asset name.</param>
//         /// <param name="callback">Callback.</param>
//         /// <param name="memeryHold">Memery hold.</param>
//         /// <typeparam name="T">The 1st type parameter.</typeparam>
//         public void LoadAssetAsync<T>(string assetBundle, string assetName = null, OnLoadComplete callback = null, MemeryHold memeryHold = MemeryHold.Normal) where T : Object
//         {
//             LoadAssetAsync(assetBundle, assetName, typeof(T), callback,  memeryHold);
//         }
//
//         public string LoadAssetAsync<T>(string assetPath, OnLoadComplete callback = null, bool isSingleBundle = true) where T : Object
//         {
//             ParseAssetPath(assetPath, isSingleBundle, out string a, out string b);
//             return LoadAssetAsync(b, a, typeof(T), callback);
//         }
//         
//         public T LoadAssetSync<T>(string assetPath) where T : Object
//         {
//             ParseAssetPath(assetPath, true, out string assetName, out string assetBundle);
//             if (string.IsNullOrEmpty(assetBundle))
//             {
//                 throw new Exception($"LoadAssetAsync {assetPath} assetBundle is null or empty!!!");
//             }
//
//             var type = typeof(T);
//             // 先获取缓存区
//             assetBundle = assetBundle.ToLowerInvariant();
//             string key = AssetKeyLower(assetBundle, assetName, type);
//             AssetCache cache = GenAssetCache(key, assetBundle, assetName, type, MemeryHold.Normal);
//             // 如果缓存区已经有资源直接回调
//             if (cache.obj != null)
//                 return cache.obj as T;
//        
//             var obj = AssetBundleManager.LoadAssetSync(assetBundle, assetName, type);
//             if (obj != null)
//             {
//                 AssetCache asset = CacheAsset(key, obj);
//             
// #if UNITY_EDITOR
//                 // 如果是Editor中的Bundle模式的话，就需要在Editor中重置一下shader
//                 if (!AssetBundleManager.SimulateAssetBundleInEditor)
//                 { 
//                     GameObject go = asset.obj as GameObject;
//                     AssetBundleManager.resetEditorShader(go);
//                 }
// #endif
//
//                 asset.assetbundleVariant = AssetBundleManager.RemapVariantName(assetBundle);
//             }
//             else
//             {
//                 cache.assetbundleVariant = AssetBundleManager.RemapVariantName(assetBundle);
//                 RemoveCachedAsset(key);
//             }
//
//             return obj as T;
//         }
//
//       
//         /**
//          * 加载assetBundle资源
//          * 注意：如果memeryHold 是 MemeryHold.Normal的情况 LoadAssetAsync 后一定要调用卸载函数以保证引用计数正确
//          */
//         private string LoadAssetAsync(string assetBundle, string assetName, System.Type type, OnLoadComplete callback = null,  MemeryHold memeryHold = MemeryHold.Normal)
//         {
//             if (string.IsNullOrEmpty(assetBundle))
//             {
//                 throw new Exception($"LoadAssetAsync {assetName} assetBundle is null or empty!!!");
//             }
//
//             // 先获取缓存区
//             assetBundle = assetBundle.ToLowerInvariant();
//             string key = AssetKeyLower(assetBundle, assetName, type);
//             AssetCache cache = GenAssetCache(key, assetBundle, assetName, type, memeryHold);
//
//             // 判断是否正在Loading
//             if (IsInLoading(key))
//             {
//                 // 如果同一个资源已经在Loading，不用再新开加载,注册callBack等待回调就可以了
//                 RegistCallback(key, callback);
//                 return key;
//             }
//
//             // 如果缓存区已经有资源直接回调
//             if (cache.obj != null)
//             {
//                 callback?.Invoke(key, cache.obj, null);
//                 return key;
//             }
//
//             // 注册回调
//             RegistCallback(key, callback);
//             AssetBundleLoadAssetOperation ao = AssetBundleManager.LoadAssetAsync(assetBundle, assetName, type);
//             if (ao != null)
//                 m_InProgressOperations.Add(ao);
//
//             return key;
//         }
//
//     
//
//         public void UnloadAssetWithObject(object obj, bool immediately = false)
//         {
//             if (obj == null)
//                 return;
//
//             if (m_ObjectKeyMap.TryGetValue(obj, out string key))
//                 UnloadAssetWithKey(key, immediately);
//         }
//
//         /// <summary>
//         /// Unloads the asset.
//         /// </summary>
//         /// <param name="key">Key.</param>
//         public void UnloadAssetWithKey(string key, bool immediately = false)
//         {
//             if (string.IsNullOrEmpty(key))
//             {
//                 Log.Error("UnloadAssetWithKey:key is null");
//                 return;
//             }
//
//             if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
//             {
//                 cache.refCount--;
//                 cache.refCount = Mathf.Clamp(cache.refCount, 0, cache.refCount);
//             }
//
//             if (cache == null || cache.refCount != 0 || cache.hold == MemeryHold.Always) 
//                 return;
//             if (applicationIsQuitting || immediately)
//                 RemoveCachedAsset(key);
//             else
//                 RemoveCachedAssetDelay(key, RemoveAssetDelay);
//         }
//
//         /// <summary>
//         /// Unloads the unused assets.
//         /// </summary>
//         public void UnloadUnusedAssets(MemeryHold hold = MemeryHold.Normal)
//         {
//             List<string> tempList = new List<string>();
//             Dictionary<string, AssetCache>.Enumerator e = m_AssetCaches.GetEnumerator();
//             while (e.MoveNext())
//             {
//                 if (e.Current.Value.hold <= hold && e.Current.Value.refCount == 0)
//                 {
//                     tempList.Add(e.Current.Key);
//                 }
//             }
//             for (int i = 0; i < tempList.Count; ++i)
//             {
//                 // 立即移除
//                 RemoveCachedAsset(tempList[i]);
//             }
//             tempList.Clear();
//             e.Dispose();
//             System.GC.Collect();
//         }
//
//
//         public void StopAllLoadingProgress()
//         {
//             AssetBundleLoadAssetOperation operation = null;
//             for (int i = 0; i < m_InProgressOperations.Count; i++)
//             {
//                 operation = m_InProgressOperations[i];
//                 string key = AssetKeyLower(operation.AssetBundleName, operation.AssetName, operation.type);
//                 UnregistCallback(key);
//             }
//         }
//
//         protected override void OnUpdate(float delta)
//         {
//             if(!StartInitialize) return;
//             
//             base.OnUpdate(delta);
//
//             if (!IsInited)
//             {
//                 // 等AssetBundleManifest初始化完成，再继续更新其他Operation
// #if UNITY_EDITOR
//                 if (AssetBundleManager.SimulateAssetBundleInEditor)
//                 {
//                     IsInited = true;
//                     Callback(AssetBundles.Utility.GetPlatformName());
//                 }
//                 else
// #endif
//                 {
//                     if (m_AssetBundleLoadManifestOperation != null && m_AssetBundleLoadManifestOperation.IsDone())
//                     {
//                         IsInited = true;
//                         Callback(AssetBundles.Utility.GetPlatformName(), m_AssetBundleLoadManifestOperation.GetAsset<AssetBundleManifest>(), m_AssetBundleLoadManifestOperation.Error);
//                         m_AssetBundleLoadManifestOperation = null;
//                     }
//                 }
//             }
//             else
//             {
//                 UpdateLoadAssets();
//
//                 m_UpdateTick += delta;
//                 if (m_UpdateTick > 1)
//                 {
//                     m_UpdateTick -= 1;
//                     // 这里强制处理一下，不用追帧了，因为没啥大碍。
//                     if (m_UpdateTick > 1)
//                     {
//                         m_UpdateTick = 0;
//                     }
//                     ++m_DelayTick;
//
//                     UpdateAutoRelease2();
//                     UpdateDelayRelease();
//                 }
//
//             }
//         }
//
//         private void UpdateLoadAssets()
//         {
//             AssetBundleLoadAssetOperation operation = null;
//             for (int i = 0; i < m_InProgressOperations.Count;)
//             {
//                 operation = m_InProgressOperations[i];
//                 if (operation.IsDone())
//                 {
//                     // 如果完成直接移除，避免后面回调方法中上层逻辑错误导致不停Update
//                     m_InProgressOperations.RemoveAt(i);
//                     string key = AssetKeyLower(operation.AssetBundleName, operation.AssetName, operation.type);
//
//                     // 读取所有Assets或者指定Asset
//                     object obj;
//                     if (string.IsNullOrEmpty(operation.AssetName))
//                     {
//                         obj = operation.GetAllAssets();
//                     }
//                     else
//                     {
//                         obj = operation.GetAsset();
//                     }
//
//                     if (obj != null)
//                     {
//                         if (string.IsNullOrEmpty(operation.Error) == false)
//                             Log.Info("Load Assets {0} has error {1}", operation.AssetName, operation.Error);
//                        
//                         // 将obj和key进行缓存
//                         AssetCache asset = CacheAsset(key, obj);
//                         if (asset != null)
//                         {
// #if UNITY_EDITOR
//                             // 如果是Editor中的Bundle模式的话，就需要在Editor中重置一下shader
//                             if (!AssetBundleManager.SimulateAssetBundleInEditor)
//                             { 
//                                 GameObject go = asset.obj as GameObject;
//                                 AssetBundleManager.resetEditorShader(go);
//                             }
// #endif
//
//                             asset.assetbundleVariant = operation.assetBundleVariant;
//                             m_CallbackTemp.Add(key);
//                         }
//                         else
//                         {
//                             AssetBundleManager.UnloadAssetBundle(operation.assetBundleVariant);
//                             string warningStr = $"{key} : assetCache had removed!";
//                             Log.Warning(warningStr);
//                             Callback(key, null, warningStr);
//                         }
//                     }
//                     else
//                     {
//                         if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
//                         {
//                             cache.assetbundleVariant = operation.assetBundleVariant;
//                         }
//
//                         RemoveCachedAsset(key);
//
//                         string err = $"Load {operation.AssetBundleName} error, because {operation.Error}.";
//                         Log.Error(err);
//                         Callback(key, null, err);
//                     }
//                 }
//                 else
//                 {
//                     i++;
//                 }
//             }
//
//             while (m_CallbackTemp.Count > 0)
//             {
//                 string key = m_CallbackTemp[0];
//
//                 if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
//                 {
//                     if (cache != null && cache.obj != null)
//                     {
//                         Callback(key, cache.obj, string.Empty);
//                     }
//                     else
//                     {
//                         Log.Error("{0} is Null", key);
//                     }
//                 }
//
//                 m_CallbackTemp.RemoveAt(0);
//             }
//         }
//
//         private void Callback(string key, object t = null, string err = null)
//         {
//             if (m_CallbackStack.TryGetValue(key, out List<System.Delegate> callbackList))
//             {
//                 m_CallbackStack.Remove(key);
//                 foreach (OnLoadComplete callback in callbackList)
//                 {
//                     try
//                     {
//                         callback?.Invoke(key, t, err);
//                     }
//                     catch (System.Exception e)
//                     {
//                         Log.Error($"{key} : {e}");
//                     }
//                 }
//             }
//             else
//             {
//                 RemoveCachedAsset(key);
//             }
//         }
//
//         /**
//          * 注册回调，等待加载ab包完成就行了，
//          * 注意这里不用判断m_CallbackStack[key].Contain(callback),
//          * 保证调用几次LoadAssetAsync 就回调几次，为什么呢？因为LoadAssetAsync会增加引用计数，有人要是在callBack里释放资源的话，
//          * LoadAssetAsync的多次调用就会导致资源释放不掉
//          */
//         private void RegistCallback(string key, System.Delegate callback)
//         {
//             if (callback != null && !string.IsNullOrEmpty(key))
//             {
//                 if (!m_CallbackStack.ContainsKey(key))
//                     m_CallbackStack.Add(key, new List<System.Delegate>());
//              
//                 m_CallbackStack[key].Add(callback);
//             }
//         }
//
//         private void UnregistCallback(string key)
//         {
//             if (m_CallbackStack.TryGetValue(key, out List<System.Delegate> callbackList))
//             {
//                 m_CallbackStack.Remove(key);
//
//                 foreach (OnLoadComplete callback in callbackList)
//                 {
//                     try
//                     {
//                         callback?.Invoke(key, null, string.Empty);
//                     }
//                     catch (System.Exception e)
//                     {
//                         Log.Error($"{key} : {e}");
//                     }
//                 }
//             }
//         }
//
//         private bool IsInLoading(string key)
//         {
//             return m_CallbackStack.ContainsKey(key);
//         }
//
//         private void RemoveCachedAssetDelay(string key, int delay)
//         {
//             ReleaseInfo info = GetReleaseInfo();
//             info.key = key;
//             info.delay = m_DelayTick + delay + 1;
//
//             if (m_DelayRemoveList.Length == 0)
//             {
//                 Log.Error("DelayRemoveList length = 0!");
//                 return;
//             }
//
//             // 添加到相应的slot桶里
//             int slot = info.delay % m_DelayRemoveList.Length;
//             m_DelayRemoveList[slot].Add(info);
//         }
//
//
//         private void RemoveCachedAsset(string key,bool force = true)
//         {
//             if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
//             {
//                 //非强制删除的话就根据引用计数来判断是否释放
//                 if (!force && cache.refCount > 0)
//                     return;
//                 if (cache.type == typeof(SpriteAtlas))
//                 {
//                     AtlasUtils.ClearCacheSprite(cache.assetName);
//                 }
//                 
//                 if (cache.obj != null)
//                 {
//                     m_ObjectKeyMap.Remove(cache.obj);
//                 }
//                 cache.Release();
//                 m_AssetCaches.Remove(key);
//             }
//         }
//
//
//         private AssetCache CacheAsset(string key, object obj)
//         {
//             if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
//             {
//                 cache.obj = obj;
//                 if (!m_ObjectKeyMap.ContainsKey(obj))
//                     m_ObjectKeyMap.Add(obj, key);
//             }
//             else
//             {
//                 Log.Warning("There is no asset cache with key : {0}, so {1} can not be cached.", key, ((Object)obj).name);
//             }
//
//             return cache;
//         }
//
//         private AssetCache GenAssetCache(string key, string assetBundle, string assetName, System.Type type, MemeryHold hold)
//         {
//             if (m_AssetCaches.TryGetValue(key, out var cache))
//             {
//                 Debug.Assert(assetBundle == cache.assetbundleName && assetName == cache.assetName && type == cache.type, "Cached Asset is not same.");
//
//                 if (cache.hold < hold)
//                     cache.hold = hold;
//
//                 cache.refCount++;
//                 return cache;
//             }
//           
//             cache = new AssetCache
//             {
//                 assetName = assetName,
//                 assetbundleName = assetBundle,
//                 type = type,
//                 hold = hold,
//                 refCount =  1,
//             };
//
//             m_AssetCaches.Add(key, cache);
//             return cache;
//         }
//
//      
//
//         /// <summary>
//         /// 异步加载GameObject方法，GameObject无需释放，直接Destroy或者Recycle
//         /// </summary>
//         public void LoadGameObjectAsync(string assetPath, System.Action<GameObject> callback, bool isSingleBundle = true)
//         {
//             ParseAssetPath(assetPath, isSingleBundle, out string assetName, out string assetBundle);
//             LoadAssetAsync<GameObject>(assetBundle, assetName, (key, asset, err) =>
//             {
//                 if (asset is GameObject prefab)
//                 {
//                     prefab.CreatePool();
//                     GameObject go = prefab.Spawn();
//                     AddAutoRelease(key, go);
//                     callback?.Invoke(go);
//                     return;
//                 }
//                 callback?.Invoke(null);
//             });
//         }
//
//         public void LoadGameObjectAsyncWithoutCache (string assetPath, Transform parent, Action<GameObject> callback)
//         {
//             ParseAssetPath(assetPath, true, out var assetName, out var assetBundle);
//             LoadAssetAsync<GameObject>(assetBundle, assetName, (key, asset, err) =>
//             {
//                 if (asset is GameObject prefab)
//                 {
//                     GameObject go = prefab.Instantiate (parent);
//                     AddAutoRelease(key, go);
//                     callback?.Invoke(go);
//                     return;
//                 }
//                 callback?.Invoke(null);
//             });
//            
//         }
//
//
// #region release begin
//
//         // 从池里获取一个释放信息类
//         private ReleaseInfo GetReleaseInfo()
//         {
//             if (m_releaseInfoPool.Count == 0)
//                 return new ReleaseInfo();
//
//             int lastIndex = m_releaseInfoPool.Count - 1;
//             ReleaseInfo info = m_releaseInfoPool[lastIndex];
//             m_releaseInfoPool.RemoveAt(lastIndex);
//             return info;
//         }
//
//         private void BackReleaseInfo(ReleaseInfo info)
//         {
//             if (m_releaseInfoPool.Count > 200)
//                 return;
//
//             info.go = null;
//             info.delay = 0;
//             info.key = string.Empty;
//
//             m_releaseInfoPool.Add(info);
//         }
//
//         // 处理延迟释放资源
//         private void UpdateDelayRelease()
//         {
//             if (m_DelayRemoveList.Length == 0)
//             {
//                 Log.Error("AddDelayRelease but length = 0!");
//                 return;
//             }
//
//             // 从后向前删除即可
//             var curSlot = m_DelayRemoveList[m_DelayTick % m_DelayRemoveList.Length];
//             for (int i = curSlot.Count - 1; i>=0; --i)
//             {
//                 var obj = curSlot[i];
//                 if (m_DelayTick < obj.delay) 
//                     continue;
//                 curSlot.RemoveAt(i);
//                 // 为false的原因：避免延迟期间，又被重新引用
//                 RemoveCachedAsset(obj.key,false);
//                 BackReleaseInfo(obj);
//             }
//         }
//
//         // 添加自动释放资源
//         void AddAutoRelease(string key, GameObject obj)
//         {
//             ReleaseInfo info = GetReleaseInfo();
//             info.key = key;
//             info.go = obj;
//
//             m_autoRemoveList.Add(info);
//         }
//
//         public void AutoRelease (string key, GameObject obj)
//         {
//             AddAutoRelease (key, obj);
//         }
//
//         private void UpdateAutoRelease2()
//         {            
//             for (int i=0; i<m_autoRemoveList.Count; ++i)
//             {
//                 var obj = m_autoRemoveList[i];
//                 // 为了防止O(n^2)移动，这里使用线性移动处理
//                 // 每次直接把最后的拿过来放在这里，然后下一秒再检测。
//
//                 // NOTICE：这里使用了Unity的处理，Unity在引擎对象被释放后，会把C#层的obj操作符重载且强行等于null（但实际对象不为null）。
//                 // 所以这里的处理是可以成立的。
//                 if (obj.go == null)
//                 {
//                     m_autoRemoveList[i] = m_autoRemoveList[m_autoRemoveList.Count - 1];
//                     m_autoRemoveList.RemoveAt(m_autoRemoveList.Count - 1);
//
//                     UnloadAssetWithKey(obj.key);
//                     BackReleaseInfo(obj);
//                 }
//             }
//         }
//
// #endregion
    }
}
