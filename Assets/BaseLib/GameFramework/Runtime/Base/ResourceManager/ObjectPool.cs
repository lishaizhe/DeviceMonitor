using System.Collections.Generic;
using UnityEngine;

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