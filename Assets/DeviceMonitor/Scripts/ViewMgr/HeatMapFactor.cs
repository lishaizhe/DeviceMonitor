using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapFactor : MonoBehaviour
{
    public string ObjName {
        get
        {
            return gameObject.name;
        }
    }

    public string SensorName { set; get; }

    private bool m_isOn = false;
    public bool IsOn {
        set
        {
            m_isOn = value;
        }
        get
        {
            return m_isOn;
        }
    }
    public float influenceRadius = 3.0f;
    public float intensity = 3.0f;
    public float temperatureFactor = 1.0f;
    
    public void SetData(HotPointInfo hotPointInfo)
    {
        influenceRadius = hotPointInfo.range;
        SensorName = hotPointInfo.sensorName;
    }

    public void UpdateTemperature(float t)
    {
        temperatureFactor = t;
    }
}