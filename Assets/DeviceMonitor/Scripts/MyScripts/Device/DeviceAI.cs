using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 模拟量设备
/// </summary>
public class DeviceAI : DeviceBase
{
    [Header("AI Settings")]
    [SerializeField] protected TextMeshProUGUI m_DeviceValue;    //当前值

    protected override void Awake()
    {
        //注册点击事件
        if (m_IsClick)
        {
            var button = GetComponent<Button>();
            if (button == null)
            {
                Debug.Log("没有找到button");
                return;
            }
            button.onClick.AddListener(OnClickOpen);
        }
    }

    /// <summary>
    /// 弹出窗口
    /// </summary>
    protected override void OnClickOpen()
    {
        if (m_ClickOpenWindow == null)
        {
            return;
        }
        var window = m_ClickOpenWindow.GetComponent<WindowAI>();
        if (window != null)
        {
            window.EqName = m_EqName;
            window.ShowName = m_DeviceShowName.text;
            window.OpenWindow();
        }
        else
        {
            Debug.Log("OpenWindow = null");
        }
    }

    private bool _state = false;
    public override void SetStateColor(int state)
    {
        //报警
        if (256 <= state && state < 512)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceAlarm : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        //故障
        if (512 <= state)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceFault : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
    }

    /// <summary>
    /// 设置当前值
    /// </summary>
    /// <param name="value"></param>
    public void SetCurrentValue(string value)
    {
        m_DeviceValue.text = value;
    }
}
