using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DevicePump : DeviceBase
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
        var window = m_ClickOpenWindow.GetComponent<WindowPump>();
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
        //关到位
        if (state == 2)
        {
            m_DeviceStateColor.color = ColorManager.DeviceStop;
            return;
        }
        //开到位
        if (state == 3)
        {
            m_DeviceStateColor.color = ColorManager.DeviceRun;
            return;
        }
        //正在关
        if (state == 0)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceStop : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        //正在开
        if (state == 1)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceRun : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        //报警
        if (256 <= state && state < 512  )
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
