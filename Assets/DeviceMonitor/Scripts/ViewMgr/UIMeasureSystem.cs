using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/*
 * 存在一个BUG，当修改当前选中的设备后,需要重新设置一次selectInfo
 * 
 */


public class UIMeasureSystem : BaseUIForm
{
    //渲染模型
    [SerializeField] private ModelShowMono m_showMono;
    //修改界面
    [SerializeField] private DeviceAdjustView m_editView;
    //设备列表节点
    public Transform m_transDeviceList;
    //传感器列表根节点
    public RectTransform m_transHotPointRoot;
    
    // 
    public GameObject m_3DModel;

    /* 预制体 */
    [Tooltip("设备列表中")]
    public GameObject m_objPrefabDevice;
    [Tooltip("传感器列表中")]
    public GameObject m_objPrefabSensor;
    
    //列表管理
    private Dictionary<string, DeviceItem> m_objDeviceList = new Dictionary<string, DeviceItem>();
    private List<SensorItem> m_objSensorItemList = new List<SensorItem>();
    
    //当前选中的设备信息
    private DeviceData m_selectDInfo;
    //当前渲染的设备信息
    private DeviceData m_showDInfo;
    //控制鼠标的处理
    public MouseLook m_mouseLook;

    private float m_tick = 0.0f;
    private static readonly float PERTICK = 10; //每10s,更新一次传感器数据,可以时间更久一些

    private bool m_isRendering = false;
    public float C_RecomandZ { set; get; }
    public float C_MinZ { set; get; }
    public float C_MaxZ { set; get; }

    private InstanceRequest m_instanceRequest;

    // Start is called before the first frame update
    void Start()
    {
        //创建3DModel,用来做模型展示
        m_instanceRequest = GameEntry.Resource.InstantiateAsync(EntityAssets.Model3D);
        m_instanceRequest.completed += request =>
        {
            if (request.isDone)
            {
                var obj = request.gameObject;
                m_showMono = obj.GetComponentInChildren<ModelShowMono>();
                m_mouseLook = obj.GetComponentInChildren<MouseLook>();
                m_showMono.SetMainView(this);
                m_mouseLook.SetMainViewHandler(this);
            }
        };

        //初始化数据
        DataInterface.GetInst().InitAllDeviceData();
        
        m_editView.Init(this);
        m_editView.gameObject.SetActive(false);
        
        //创建列表
        ToCreateDeviceList(null);
        //创建传感器列表
        ToCreateSensorList();
        AddListener();
    }

    private void OnEnable()
    {
        if(m_3DModel != null)
            m_3DModel.SetActive(true);
    }

    private void OnDisable()
    {
        if (m_3DModel != null)
            m_3DModel.SetActive(false);
    }

    void AddListener()
    {
        GameEntry.Event.Subscribe(EventId.E_ReloadDeviceList, ToCreateDeviceList);
        GameEntry.Event.Subscribe(EventId.E_SelectDevice, SetSelectedDevice);
        GameEntry.Event.Subscribe(EventId.E_BeginToRender, UpdateSensorData);
        GameEntry.Event.Subscribe(EventId.E_TouchEnterSensorTips, TouchEnterSensorTips);
        GameEntry.Event.Subscribe(EventId.E_TouchExitSensorTips, TouchExitSensorTips);
        GameEntry.Event.Subscribe(EventId.E_EditDeviceDone, EditDeviceDone);
    }
    
    void RemoveListener()
    {
        GameEntry.Event.Unsubscribe(EventId.E_ReloadDeviceList, ToCreateDeviceList);
        GameEntry.Event.Unsubscribe(EventId.E_SelectDevice, SetSelectedDevice);
        GameEntry.Event.Unsubscribe(EventId.E_BeginToRender, UpdateSensorData);
        GameEntry.Event.Unsubscribe(EventId.E_TouchEnterSensorTips, TouchEnterSensorTips);
        GameEntry.Event.Unsubscribe(EventId.E_TouchExitSensorTips, TouchExitSensorTips);
        GameEntry.Event.Unsubscribe(EventId.E_EditDeviceDone, EditDeviceDone);
    }

    public void BeginRender()
    {
        m_isRendering = true;
        Shader.EnableKeyword("BEGIN_RENDER");        
    }

    public void StopRender()
    {
        m_isRendering = false;
        Shader.DisableKeyword("BEGIN_RENDER");
    }

