
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