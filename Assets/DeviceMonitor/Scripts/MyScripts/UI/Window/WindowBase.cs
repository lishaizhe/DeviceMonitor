using UnityEngine;

public class WindowBase : MonoBehaviour
{
    public virtual UITpye WindowType { get;}
    public bool IsActive { get => gameObject.activeSelf; }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void Init()
    {

    }

    /// <summary>
    /// 激活界面时处理
    /// </summary>
    protected virtual void OnEnable()
    {
        Init();
        OnAddListener();
    }

    /// <summary>
    /// 关闭窗口时候处理
    /// </summary>
    protected virtual void OnDisable()
    {
        OnRemoveListener();
    }

    //
    protected virtual void UpdateData()
    { 
    
    }

    /// <summary>
    /// 打开窗口
    /// </summary>
    public virtual void OpenWindow()
    {
        
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void CloseWindow()
    {

    }

    /// <summary>
    /// 注册事件
    /// </summary>
    protected virtual void OnAddListener()
    {
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    protected virtual void OnRemoveListener()
    {

    }

}

