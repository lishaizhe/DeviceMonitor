using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//传感器
public class SensorItemData
{
    public string name;
    public float temperature;
}

//挂点
public class HotPointInfo
{
    public bool select;
    public string name;
    public string sensorName;
    public float range;
}

public class DeviceData
{
    public string name = "";
    public float modelWidth = 0;
    public float modelHeight = 0;
    public string desc = "";
    public string modelPath = "";
    public byte[] modelData = null; //这块最好单独处理
    public List<HotPointInfo> m_hotpointList = new List<HotPointInfo>();

    public void CopyTo(DeviceData dInfo)
    {
        if (dInfo == null)
            return;
        dInfo.name = name;
        dInfo.desc = desc;
        dInfo.modelWidth = modelWidth;
        dInfo.modelHeight = modelHeight;
        dInfo.modelPath = modelPath;
        if (modelData != null)
        {
            dInfo.modelData = (byte[])modelData.Clone();
        }

        for (int i = 0; i < m_hotpointList.Count; ++i)
        {
            HotPointInfo _hotPoint = new HotPointInfo();
            _hotPoint.name = m_hotpointList[i].name;
            _hotPoint.@select = m_hotpointList[i].@select;
            _hotPoint.sensorName = m_hotpointList[i].sensorName;
            _hotPoint.range = m_hotpointList[i].range;
            dInfo.m_hotpointList.Add(_hotPoint);
        }
    }
}


public class DeviceSensorData
{
    public List<SensorItemData> allSensorData = new List<SensorItemData>();
}
