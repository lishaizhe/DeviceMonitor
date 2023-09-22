using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAssets
{
    public const string CUBE = "Assets/DeviceMonitor/Resource/Prefab/Cube.prefab";
    public const string UILoading = "Assets/DeviceMonitor/Resource/Prefab/UILoadingView.prefab";
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
}