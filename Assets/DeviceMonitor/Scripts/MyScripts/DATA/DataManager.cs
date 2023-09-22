using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 数据处理刷新
/// </summary>
public class DataManager : MonoSingleton<DataManager>
{
    /*注意事项：
     * 1. 模板名称和unity界面名称一致
     * 
     */
    // 界面刷新时间,默认1s
    [Header("Settings")]
    [Range(0.5f, 3f)]
    [SerializeField] private float m_dataUpdateTime = 1f;

    [Header("Login")]
    [SerializeField] private TextMeshProUGUI m_loginName;

    [Header("Event")]
    [Range(0.5f, 3f)]
    public float m_eventUpdateTime = 1f;
    [SerializeField] private GameObject m_eventCount;

    [Header("Test")]
    [SerializeField] private Button m_isUpdate;
    [SerializeField] private Button m_debugButton;

    [SerializeField] private GameObject m_debugWindow;
    public InputField m_Result;

    /// <summary>
    /// 来自Vue页面所有模板信息
    /// <页面名称,<模板名称,模板ID>>
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> TemplateInfoDict;

    /// <summary>
    /// Vue所有模板设备信息
    /// <模板名称,<设备名称,设备信息>>
    /// </summary>
    public Dictionary<string, Dictionary<string, TemplateData>> DeviceInfoDict;

    /// <summary>
    /// Vue所有模板设备值信息
    /// <模板名称,设备属性值>
    /// </summary>
    public Dictionary<string, string[]> DeviceValueDict;

    /// <summary>
    /// Vue所有模板设备信息(数组中的位置)
    /// <模板名称, <设备名称,设备索引>>
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> DeviceIndexDict;

