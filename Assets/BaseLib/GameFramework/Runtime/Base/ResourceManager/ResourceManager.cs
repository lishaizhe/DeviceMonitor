using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

//
// 资源管理
//
public class ResourceManager
{
    private float lastTimePerSecondUpdate;
    private string DownloadURL
    {
        get { return ""; }
    }

    public string CheckVersionURL
    {
        get
        {
            return "";

        }
    }

    public enum PreloadType { Cache, KeepAlive }

    public class PreloadCache
    {
        public VEngine.Asset asset;
        public float expiredTime;
    }

    private const float CacheTime = 300.0f;
    private Dictionary<string, PreloadCache> preloadCache = new Dictionary<string, PreloadCache>();
    private List<string> keysRemove = new List<string>();
    
    public void Initialize(Action<bool> onComplete)
    {
        var operation = VEngine.Versions.InitializeAsync();
        operation.completed += delegate
        {
            if (operation.status == VEngine.OperationStatus.Failed)
            {
                Log.Error(operation.error);
            }

            onComplete?.Invoke(operation.status == VEngine.OperationStatus.Success);
        };
        
    
        VEngine.Versions.DownloadURL = DownloadURL;
        VEngine.Versions.getDownloadURL = GetDownloadURL;
        
        SpriteAtlasManager.atlasRegistered += OnAtlasRegistered;
        SpriteAtlasManager.atlasRequested += OnAtlasRequested;
    }

    private string GetDownloadURL(string filename)
    {
        var platformPath = VEngine.Versions.PlatformName;
        string package_name = Application.identifier;
        return $"{DownloadURL}{package_name}/{platformPath}/{filename}";
    }

    public string GetTempDownloadPath(string file)
    {
        var ret = $"{Application.temporaryCachePath}/Download/{file}";
        var dir = Path.GetDirectoryName(ret);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return ret;
    }
    
    //
    // 获取更新文件总大小
    //
    public ulong GetDownloadSize(List<VEngine.Manifest> manifests, List<VEngine.DownloadInfo> downloadInfos)
    {
        if (VEngine.Versions.SkipUpdate || manifests == null || manifests.Count == 0)
            return 0;

        ulong totalSize = 0;
        downloadInfos.Clear();
        var bundles = VEngine.Versions.GetBundlesWithGroups(manifests.ToArray(), null);
        foreach (var bundle in bundles)
        {
            var savePath = VEngine.Versions.GetDownloadDataPath(bundle.name);
            
            if (!VEngine.Versions.IsDownloaded(bundle))
            {
                if (!downloadInfos.Exists(downloadInfo => downloadInfo.savePath == savePath))
                {
                    totalSize += bundle.size;
                    downloadInfos.Add(new VEngine.DownloadInfo
                    {
                        crc = bundle.crc,
                        url = VEngine.Versions.GetDownloadURL(bundle.name),
                        size = bundle.size,
                        savePath = savePath
                    });
                }
            }    
        }
        
        return totalSize;
    }

    //
    // 下载所有更新
    //
    public VEngine.DownloadVersions DownloadUpdates(List<VEngine.DownloadInfo> downloadInfos)
    {
        return VEngine.Versions.DownloadAsync(downloadInfos.ToArray());
    }

    public void Clear()
    {
        foreach (var i in preloadCache)
        {
            i.Value.asset.Release();
        }
        preloadCache.Clear();

        ClearInstance();
        
        objectPoolMgr.ClearAllPool();
    }

    //
    // 加载资源
    //
    public VEngine.Asset LoadAsset(string path, Type type)
    {
        return VEngine.Asset.Load(path, type);
    }

    public VEngine.Asset LoadAssetAsync(string path, Type type)
    {
        return VEngine.Asset.LoadAsync(path, type);
    }

    public VEngine.Asset LoadAssetAsync<T>(string path, Action<VEngine.Asset> complete = null)
    {
        return VEngine.Asset.LoadAsync<T>(path, complete);
    }
    
    public void PreloadAsset(string path, Type type, PreloadType preloadType = PreloadType.Cache)
    {
        var expiredTime = preloadType == PreloadType.Cache ? Time.realtimeSinceStartup + CacheTime : float.MaxValue;
        if (preloadCache.TryGetValue(path, out var cache))
        {
            cache.expiredTime = expiredTime;
        }
        else
        {
            var asset = VEngine.Asset.LoadAsync(path, type);
            preloadCache.Add(path, new PreloadCache {asset = asset, expiredTime = expiredTime});
        }
    }

