
using UnityEngine;

public class ColorManager
{
    /// <summary>
    /// 按钮未按下颜色
    /// </summary>
    public static Color ButtonDefault;

    /// <summary>
    /// 按下按钮颜色
    /// </summary>
    public static Color ButtonDown;

    /// <summary>
    /// 鼠标滑过按钮颜色
    /// </summary>
    public static Color ButtonHover;

    /// <summary>
    /// 确认按钮默认颜色
    /// </summary>
    public static Color ConfirmButtonDefault;

    /// <summary>
    /// 确认按钮点击颜色
    /// </summary>
    public static Color ConfirmButtonDown;

    /// <summary>
    /// 确认按钮划过颜色
    /// </summary>
    public static Color ConfirmButtonHover;

    /// <summary>
    /// 设备停止颜色
    /// </summary>
    public static Color DeviceDefault;

    /// <summary>
    /// 设备停止颜色
    /// </summary>
    public static Color DeviceStop;

    /// <summary>
    /// 设备运行颜色
    /// </summary>
    public static Color DeviceRun;

    /// <summary>
    /// 设备报警颜色
    /// </summary>
    public static Color DeviceAlarm;

    /// <summary>
    /// 设备故障颜色
    /// </summary>
    public static Color DeviceFault;

    //
    public enum Theme
    { 
        Light,
        Dark,
    }

    public static void SetTheme(Theme t)
    { 
        switch (t)
        {
            case Theme.Light:
                //通用按钮颜色
                ButtonDefault = new Color(16f / 255f, 41f / 255f, 99f / 255f);
                ButtonDown = new Color(52f / 255f, 114f / 255f, 194f / 255f);
                ButtonHover = new Color(16f / 255f, 41f / 255f, 99f / 255f);
                //
                ConfirmButtonDefault = new Color(20f / 255f, 51f / 255f, 123f / 255f);
                ConfirmButtonDown = new Color(52f / 255f, 114f / 255f, 194f / 255f);
                ConfirmButtonHover = new Color(16f / 255f, 41f / 255f, 99f / 255f);
                //
                DeviceDefault = Color.white;
                DeviceStop = Color.red;
                DeviceRun = Color.green;
                DeviceAlarm = Color.yellow;
                DeviceFault = new Color(1f, 0, 1f);

                break; 
            case Theme.Dark:

              break;
        }
    }

    static ColorManager()
    {
        SetTheme(Theme.Light);
    }
}