    /// <summary>
    /// 事件信息存储
    /// <事件id, 事件对象>
    /// </summary>
    public Dictionary<string, TemplateEvent> DeviceEventDict;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        // 初始化
        TemplateInfoDict = new Dictionary<string, Dictionary<string, string>>();
        DeviceInfoDict = new Dictionary<string, Dictionary<string, TemplateData>>();
        DeviceIndexDict = new Dictionary<string, Dictionary<string, int>>();
        DeviceValueDict = new Dictionary<string, string[]>();
        DeviceEventDict = new Dictionary<string, TemplateEvent>();
        // 绑定事件
        OnAddListener();
        // 初始化信息
        StartCoroutine(InitData());
    }

    /*
     请求事件和请求设备值错开
     */
    private float _valueUpdateTimeSum = 0.0f;
    private float _eventUpdateTimeSum = -0.5f;
    private bool _isUpdate = false; //是否刷新数据

    /// <summary>
    /// 设置是否刷新
    /// </summary>
    /// <param name="b">设置刷新状态</param>
    public void SetIsUpdate(bool b)
    {
        _isUpdate = b;
    }

    /// <summary>
    /// 固定1s刷新一次
    /// </summary>
    private void FixedUpdate()
    {
        if (_isUpdate)
        { 
            UpdateValue();
            UpdateUI();
            UpdateDeviceEvent();
        }
    }

    //private void Update()
    //{
    //    // 是否刷新
    //    if (!_isUpdate) return;

    //    // 刷新值
    //    _valueUpdateTimeSum += Time.deltaTime;
    //    if (_valueUpdateTimeSum > m_dataUpdateTime)
    //    {
    //        Debug.Log("--------------------------1" + ":" + _valueUpdateTimeSum);
    //        Debug.Log("--------------------------1" + ":" + _valueUpdateTimeSum);
    //        UpdateValue();
    //        UpdateUI();
    //        Debug.Log("--------------------------2" + ":" + _valueUpdateTimeSum);
    //        Debug.Log("--------------------------2" + ":" + _valueUpdateTimeSum);
    //        _valueUpdateTimeSum = 0f;

    //    }

    //    // 刷新事件
    //    _eventUpdateTimeSum += Time.deltaTime;
    //    if (_eventUpdateTimeSum > m_eventUpdateTime)
    //    {
    //        UpdateDeviceEvent();
    //        Debug.Log("--------------------------3" + ":" + _valueUpdateTimeSum);
    //        Debug.Log("--------------------------3" + ":" + _eventUpdateTimeSum);
    //        _eventUpdateTimeSum = 0f;

    //    }
    //}

    /// <summary>
    /// 注册事件
    /// </summary>
    private void OnAddListener()
    {
        // 绑定更新事件
        if (m_isUpdate != null)
        {
            m_isUpdate.onClick.RemoveAllListeners();
            m_isUpdate.onClick.AddListener(() => {
                _isUpdate = !_isUpdate;
                var t = m_isUpdate.GetComponentInChildren<Text>();
                if (t != null)
                    t.text = _isUpdate ? "关闭刷新" : "开启刷新";
            });
        }
        // 绑定调试页面事件
        if (m_debugButton != null && m_debugWindow != null)
        {
            m_debugButton.onClick.RemoveAllListeners();
            m_debugButton.onClick.AddListener(() => {
                m_debugWindow.SetActive(!m_debugWindow.activeSelf);
                var t = m_debugButton.GetComponentInChildren<Text>();
                if (t != null)
                    t.text = m_debugWindow.activeSelf ? "关闭窗口" : "打开窗口";
            });
        }
    }

    private List<string> _mianMenuNameList;     // 所有主界面名称
        
    /// <summary>
    /// 初始化vue信息和unity ui信息
    /// </summary>
    private IEnumerator InitData()
    {
        //----------------------获取数据----------------------//
        const int INITCOUNT = 10; // 最大请求出错次数
        const float _intervalSendAndRecv = 1f;  // 接口请求接受时间间隔

        // 延时5秒钟刷新
        yield return new WaitForSecondsRealtime(3f);    

        // 获取<实时数据>页面模板信息
        StartCoroutine(GetData());

        // 实时数据获取
        IEnumerator GetData()
        {
            // 连接
            for (int i = 0; i < INITCOUNT; i++)
            {
                var pageName = "实时数据";
                VueData.GetTemplateSend(pageName);
                yield return new WaitForSecondsRealtime(_intervalSendAndRecv);
                var templates = VueData.GetTemplateRecv();
                if (templates != null)
                {
                    // 保存<实时数据>页面模板信息
                    var realTimeDataTemplateDict = new Dictionary<string, string>();
                    foreach (var item in templates)
                    {
                        if (!realTimeDataTemplateDict.ContainsKey(item.templateName))
                        {
                            Debug.Log($"实时数据模板信息：{item.templateName}|{item.templateid}");
                            realTimeDataTemplateDict.Add(item.templateName, item.templateid);
                        }
                    }

                    // 添加和修改为最新(为手动刷新准备)
                    if (TemplateInfoDict.ContainsKey(pageName))
                    {
                        TemplateInfoDict[pageName] = realTimeDataTemplateDict;
                    }
                    else
                    {
                        TemplateInfoDict.Add(pageName, realTimeDataTemplateDict);
                    }

                    // 获取<实时数据>页面模板包含的所有设备信息
                    foreach (var template in TemplateInfoDict[pageName])
                    {
                        // <实时数据>模板id,name(应和unity页面保持一致)
                        var id = template.Value;
                        var name = template.Key;

                        //StartCoroutine(GetTemplateData());
                        yield return GetTemplateData();

                        //GetTemplateData();
                        IEnumerator GetTemplateData()
                        {
                            for (int j = 0; j < INITCOUNT; j++)
                            {
                                VueData.GetTemplateDataSend(pageName, id);
                                yield return new WaitForSecondsRealtime(_intervalSendAndRecv);
                                var data = VueData.GetTemplateDataRecv();
                                if (data != null)
                                {
                                    var deviceInfoDict = new Dictionary<string, TemplateData>();
                                    var deviceIndexDict = new Dictionary<string, int>();
                                    var index = 0;
                                    foreach (var item in data)
                                    {
                                        var key = item.name + item.fieldName;
                                        if (!deviceInfoDict.ContainsKey(key))
                                        {
                                            deviceInfoDict.Add(key, item);
                                            deviceIndexDict.Add(key, index);
                                            index++;
                                        }
                                    }
                                    if (!DeviceInfoDict.ContainsKey(name))
                                    {
                                        DeviceInfoDict.Add(name, deviceInfoDict);
                                        DeviceIndexDict.Add(name, deviceIndexDict);
                                    }

                                    break;
                                }
                                else
                                {
                                    if (j == INITCOUNT - 1)
                                    {
                                        Debug.LogError($"<{pageName}>页面<{template.Value}>模板获取设备信息失败.");
                                    }
                                }
                            }
                        }
                    }
                    // <实时事件>页面模板信息获取
                    StartCoroutine(GetDeviceEvent());
                    // 跳出循环
                    break;
                }
                else
                {
                    if (i == INITCOUNT - 1)
                    {
                        Debug.LogError($"Unity初始化<{pageName}>页面模板获取失败.");
                        _isUpdate = false;  // 不启动数据刷新
                    }
                }
            }
        }

        // 事件获取
        IEnumerator GetDeviceEvent()
        {
            var pageName = "实时事件";
            // 连接
            for (int i = 0; i < INITCOUNT; i++)
            {

                VueData.GetTemplateSend(pageName);
                yield return new WaitForSecondsRealtime(_intervalSendAndRecv);
                var templatesEvent = VueData.GetTemplateRecv();

                if (templatesEvent != null)
                {
                    // 保存<实时事件>页面模板信息
                    var eventTemplateDict = new Dictionary<string, string>();
                    foreach (var item in templatesEvent)
                    {
                        if (!eventTemplateDict.ContainsKey(item.templateName))
                        {
                            Debug.Log($"事件保存：{item.templateName}|{item.templateid}");
                            eventTemplateDict.Add(item.templateName, item.templateid);
                        }
                    }
                    // 添加和修改为最新(为手动刷新准备)
                    if (TemplateInfoDict.ContainsKey(pageName))
                    {
                        TemplateInfoDict[pageName] = eventTemplateDict;
                    }
                    else
                    {
                        TemplateInfoDict.Add(pageName, eventTemplateDict);
                    }
                    // 开始更新数据
                    SetIsUpdate(true);
                    //退出循环
                    break;
                }
                else
                {
                    if (i == INITCOUNT - 1)
                    {
                        Debug.LogError($"Unity初始化<{pageName}>页面模板获取失败.");
                    }
                }
            }
        }

        // 获取unity所有主界面名称
        _mianMenuNameList = new List<string>();
        foreach (var item in WindowManager.Instance.AllMainWindowDict.Keys)
        {
            _mianMenuNameList.Add(item);
        }
    }

    /// <summary>
    /// 获取打开页面的设备信息
    /// </summary>
    private void UpdateValue()
    {
        var pageName = "实时数据";
        // 获取<实时数据>页面当前值
        for (int i = 0; i < _mianMenuNameList.Count; i++)
        {
            var windowName = _mianMenuNameList[i];
            if (WindowManager.Instance.AllMainWindowDict[windowName].IsActive)
            {
                if (TemplateInfoDict[pageName].TryGetValue(windowName, out var templateId))
                {
                    VueData.GetTemplateValueSend(pageName, templateId);
                    var templates = VueData.GetTemplateValueRecv();
                    if (templates == null)
                    {
                        Debug.Log($"<{pageName}>页面<{windowName}>模板获取设备值信息失败");
                        return;
                    }
                    // 更新数据
                    if (DeviceValueDict.ContainsKey(windowName))
                    {
                        DeviceValueDict[windowName] = templates;
                    }
                    else
                    {
                        DeviceValueDict.Add(windowName, templates);
                    }
                }
            }
        }
    }

    // fieldName
    private string _fnState = "State";
    private string _fnValue = "ValCur";

    /// <summary>
    /// 更新打开激活页面的设备信息
    /// </summary>
    private void UpdateUI()
    {
        var allWinInfo = WindowManager.Instance.AllMainWindowDict;
        for (int i = 0; i < _mianMenuNameList.Count; i++)
        {
            var name = _mianMenuNameList[i];
            var objWin = allWinInfo[name];
            // 仅更新已打开激活的页面
            if (objWin.IsActive 
                    && DeviceValueDict.TryGetValue(name,out var devicesInfo)
                    && DeviceIndexDict.TryGetValue(name, out var devicesIndex)
                )
            {
                for (int j = 0; j < objWin.AllDevices.Count; j++)
                {
                    var deviceBase = objWin.AllDevices[j];
                    var type = deviceBase.GetType();
                    // valve类型
                    if (type == typeof(DeviceValve))
                    {
                        var device = deviceBase as DeviceValve;
                        // 颜色状态
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    // pump类型
                    if (type == typeof(DevicePump))
                    {
                        var device = deviceBase as DevicePump;
                        // 颜色状态
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    // coolpump类型
                    if (type == typeof(DeviceCoolPump))
                    {
                        var device = deviceBase as DeviceCoolPump;
                        // 颜色状态
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    //模拟量类型
                    if (type == typeof(DeviceAI))
                    {
                        var device = deviceBase as DeviceAI;
                        int index;
                        //颜色状态
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                        //当前值
                        if (devicesIndex.TryGetValue(device.EqName + _fnValue, out index)
                            && index < devicesInfo.Length)
                        {
                            // 格式判断
                            if (float.TryParse(devicesInfo[index], out var value))
                            {
                                var absValue = System.Math.Abs(value);
                                if (absValue < 0.001 && absValue > 0 || absValue > 10000)
                                {
                                    device.SetCurrentValue(value.ToString("E1"));
                                }
                                else
                                { 
                                    device.SetCurrentValue(value.ToString("F1"));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private Text _eventCount;
    private TemplateDataItem _sendEventInfo = new TemplateDataItem();
    private string[] _snedEventTemplate = new string[]{
                "BZ1800.实时事件",
                //"BZ1000.实时事件"
            };
    /// <summary>
    /// 更新实时事件
    /// </summary>
    protected void UpdateDeviceEvent()
    {
        // 清空
        DeviceEventDict.Clear();
        
        // 刷新事件
        _sendEventInfo.pagename = "实时事件";
        for (int i = 0; i < _snedEventTemplate.Length; i++)
        {
            var templateName = _snedEventTemplate[i];
            if (TemplateInfoDict[_sendEventInfo.pagename].TryGetValue(templateName, out var id))
            {
                // 获取事件信息
                _sendEventInfo.templateid = id;
                var eventInfo = GetTemplateEvent(_sendEventInfo);
                if (eventInfo != null)
                {
                    for (int j = 0; j < eventInfo.Length; j++)
                    {
                        var e = eventInfo[j];
                        if (DeviceEventDict.ContainsKey(e.id))
                        {
                            DeviceEventDict[e.id] = e;
                        }
                        else
                        {
                            DeviceEventDict.Add(e.id, e);
                        } 
                    }

                    // UI界面>计数显示更新
                    if (DeviceEventDict.Count < 1)
                    {
                        m_eventCount.SetActive(false);
                    }
                    else
                    {
                        if (_eventCount == null)
                        {
                            _eventCount = m_eventCount.GetComponentInChildren<Text>();
                        }
                        var count = DeviceEventDict.Count;
                        _eventCount.text = count < 100 ? count.ToString() : "99+";
                        m_eventCount.SetActive(count > 0 ? true : false);
                    }
                }
            }
            else
            {
                Debug.LogError($"不存在 <{templateName}> 模板.");
            }
        }
    }


    /// <summary>
    /// 获取指定设备指定属性值
    /// </summary>
    /// <param windowName="eqName"></param>
    /// <param windowName="fieldName"></param>
    /// <returns></returns>
    public string GetDeviceValue(string eqName, string feildName)
    {
        foreach (var item in DeviceIndexDict)
        {
            if (item.Value.TryGetValue(eqName + feildName, out var index))
            {
                if (index < DeviceValueDict[item.Key].Length)
                {
                    return DeviceValueDict[item.Key][index];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取指定设备信息
    /// </summary>
    /// <param windowName="eqName"></param>
    /// <param windowName="fieldName"></param>
    /// <returns></returns>
    public TemplateData GetDeviceInfo(string eqName, string feildName)
    {
        foreach (var item in DeviceInfoDict)
        {
            if (item.Value.TryGetValue(eqName + feildName, out var info))
            {
                return info;
            }
        }
        return null;
    }

    /// <summary>
    /// 设置设备数据
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public string SetTemplateValue(SetTemplateValues item)
    {
        VueData.SetTemplateValueSend(item);
        return VueData.SetTemplateValueRecv();
    }

    /// <summary>
    /// 获得实时事件
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public TemplateEvent[] GetTemplateEvent(TemplateDataItem item)
    {
        VueData.GetTemplateEventSend(item);
        return VueData.GetTemplateEventRecv();
    }

    /// <summary>
    /// 确认实时事件
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public string SetTemplateEvent(SetTemplateEvent item)
    {
        VueData.SetTemplateEventSend(item);
        return VueData.SetTemplateEventRecv();
    }

    /// <summary>
    /// 获得登录名
    /// </summary>
    /// <returns></returns>
    public void GetUserName()
    {
        VueData.GetUserNameSend();
        m_loginName.text = VueData.GetUserNameRecv();
    }

    public void SetLogout()
    {
        m_loginName.text = "请登录!";
        VueData.SetLogout();
    }


    /// <summary>
    /// 设置屏幕最大化
    /// </summary>
    /// <param name="isMax"></param>
    public void SetWindowMax(bool isMax)
    {
        VueData.SetWindowMax(isMax);
    }
}
