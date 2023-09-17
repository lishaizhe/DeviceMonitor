
using System.Collections.Generic;
using System.Text;
using GameFramework;
using UnityEngine;

public class ObjectPoolMgr
{
	public ObjectPoolMgr()
	{
		root = new GameObject("ObjectPoolRoot");
		Object.DontDestroyOnLoad(root);
	}
	
    public ObjectPool GetPool(string prefabPath, ResourceManager resourceManager)
    {
	    ObjectPool pool;
        if (!poolList.TryGetValue(prefabPath, out pool))
        {
	        pool = new ObjectPool(this, prefabPath, resourceManager);
	        poolList.Add(prefabPath, pool);
        }
        return pool;
    }

    public void ClearPool(string prefabPath)
    {
	    ObjectPool pool;
        if (!poolList.TryGetValue(prefabPath, out pool))
        {
            return;
        }
        pool.Clear();
        poolList.Remove(prefabPath);
    }

    public void ClearAllPool()
    {
        foreach (ObjectPool objectPool in poolList.Values)
        {
            objectPool.Clear();
        }
        poolList.Clear();
    }

    public void ClearUnusedPool()
    {
	    var keys = new List<string>();
	    foreach (var i in poolList)
	    {
		    if (i.Value.Clear())
		    {
			    keys.Add(i.Key);
		    }
	    }

	    for (int i = 0; i < keys.Count; i++)
	    {
		    poolList.Remove(keys[i]);
	    }
    }

    public void DebugOutput()
    {
	    // 输出日志：每个池的对象数量，总对象数量
	    int totalPoolObj = 0;
	    int totalObj = 0;
	    StringBuilder builder = new StringBuilder();
	    foreach (var i in poolList)
	    {
		    var prefabPath = i.Key;
		    var pool = i.Value;

		    totalPoolObj += pool.GetPoolCount();
		    totalObj += pool.GetObjCount();
		    
		    builder.AppendLine($"pool: {prefabPath}, pool obj count: {pool.GetPoolCount()}, total obj count: {pool.GetObjCount()}");
	    }
	    
	    Log.Info($"ObjectPoolMgr: total pool obj count: {totalPoolObj}, total obj count: {totalObj}\n" + builder.ToString());
    }

    public void TryCleanPool()
    {
        foreach (var i in poolList)
        {
	        if (i.Value.TryClean())
	        {
		        unusedPool.Add(i.Key);
	        }
        }

        if (unusedPool.Count > 0)
        {
	        foreach (var i in unusedPool)
	        {
		        poolList.Remove(i);
	        }
	        unusedPool.Clear();
        }
    }
    
    public Transform Root
    {
	    get { return root.transform; }
    }

    public static readonly float CleanPoolTime = 60f;

    private GameObject root;
    private Dictionary<string, ObjectPool> poolList = new Dictionary<string, ObjectPool>();
    private List<string> unusedPool = new List<string>();
}

public class ObjectPool
{
	public bool IsAssetLoaded
	{
		get { return request == null || request.isDone; }
	}
	
	public ObjectPool(ObjectPoolMgr mgr, string prafabPath, ResourceManager resourceManager)
	{
		this.mgr = mgr;
		request = resourceManager.LoadAssetAsync(prafabPath, typeof(GameObject));
	}
	
	public GameObject Spawn()
	{
		if (request == null)
			return null;
		lastDespawnTime = float.MaxValue;
		if (pool.Count > 0)
		{
			GameObject gameObject = pool.Pop();
			gameObject.transform.SetParent(null);
			gameObject.SetActive(true);
			return gameObject;
		}

		GameObject go = Object.Instantiate(request.asset) as GameObject;
		objCount++;
		return go;
	}

	public void DeSpawn(GameObject obj)
	{
		lastDespawnTime = Time.realtimeSinceStartup;
		obj.transform.SetParent(mgr.Root);
		obj.SetActive(false);
		pool.Push(obj);
	}

	public bool Clear()
	{
		while (pool.Count != 0)
		{
			var go = pool.Pop();
			Object.Destroy(go);
			objCount--;
		}
		pool.Clear();
		
		if (request != null && objCount == 0)
		{
			request.Release();
			request = null;
			return true;
		}

		return false;
	}

	public bool TryClean()
	{
		if (Time.realtimeSinceStartup - lastDespawnTime >= ObjectPoolMgr.CleanPoolTime)
		{
			Clear();
		}

		return objCount <= 0 && (request == null || request.isDone);
	}

	public int GetPoolCount()
	{
		return pool.Count;
	}

	public int GetObjCount()
	{
		return objCount;
	}
	
	private ObjectPoolMgr mgr;
	private VEngine.Asset request;
	private Stack<GameObject> pool = new Stack<GameObject>();
	private int objCount;
	private float lastDespawnTime = float.MaxValue;
}