    public void ResetPos()
    {
        if (m_showMono)
            m_showMono.ResetPos();
    }

    void EditDeviceDone(object o)
    {
        DeviceData dinfo = o as DeviceData;
        if (dinfo == null)
            return;
        if (m_showDInfo != null && m_showDInfo.name == dinfo.name) //表示修改的是当前的模型设备,需要重刷
        {
            GameEntry.Event.Fire(EventId.E_ClearAllFollowItem);
            //如果重复打开相同的,则删除之前的,重新打开一个新的
            m_showDInfo = null;
            m_showMono.ShowModel(m_showDInfo);
            m_selectDInfo = dinfo;
            m_showDInfo = dinfo;
            m_showMono.ShowModel(m_showDInfo);
            SetShowDevice(m_showDInfo);
            StopRender();
        }
    }

    private void Update()
    {
        if (m_showDInfo == null) //没有当前渲染的模型,忽略
            return;
        if (!m_isRendering)
            return;
        m_tick += Time.deltaTime;
        if (m_tick > PERTICK)
        {
            //更新
            UpdateSensorData(null);
            m_tick = 0.0f;
        }
    }

    public void UpdateSensorData(object o)
    {
        if (m_showDInfo == null)
            return;
        var sensorData = DataInterface.GetInst().GetSensorDataByDeviceKey(m_showDInfo.name);
        if (sensorData == null)
            return;
        //1.获取数据后,塞给模型
        m_showMono.SetSensorData(sensorData); //设置
        //2.更新传感器列表的数据
        UpdateSensorList(sensorData);
    }

    public void UpdateSensorList(DeviceSensorData sensorData)
    {
        foreach (var objItem in m_objSensorItemList)
        {
            var _sensorData = sensorData.allSensorData.Find(x => x.name == objItem.GetUuid());
            if (_sensorData != null)
                objItem.UpdateData(_sensorData);
            else
            {
                objItem.SetToggle(false);
            }
        }
    }
    

    void TouchEnterSensorTips(object o)
    {
        int index = 0;
        string sensorKey = o as string;
        for (int i = 0; i < m_objSensorItemList.Count; ++i)
        {
            if (m_objSensorItemList[i].GetUuid() == sensorKey)
            {
                index = i;
                m_objSensorItemList[i].SetHighLight(true);
                break;
            }
        }
        // 更细位置
        var oldPos = m_transHotPointRoot.anchoredPosition;
        m_transHotPointRoot.anchoredPosition = new Vector2(oldPos.x, 31*index);
    }

