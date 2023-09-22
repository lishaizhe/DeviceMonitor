using UnityEngine;

public class WindowBase : MonoBehaviour
{
    public virtual UITpye WindowType { get;}
    public bool IsActive { get => gameObject.activeSelf; }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    protected virtual void Init()
    {

    }

    /// <summary>
    /// �������ʱ����
    /// </summary>
    protected virtual void OnEnable()
    {
        Init();
        OnAddListener();
    }

    /// <summary>
    /// �رմ���ʱ����
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
    /// �򿪴���
    /// </summary>
    public virtual void OpenWindow()
    {
        
    }

    /// <summary>
    /// �رմ���
    /// </summary>
    public virtual void CloseWindow()
    {

    }

    /// <summary>
    /// ע���¼�
    /// </summary>
    protected virtual void OnAddListener()
    {
    }

    /// <summary>
    /// ע���¼�
    /// </summary>
    protected virtual void OnRemoveListener()
    {

    }

}

