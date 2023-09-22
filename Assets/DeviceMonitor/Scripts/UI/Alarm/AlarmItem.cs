using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlarmItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI m_StartTime;   //����ʱ��
    [SerializeField] private TextMeshProUGUI m_EndTime;     //����ʱ��
    [SerializeField] private TextMeshProUGUI m_DeviceName;  //����
    [SerializeField] private TextMeshProUGUI m_Describe;    //����
    [SerializeField] private TextMeshProUGUI m_Value;       //ֵ
    [SerializeField] private TextMeshProUGUI m_AlarmType;   //����
    [SerializeField] private Toggle m_IsOn;                 //�Ƿ�ѡ
    [SerializeField] private ButtonColor m_ButtonColor;     //������ɫ

    public bool IsSelect { get => m_IsOn.isOn; set => m_IsOn.isOn = value; }   //�Ƿ�ѡ��

    public string AlarmID { get; set; }

    /// <summary>
    /// ���ñ�����Ϣ
    /// </summary>
    /// <param name="time">����ʱ��</param>
    /// <param name="name">�����豸</param>
    /// <param name="desc">��������</param>
    /// <param name="args">������ǰֵ</param>
    /// <param name="type">��������</param>
    public void SetAlarmInfo(string id, string startTime, string endTime, string name, string desc, string args, string type)
    {
        AlarmID = id;
        m_StartTime.text = startTime;
        m_EndTime.text = endTime;
        m_DeviceName.text = name;
        m_Describe.text = desc;
        m_Value.text = args;
        m_AlarmType.text = type;
    }

    /// <summary>
    /// ɾ������
    /// </summary>
    public void DestroyAlarm()
    {
        Destroy(this.gameObject);
    }

    public void OnClickCheckBox()
    {
        if (m_ButtonColor != null)
        {
            m_ButtonColor.SetButtonState(IsSelect);
        }
    }

    public void OnClickAlarmArea()
    {
        IsSelect = !IsSelect;
        if (m_ButtonColor != null)
        {
            m_ButtonColor.SetButtonState(IsSelect);
        }
    }
}
