using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlarmItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI m_StartTime;   //发生时间
    [SerializeField] private TextMeshProUGUI m_EndTime;     //结束时间
    [SerializeField] private TextMeshProUGUI m_DeviceName;  //名称
    [SerializeField] private TextMeshProUGUI m_Describe;    //描述
    [SerializeField] private TextMeshProUGUI m_Value;       //值
    [SerializeField] private TextMeshProUGUI m_AlarmType;   //类型
    [SerializeField] private Toggle m_IsOn;                 //是否勾选
    [SerializeField] private ButtonColor m_ButtonColor;     //报警颜色

    public bool IsSelect { get => m_IsOn.isOn; set => m_IsOn.isOn = value; }   //是否选择

    public string AlarmID { get; set; }

    /// <summary>
    /// 设置报警信息
    /// </summary>
    /// <param name="time">报警时间</param>
    /// <param name="name">报警设备</param>
    /// <param name="desc">报警描述</param>
    /// <param name="args">报警当前值</param>
    /// <param name="type">报警类型</param>
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
    /// 删除报警
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
