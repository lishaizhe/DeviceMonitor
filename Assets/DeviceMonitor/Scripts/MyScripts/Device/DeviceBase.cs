using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设备基类
/// </summary>
public class DeviceBase : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected string m_EqName; //设备eqName
    [SerializeField] protected TextMeshProUGUI m_DeviceShowName;    //设备显示名称
    [SerializeField] protected Image m_DeviceStateColor;    //设备状态颜色

    [Header("Device Operation")]
    [SerializeField] protected bool m_IsClick = true;           //是否可点击
    [SerializeField] protected GameObject m_ClickOpenWindow;    //点击弹出的UI窗口

    /// <summary>
    /// 设备名称(唯一)
    /// </summary>
    public string EqName { get => m_EqName; }


    protected virtual void Awake()
    {

    }

    /// <summary>
    /// 弹出窗口
    /// </summary>
    protected virtual void OnClickOpen()
    {

    }

    /// <summary>
    /// 颜色状态
    /// </summary>
    /// <param name="state"></param>
    public virtual void SetStateColor(int state)
    {

    }
}

public enum DeviceColorState
{ 
    Red,
    Green,
    Yellow,
    Purple
}