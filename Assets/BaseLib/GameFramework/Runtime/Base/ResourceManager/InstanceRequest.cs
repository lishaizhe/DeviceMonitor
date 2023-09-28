using System;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

public class InstanceRequest
{
    private string prefabPath;
    private ObjectPool pool;

    public enum State
    {
        Init, Loading, Instanced, Destroy
    }

    public string PrefabPath
    {
        get { return prefabPath; }
    }

    public bool isDone { get; private set; }

    public State state;
    public GameObject gameObject;
    public event Action<InstanceRequest> completed;

    public InstanceRequest(string prefabPath)
    {
        this.prefabPath = prefabPath;
        state = State.Init;
    }
    
    public void Instantiate()
    {
        pool = GameEntry.Resource.GetObjectPool(prefabPath);
        state = State.Loading;
    }

    public void Destroy()
    {
        if (gameObject != null)
        {
            pool.DeSpawn(gameObject);
            gameObject = null;
            pool = null;
        }
        
        state = State.Destroy;
        completed = null;
    }

    public bool Update()
    {
        if (state == State.Destroy)
            return false;
        if (!pool.IsAssetLoaded)
            return true;
        try
        {
            if (gameObject == null)
            {
                gameObject = pool.Spawn();
                state = State.Instanced;
                isDone = true;
            }

            completed?.Invoke(this);
            completed = null;
        }
        catch (Exception ex)
        {
            //Log.Error(ex.StackTrace);
            Log.Error(ex);
        }
        
        return false;
    }
}