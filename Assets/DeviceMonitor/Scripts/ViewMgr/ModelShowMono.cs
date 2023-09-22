using System.Collections;
using System.Collections.Generic;
using System.IO;
using TriLibCore;
using UnityEngine;
using UnityGameFramework.Runtime;

public class ModelShowMono : MonoBehaviour
{
    //用来替换
    public Material m_mat;
    public GameObject m_objPrefabHotPoint;
    private DeviceData m_dInfo;
    private GameObject m_instObj;
    private HeatMapComponent2 m_heatMap;
    private MainView m_mainView;

    public void SetMainView(MainView view)
    {
        m_mainView = view;
    }

    public void ShowModel(DeviceData dInfo)
    {
        if (m_dInfo != null && dInfo != null && m_dInfo.name == dInfo.name) //相同的直接return
            return;
        //首先先将模型坐标归零
        this.transform.localPosition = new Vector3(0,0, m_mainView.C_RecomandZ);
        this.transform.RemoveAllChild();
        m_dInfo = dInfo;
        InstanceObj();
    }

    public void ResetPos()
    {
        if (m_dInfo != null)
        {
            this.transform.localPosition = new Vector3(0,0, m_mainView.C_RecomandZ);
            this.transform.rotation = Quaternion.identity;
        }
    }

    //设置传感器数据
    public void SetSensorData( DeviceSensorData sensorData )
    {
        if (m_heatMap)
            m_heatMap.UpdateSensorData(sensorData);
    }

    public void InstanceObj()
    {
        if (m_dInfo == null || m_dInfo.modelData == null || m_dInfo.modelData.Length == 0)
            return;
        //根据数据加载模型
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        Stream stream = new MemoryStream(m_dInfo.modelData);
        AssetLoader.LoadModelFromStream(stream, 
            "water.fbx", 
            null, 
            OnLoad, 
            null, 
            null,
            null,  
            null, 
            assetLoaderOptions);
    }
    
    protected void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        if (assetLoaderContext.RootGameObject == null)
            return;
        GameObject objRoot = assetLoaderContext.RootGameObject;
        objRoot.transform.parent = transform;
        objRoot.SetLayer("Model");
        objRoot.transform.localScale = new Vector3(1, 1, 1);
        objRoot.transform.localRotation = Quaternion.identity;
        objRoot.transform.localPosition = new Vector3(0,0,0);
        m_instObj = objRoot;
        SetEnvForModelObj();
    }

    private void SetEnvForModelObj()
    {
        
        
        StartCoroutine(loadMat());
    }

    HotPointInfo GetHotPointInfoByHotPointId(string hotpointId)
    {
        if (m_dInfo == null)
            return null;
        var hotPointItem = m_dInfo.m_hotpointList.Find(x => x.name == hotpointId);
        if (hotPointItem != null)
        {
            return hotPointItem;
        }

        return null;
    }

    IEnumerator loadMat()
    {
        yield return new WaitForSeconds(0.1f);
        //替换材质
        MeshRenderer[] allRender = m_instObj.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (var render in allRender)
        {
            render.sharedMaterial = m_mat;
        }
        
        //对节点上的传感器设置脚本
        Transform[] allTrans = m_instObj.transform.GetComponentsInChildren<Transform>();
        foreach (var trans in allTrans)
        {
            if (trans.gameObject.name.Contains("hotpoint"))
            {
                var info = GetHotPointInfoByHotPointId(trans.gameObject.name); //根据模型名字选择对应的传感器名字
                if (info != null && info.@select && !string.IsNullOrEmpty(info.sensorName))
                {
                    trans.gameObject.AddComponent<HeatMapFactor>();
                    var obj = Instantiate(m_objPrefabHotPoint, trans);
                    obj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    obj.transform.localPosition = Vector3.zero;
                    obj.layer = LayerMask.NameToLayer("Model");
                    GameObjectIsVisible script = obj.GetComponent<GameObjectIsVisible>();
                    script.SetData(info.sensorName, m_instObj);
                }
            }
        }

        // 添加渲染脚本
        m_heatMap = m_instObj.AddComponent<HeatMapComponent2>();
        m_heatMap.SetDInfo(m_dInfo);
        GameEntry.Event.Fire(EventId.E_BeginToRender);
    }
    

}
