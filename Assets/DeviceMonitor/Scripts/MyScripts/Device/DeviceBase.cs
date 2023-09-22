using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �豸����
/// </summary>
public class DeviceBase : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected string m_EqName; //�豸eqName
    [SerializeField] protected TextMeshProUGUI m_DeviceShowName;    //�豸��ʾ����
    [SerializeField] protected Image m_DeviceStateColor;    //�豸״̬��ɫ

    [Header("Device Operation")]
    [SerializeField] protected bool m_IsClick = true;           //�Ƿ�ɵ��
    [SerializeField] protected GameObject m_ClickOpenWindow;    //���������UI����

    /// <summary>
    /// �豸����(Ψһ)
    /// </summary>
    public string EqName { get => m_EqName; }


    protected virtual void Awake()
    {

    }

    /// <summary>
    /// ��������
    /// </summary>
    protected virtual void OnClickOpen()
    {

    }

    /// <summary>
    /// ��ɫ״̬
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