    void TouchExitSensorTips(object o)
    {
        string sensorKey = o as string;
        foreach (var sItem in m_objSensorItemList)
        {
            if (sItem.GetUuid() == sensorKey)
            {
                sItem.SetHighLight(false);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (m_instanceRequest != null)
        {
            m_instanceRequest.Destroy();
            m_instanceRequest = null;
        }

        RemoveListener();
    }

    //当前是否支持鼠标处理
    public bool IsMouseEnable()
    {
        if (m_showDInfo == null || m_editView.gameObject.activeSelf)
            return false;
        return true;
    }
    
    public void SetSelectedDevice(object data)
    {
        DeviceData dInfo = data as DeviceData;
        m_selectDInfo = dInfo;
        foreach (var dItem in m_objDeviceList)
        {
            if (dItem.Key != dInfo.name)
            {
                dItem.Value.SetInteractable(true);
            }
        }
    }

    public void SetShowDevice(object data)
    {
        DeviceData dInfo = data as DeviceData;
        m_selectDInfo = dInfo;
        foreach (var dItem in m_objDeviceList)
        {
            if (dItem.Key != dInfo.name)
            {
                dItem.Value.SetCheckBoxSelect(false);
            }
            else
            {
                dItem.Value.SetCheckBoxSelect(true);
            }
        }
    }

    //添加设备
    public void OnClickAddBtn()
    {
        Debug.Log($">>>> OnClickAddBtn");
        m_editView.ShowData();
    }
    
    //打开指定设备
    public void OnClickOpenBtn()
    {
        GameEntry.Event.Fire(EventId.E_ClearAllFollowItem);
        //如果重复打开相同的,则删除之前的,重新打开一个新的
        if (m_showDInfo != null && m_selectDInfo != null && m_showDInfo.name == m_selectDInfo.name)
        {
            m_showDInfo = null;
            m_showMono.ShowModel(m_showDInfo);
        }
        m_showDInfo = m_selectDInfo;
        if (m_showDInfo == null)
        {
            return;
        }
        //设置推荐位置
        C_RecomandZ = m_showDInfo.modelHeight / Mathf.Tan(26.0f * 0.5f * Mathf.Deg2Rad);
        C_MinZ = C_RecomandZ * 0.5f;
        C_MinZ = C_MinZ < 10 ? 10 : C_MinZ;
        C_MaxZ = C_RecomandZ * 1.5f;
        C_MaxZ = C_MaxZ > 500 ? 500 : C_MaxZ;
        m_showMono.ShowModel(m_showDInfo);
        SetShowDevice(m_showDInfo);
        StopRender();
    }
    
    // 删除指定设备
    public void OnClickDeleteBtn()
    {
        if (m_selectDInfo == null)
            return;
        //如果当前渲染的模型和要删除的是同一个,渲染也停止
        if (m_showDInfo != null && m_showDInfo.name.Equals(m_selectDInfo.name))
        {
            GameEntry.Event.Fire(EventId.E_ClearAllFollowItem);
            m_showDInfo = null;
            m_showMono.ShowModel(null);
        }

        DataInterface.GetInst().DeleteDInfoFromDB(m_selectDInfo.name);
        m_selectDInfo = null;
    }
    
    //修改指定设备
    public void OnClickAdjustBtn()
    {   if (m_editView != null && m_selectDInfo != null)
        {
            m_editView.ShowData(m_selectDInfo.name);
        }
    }
    
    // 渲染指定模型
    public void ShowModel()
    {
        if (m_selectDInfo != null)
        {
            m_showMono.ShowModel(m_selectDInfo);
        }
    }

    //创建设备列表
    public void ToCreateDeviceList(object data)
    {
        RemoveAllDeviceList();
        var allDInfo = DataInterface.GetInst().GetAllDInfo();
        foreach (var dInfo in allDInfo)
        {
            var obj = Instantiate(m_objPrefabDevice, m_transDeviceList);
            var dItem = obj.GetComponent<DeviceItem>();
            dItem.SetData(dInfo.Value);
            m_objDeviceList[dItem.GetDeviceId()] = dItem;
            //如果是当前渲染的,则显示toggle
            if (m_showDInfo != null && m_showDInfo.name == dItem.GetDeviceId())
            {
                dItem.SetCheckBoxSelect(true);
            }
            //如果是当前选中的,则高亮
            if (m_selectDInfo != null && m_selectDInfo.name == dItem.GetDeviceId())
            {
                dItem.SetInteractable(false);
            }
        }
    }

    private void RemoveAllDeviceList()
    {
        foreach (var dItem in m_objDeviceList)
        {
            DestroyImmediate(dItem.Value.gameObject);
        }
        m_objDeviceList.Clear();
    }

    private static int MaxCnt = 100;
    //创建传感器列表
    public void ToCreateSensorList()
    {
        RemoveAllSensorItem();
        List<SensorItemData> sensorData = new List<SensorItemData>();
        if (m_selectDInfo != null)
        {
            DeviceSensorData d_SensorData = DataInterface.GetInst().GetSensorDataByDeviceKey(m_selectDInfo.name);
            if (d_SensorData != null)
            {
                sensorData = d_SensorData.allSensorData;
            }
        }
        for (int i = 0; i < MaxCnt; ++i)
        {
            var obj = Instantiate(m_objPrefabSensor, m_transHotPointRoot);
            var hotPointItem = obj.GetComponent<SensorItem>();
            if (i < sensorData.Count)
            {
                hotPointItem.InitData(sensorData[i]);
                hotPointItem.SetToggle(true);
            }
            else
            {
                SensorItemData itemData = new SensorItemData();
                itemData.name = string.Format("CW{0:D2}", i);
                itemData.temperature = 0;
                hotPointItem.InitData(itemData);
            }

            m_objSensorItemList.Add(hotPointItem);
        }
    }

    //删除传感器列表
    private void RemoveAllSensorItem()
    {
        foreach (var sItem in m_objSensorItemList)
        {
            DestroyImmediate(sItem.gameObject);
        }
        m_objSensorItemList.Clear();
    }

}
