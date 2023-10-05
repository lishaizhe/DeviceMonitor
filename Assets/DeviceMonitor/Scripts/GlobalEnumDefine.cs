using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAssets
{
    //登录界面
    public const string UILoading = "Assets/DeviceMonitor/Resource/Prefab/UILoadingView.prefab";
    //主界面
    public const string UINavigationBar = "Assets/DeviceMonitor/Resource/Prefab/UINavigationBar.prefab";
    //测温界面
    public const string UIMeasureSystem = "Assets/DeviceMonitor/Resource/Prefab/UIMeasureSystem.prefab";
    //测温系统中模型
    public const string Model3D = "Assets/DeviceMonitor/Resource/Prefab/3DModel.prefab";
    //tips弹窗
    public const string UITips = "Assets/DeviceMonitor/Resource/Prefab/UITips.prefab";
    //背景板
    public const string UIBackground = "Assets/DeviceMonitor/Resource/Prefab/UIBackGround.prefab";
}


public enum SubMenuType
{
    ZhenKong, 
    Diwen,
    Measure,
}


public enum EventId
{
    E_ReloadDeviceList = 0,
    E_SelectDevice ,
    E_BeginToRender,
    E_EditDeviceDone,
    E_HotPointShow,
    E_HotPointHide,
    E_TouchEnterSensorTips, //鼠标放置在模型检测点上
    E_TouchExitSensorTips, //从检测点上移开
    E_ClearAllFollowItem, //清空所有3D UI
    E_DeActiveNavMenuBtn, //隐藏导航栏中按钮
}