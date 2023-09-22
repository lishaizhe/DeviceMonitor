using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class DataInterface
{
    private Dictionary<string, DeviceData> m_allDInfoData = new Dictionary<string, DeviceData>();
    private DataInterface()
    {
        
    }

    private static DataInterface _inst = null;

    public static DataInterface GetInst()
    {
        if (_inst == null)
            _inst = new DataInterface();
        return _inst;
    }

    //将device信息存储到数据库
    public void SaveToData(DeviceData deviceData)
    {
        //save to DB
        m_allDInfoData[deviceData.name] = deviceData;
        GameEntry.Event.Fire(EventId.E_ReloadDeviceList);
    }
    
    //删除信息
    public void DeleteDInfoFromDB(string key)
    {
        
        if (m_allDInfoData.TryGetValue(key, out _))
        {
            m_allDInfoData.Remove(key);
        }
        GameEntry.Event.Fire(EventId.E_ReloadDeviceList);
    }

    //获取指定设备的信息
    public DeviceData GetDInfoByKey(string key)
    {
        if (m_allDInfoData.TryGetValue(key, out DeviceData dInfo))
            return dInfo;
        return null;
    }

    // 初始化目前所有device基础信息
    public void InitAllDeviceData()
    {
        m_allDInfoData = new Dictionary<string, DeviceData>();
    }

    public Dictionary<string, DeviceData> GetAllDInfo()
    {
        return m_allDInfoData;
    }

    //获取指定设备,所有传感器的信息
    public DeviceSensorData GetSensorDataByDeviceKey( string key )
    {
        DeviceSensorData sdata = new DeviceSensorData();
        for (int i = 0; i < 10; ++i)
        {
            SensorItemData itemData = new SensorItemData();
            itemData.name = string.Format("CW{0:D2}", i);
            itemData.temperature = Random.Range(20, 90);
            sdata.allSensorData.Add(itemData);
        }

        return sdata;
    }

}
