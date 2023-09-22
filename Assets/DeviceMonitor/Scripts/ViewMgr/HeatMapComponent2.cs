using System;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapComponent2 : MonoBehaviour
{
    private Material m_material = null;
    private bool m_beginToRender = false;

    public Material material
    {
        get
        {
            if (null == m_material)
            {
                Renderer render = null;
                Renderer[] renders = this.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renders.Length; ++i)
                {
                    if (!renders[i].name.Contains("ObjHotPoint"))
                    {
                        render = renders[i];
                        break;
                    }
                }
                if (null == render)
                    throw new Exception("123");
                m_material = render.material;
            }
            return m_material;
        }
    }

    public List<HeatMapFactor> impactFactors = new List<HeatMapFactor>();

    public void UpdateSensorData( DeviceSensorData sensorData )
    {
        m_beginToRender = true;
        foreach (var sensor in sensorData.allSensorData)
        {
            var objHotPoint = impactFactors.Find(x => x.SensorName == sensor.name);
            if (objHotPoint != null)
            {
                objHotPoint.UpdateTemperature(sensor.temperature);
            }
        }
    }

    //设置设备信息 -- 筛选使用的factor节点
    public void SetDInfo(DeviceData dInfo)
    {
        HeatMapFactor[] objs = GetComponentsInChildren<HeatMapFactor>();
        foreach (var obj in objs)
        {
            var sensorInfo = dInfo.m_hotpointList.Find(x => x.name == obj.ObjName);
            if (sensorInfo != null && sensorInfo.@select)
            {
                obj.SetData(sensorInfo);
                impactFactors.Add(obj);
            }
        }
    }
    
    private void Update()
    {
        if (m_beginToRender == false)
            return;
        RefreshHeatmap();
    }

    private void RefreshHeatmap()
    {
        //筛选当前使用的节点,封装数据给shader
        // set impact factor count
        material.SetInt("_FactorCount", impactFactors.Count);

        // set impact factors
        var ifPosition = new Vector4[impactFactors.Count];
        for (int i = 0; i < impactFactors.Count; i++)
            ifPosition[i] = impactFactors[i].transform.position;
        material.SetVectorArray("_Factors", ifPosition);

        // set factor properties
        var properties = new Vector4[impactFactors.Count];
        for (int i = 0; i < impactFactors.Count; i++)
        {
            var factor = impactFactors[i];
            float t = (factor.temperatureFactor - (-200)) / 400;
            properties[i] = new Vector4(factor.influenceRadius, factor.intensity, t, 0.0f);
        }

        material.SetVectorArray("_FactorsProperties", properties);

        // TODO: 将温度本身数值作为一个影响因子累乘
        // set factor values
        //var values = new float[impactFactors.Count];
        //for( int i = 0 ; i < impactFactors.Count;i++ )
        //    values[i] = Random.Range(0,5);
        //material.SetFloatArray("_FactorsValues",values);
    }
}