    //
    // 卸载资源
    //
    public void UnloadAsset(VEngine.Asset asset)
    {
        if (asset != null)
        {
            asset.Release();
        }
    }

    // 资源清单中是否有该资源
    public bool HasAsset(string path)
    {
        var tempPath = path;
        return VEngine.Versions.GetAsset(ref tempPath) != null;
    }

    // 资源是否已下载
    public bool IsAssetDownloaded(string path)
    {
        return VEngine.Versions.IsAssetDownloaded(path);
    }

    public void UnloadUnusedAssets()
    {
        objectPoolMgr.ClearUnusedPool();
        VEngine.Asset.UnloadUnusedAssets();
        VEngine.Bundle.DebugOutputCache();
    }

    public void CollectGarbage()
    {
        // 输出日志
        Log.Info("CollectGarbage begin");
        objectPoolMgr.DebugOutput();
        VEngine.Asset.DebutOutputCache();
        VEngine.Bundle.DebugOutputCache();
        
        objectPoolMgr.ClearUnusedPool();
        VEngine.Asset.UnloadUnusedAssets();
        
        Log.Info("CollectGarbage end");
        objectPoolMgr.DebugOutput();
        VEngine.Asset.DebutOutputCache();
        VEngine.Bundle.DebugOutputCache();
    }

    public void DebugOutput()
    {
        objectPoolMgr.DebugOutput();
        VEngine.Asset.DebutOutputCache();
        VEngine.Bundle.DebugOutputCache();
    }

    public void DebugLoadCount()
    {
        VEngine.Asset.DebugLoadCount();
    }

    public void RemoveCachedUnusedAssets()
    {
        VEngine.Asset.RemoveCachedUnusedAssets();
    }

    public string GetResVersion()
    {
        return VEngine.Versions.ManifestsVersion;
    }

    public bool IsSimulation
    {
        get { return VEngine.Versions.IsSimulation; }
    }

    public bool SkipUpdate
    {
        get { return VEngine.Versions.SkipUpdate; }
    }

    public void Update()
    {
        UpdateInstance();

        if (Time.realtimeSinceStartup - lastTimePerSecondUpdate >= 1)
        {
            lastTimePerSecondUpdate = Time.realtimeSinceStartup;
            
            UnityUIExtension.Update();
            UpdatePoolClean();
            UpdatePreloadRelease();
        }
    }

    private void UpdatePoolClean()
    {
        objectPoolMgr.TryCleanPool();
    }

    private void UpdatePreloadRelease()
    {
        foreach (var i in preloadCache)
        {
            if (i.Value.expiredTime < Time.realtimeSinceStartup)
            {
                i.Value.asset.Release();
                keysRemove.Add(i.Key);
            }
        }

        if (keysRemove.Count > 0)
        {
            foreach (var key in keysRemove)
            {
                preloadCache.Remove(key);
            }
            keysRemove.Clear();
        }
    }
    
    //========================================================================
    // Sprite late binding
    //========================================================================
    #region SpriteAtlas
    
    private const string AtlasRootPath = "Assets/Main/Atlas/{0}.spriteatlas";
    
    private void OnAtlasRegistered(SpriteAtlas sa)
    {
        Log.Debug("OnAtlasRegistered: {0},{1}", sa.name, Time.frameCount);
    }
    
    private void OnAtlasRequested(string atlasName, System.Action<SpriteAtlas> callback)
    {
        Log.Debug("OnAtlasRequested: {0},{1}", atlasName, Time.frameCount);

        var atlasPath = string.Format(AtlasRootPath, atlasName);
        var req = VEngine.Asset.Load(atlasPath, typeof(SpriteAtlas));
        if (!req.isError)
        {
            callback(req.asset as SpriteAtlas);
        }
    }
    
    #endregion
    
    //========================================================================
    // GameObject Instantiate & Destroy
    //========================================================================
    #region Instantiate & Destroy
    
