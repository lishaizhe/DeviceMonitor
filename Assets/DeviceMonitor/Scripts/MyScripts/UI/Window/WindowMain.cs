
using System.Collections.Generic;
using UnityEngine;

public class WindowMain : WindowBase
{
    public override UITpye WindowType { get => UITpye.Main;}
    [HideInInspector] public List<DeviceBase> AllDevices = null;
    public string SystemName;
    public string PageName;
    protected override void Init()
    {
        AllDevices = new List<DeviceBase>();
        AllDevices.AddRange(GetComponentsInChildren<DeviceBase>());
    }


    public override void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    public override void OpenWindow()
    {
        gameObject.SetActive(true);
    }


    protected override void OnAddListener()
    {
        base.OnAddListener();
    }

    protected override void OnRemoveListener()
    {
        base.OnRemoveListener();
    }
}
