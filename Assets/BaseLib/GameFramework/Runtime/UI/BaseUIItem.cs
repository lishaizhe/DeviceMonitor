using GameKit.Base;
using UnityEngine;
using UnityGameFramework.Runtime;


public abstract class BaseUIItem : MonoBehaviour
{
    protected UIFormLogic m_parent;
    protected bool released;

    public void DoInit(UIFormLogic parentUI, object userData)
    {
        m_parent = parentUI;
        Init(parentUI, userData);
    }

    public virtual void Init(UIFormLogic parentUI, object userData)
    {
        released = false;
    }

    public virtual void Release()
    {
        if (released)
            return;

        released = true;
    }

    void OnDestroy()
    {
        if (released)
            return;

        Release();
    }

    /// <summary>
    /// 查找面板内组件
    /// </summary>
    /// <param name="path">面板内相对路径</param>
    /// <returns>对应组件</returns>
    protected T FindComponent<T>(string path, Transform tran = null) where T : Component
    {
        Transform trans = tran ? tran : transform;
        Transform result = trans.Find(path);
        if (result == null)
        {
            return null;
        }

        if (typeof(Transform) == typeof(T))
            return result as T;
        return result.GetComponent<T>();
    }


}