    private static readonly int MAX_INSTANCE_PERFRAME = 20;
    private static readonly float MAX_INSTANCE_TIME = 15.0f;
    private ObjectPoolMgr objectPoolMgr = new ObjectPoolMgr();
    private List<InstanceRequest> toInstanceList = new List<InstanceRequest>();
    private List<InstanceRequest> instancingList = new List<InstanceRequest>();
    private Dictionary<GameObject, InstanceRequest> gameObject2Request = new Dictionary<GameObject, InstanceRequest>();

    private void UpdateInstance()
    {
        int max = MAX_INSTANCE_PERFRAME;
        if (toInstanceList.Count > 0 && instancingList.Count < max)
        {
            int count = Math.Min(max - instancingList.Count, toInstanceList.Count);
            for (var i = 0; i < count; ++i)
            {
                var item = toInstanceList[i];
                if (item.state == InstanceRequest.State.Destroy)
                    continue;

                item.Instantiate();
                instancingList.Add(item);
            }

            toInstanceList.RemoveRange(0, count);
        }

        var time = DateTime.Now.TimeOfDay.TotalMilliseconds;
        int insCount = 0;
        for (var i = 0; i < instancingList.Count; i++)
        {
            var item = instancingList[i];
            if (item.Update())
                continue;
           

            instancingList.RemoveAt(i);
            --i;
            ++insCount;
            if (DateTime.Now.TimeOfDay.TotalMilliseconds - time > MAX_INSTANCE_TIME)
            {
                Log.Debug("resource log long time: {0} use time: {1} instaceCount : {2}", item.PrefabPath,
                    DateTime.Now.TimeOfDay.TotalMilliseconds - time, insCount);
                break;
            }
        }
    }


    private void ClearInstance()
    {
        foreach (var i in toInstanceList)
        {
            i.Destroy(); 
        }

        foreach (var i in instancingList)
        {
            i.Destroy();
        }
    }

    public ObjectPool GetObjectPool(string prefabPath)
    {
        return objectPoolMgr.GetPool(prefabPath, this);
    }

    public InstanceRequest InstantiateAsync(string prefabPath)
    {
        var req = new InstanceRequest(prefabPath);
        toInstanceList.Add(req);
        return req;
    }

    #endregion
}



//
// UI扩展
//
public static class UnityUIExtension
{
    static List<UnityEngine.Object> keyToRemove = new List<UnityEngine.Object>(100);
    static readonly Dictionary<UnityEngine.Object, VEngine.Asset> AllRefs = new Dictionary<UnityEngine.Object, VEngine.Asset>();
    
    public static void LoadSprite(this Image image, string spritePath, string defaultSprite = null)
    {
        if (AllRefs.TryGetValue(image, out var asset))
        {
            asset.Release();
            AllRefs.Remove(image);
        }
        asset = LoadSprite(spritePath, defaultSprite);
        if (asset != null && !asset.isError)
        {
            AllRefs.Add(image, asset);
            image.sprite = asset.asset as Sprite;
        }
    }
    
    
    public static void LoadSprite(this SpriteRenderer spriteRenderer, string spritePath, string defaultSprite = null)
    {
        if (AllRefs.TryGetValue(spriteRenderer, out var asset))
        {
            asset.Release();
            AllRefs.Remove(spriteRenderer);
        }
        asset = LoadSprite(spritePath, defaultSprite);
        if (asset != null && !asset.isError)
        {
            AllRefs.Add(spriteRenderer, asset);
            spriteRenderer.sprite = asset.asset as Sprite;
        }
    }

    public static void Update()
    {
        foreach (var kv in AllRefs)
        {
            var obj = kv.Key;
            if (obj == null)
            {
                kv.Value.Release();
                keyToRemove.Add(obj);
            }
        }

        if (keyToRemove.Count > 0)
        {
            foreach (var k in keyToRemove)
            {
                AllRefs.Remove(k);
            }
            keyToRemove.Clear();
        }
    }
    
    private static VEngine.Asset LoadSprite(string spritePath, string defaultSprite)
    {
        if (!spritePath.EndsWith(".png"))
        {
            spritePath += ".png";
        }
        var req = VEngine.Asset.Load(spritePath, typeof(Sprite));
        if ((req == null || req.isError) && !string.IsNullOrEmpty(defaultSprite))
        {
            if (!defaultSprite.EndsWith(".png"))
            {
                defaultSprite += ".png";
            }
            req = VEngine.Asset.Load(defaultSprite, typeof(Sprite));
        }

        return req;
    }
}