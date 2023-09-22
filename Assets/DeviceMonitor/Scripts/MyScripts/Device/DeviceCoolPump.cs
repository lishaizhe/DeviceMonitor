using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 低温泵
/// </summary>
public class DeviceCoolPump : DeviceBase
{

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

    protected override void OnClickOpen()
    {
        if (m_ClickOpenWindow == null)
        {
            return;
        }
        var window = m_ClickOpenWindow.GetComponent<WindowCoolPump>();
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
    /// <summary>
    /// 设备状态颜色
    /// </summary>
    /// <param name="state"></param>
    public override void SetStateColor(int state)
    {
        // 设备未运行
        if (state == 10)
        {
            m_DeviceStateColor.color = ColorManager.DeviceStop;
            return;
        }
        // 设备开始预冷
        if (state == 9)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceRun : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        // 设备预冷完成
        if (state == 11)
        {
            m_DeviceStateColor.color = ColorManager.DeviceRun;
            return;
        }
        // 设备开始再生回温
        if (state == 6)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceStop : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        // 设备开始再生回温完成
        if (state == 14)
        {
            m_DeviceStateColor.color = ColorManager.DeviceRun;
            _state = !_state;
            return;
        }

        // 报警
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
}
