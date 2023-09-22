using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���ݴ���ˢ��
/// </summary>
public class DataManager : MonoSingleton<DataManager>
{
    /*ע�����
     * 1. ģ�����ƺ�unity��������һ��
     * 
     */
    // ����ˢ��ʱ��,Ĭ��1s
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
    /// ����Vueҳ������ģ����Ϣ
    /// <ҳ������,<ģ������,ģ��ID>>
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> TemplateInfoDict;

    /// <summary>
    /// Vue����ģ���豸��Ϣ
    /// <ģ������,<�豸����,�豸��Ϣ>>
    /// </summary>
    public Dictionary<string, Dictionary<string, TemplateData>> DeviceInfoDict;

    /// <summary>
    /// Vue����ģ���豸ֵ��Ϣ
    /// <ģ������,�豸����ֵ>
    /// </summary>
    public Dictionary<string, string[]> DeviceValueDict;

    /// <summary>
    /// Vue����ģ���豸��Ϣ(�����е�λ��)
    /// <ģ������, <�豸����,�豸����>>
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> DeviceIndexDict;

    /// <summary>
    /// �¼���Ϣ�洢
    /// <�¼�id, �¼�����>
    /// </summary>
    public Dictionary<string, TemplateEvent> DeviceEventDict;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public override void Init()
    {
        // ��ʼ��
        TemplateInfoDict = new Dictionary<string, Dictionary<string, string>>();
        DeviceInfoDict = new Dictionary<string, Dictionary<string, TemplateData>>();
        DeviceIndexDict = new Dictionary<string, Dictionary<string, int>>();
        DeviceValueDict = new Dictionary<string, string[]>();
        DeviceEventDict = new Dictionary<string, TemplateEvent>();
        // ���¼�
        OnAddListener();
        // ��ʼ����Ϣ
        StartCoroutine(InitData());
    }

    /*
     �����¼��������豸ֵ��
     */
    private float _valueUpdateTimeSum = 0.0f;
    private float _eventUpdateTimeSum = -0.5f;
    private bool _isUpdate = false; //�Ƿ�ˢ������

    /// <summary>
    /// �����Ƿ�ˢ��
    /// </summary>
    /// <param name="b">����ˢ��״̬</param>
    public void SetIsUpdate(bool b)
    {
        _isUpdate = b;
    }

    /// <summary>
    /// �̶�1sˢ��һ��
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
    //    // �Ƿ�ˢ��
    //    if (!_isUpdate) return;

    //    // ˢ��ֵ
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

    //    // ˢ���¼�
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
    /// ע���¼�
    /// </summary>
    private void OnAddListener()
    {
        // �󶨸����¼�
        if (m_isUpdate != null)
        {
            m_isUpdate.onClick.RemoveAllListeners();
            m_isUpdate.onClick.AddListener(() => {
                _isUpdate = !_isUpdate;
                var t = m_isUpdate.GetComponentInChildren<Text>();
                if (t != null)
                    t.text = _isUpdate ? "�ر�ˢ��" : "����ˢ��";
            });
        }
        // �󶨵���ҳ���¼�
        if (m_debugButton != null && m_debugWindow != null)
        {
            m_debugButton.onClick.RemoveAllListeners();
            m_debugButton.onClick.AddListener(() => {
                m_debugWindow.SetActive(!m_debugWindow.activeSelf);
                var t = m_debugButton.GetComponentInChildren<Text>();
                if (t != null)
                    t.text = m_debugWindow.activeSelf ? "�رմ���" : "�򿪴���";
            });
        }
    }

    private List<string> _mianMenuNameList;     // ��������������
        
    /// <summary>
    /// ��ʼ��vue��Ϣ��unity ui��Ϣ
    /// </summary>
    private IEnumerator InitData()
    {
        //----------------------��ȡ����----------------------//
        const int INITCOUNT = 10; // �������������
        const float _intervalSendAndRecv = 1f;  // �ӿ��������ʱ����

        // ��ʱ5����ˢ��
        yield return new WaitForSecondsRealtime(3f);    

        // ��ȡ<ʵʱ����>ҳ��ģ����Ϣ
        StartCoroutine(GetData());

        // ʵʱ���ݻ�ȡ
        IEnumerator GetData()
        {
            // ����
            for (int i = 0; i < INITCOUNT; i++)
            {
                var pageName = "ʵʱ����";
                VueData.GetTemplateSend(pageName);
                yield return new WaitForSecondsRealtime(_intervalSendAndRecv);
                var templates = VueData.GetTemplateRecv();
                if (templates != null)
                {
                    // ����<ʵʱ����>ҳ��ģ����Ϣ
                    var realTimeDataTemplateDict = new Dictionary<string, string>();
                    foreach (var item in templates)
                    {
                        if (!realTimeDataTemplateDict.ContainsKey(item.templateName))
                        {
                            Debug.Log($"ʵʱ����ģ����Ϣ��{item.templateName}|{item.templateid}");
                            realTimeDataTemplateDict.Add(item.templateName, item.templateid);
                        }
                    }

                    // ��Ӻ��޸�Ϊ����(Ϊ�ֶ�ˢ��׼��)
                    if (TemplateInfoDict.ContainsKey(pageName))
                    {
                        TemplateInfoDict[pageName] = realTimeDataTemplateDict;
                    }
                    else
                    {
                        TemplateInfoDict.Add(pageName, realTimeDataTemplateDict);
                    }

                    // ��ȡ<ʵʱ����>ҳ��ģ������������豸��Ϣ
                    foreach (var template in TemplateInfoDict[pageName])
                    {
                        // <ʵʱ����>ģ��id,name(Ӧ��unityҳ�汣��һ��)
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
                                        Debug.LogError($"<{pageName}>ҳ��<{template.Value}>ģ���ȡ�豸��Ϣʧ��.");
                                    }
                                }
                            }
                        }
                    }
                    // <ʵʱ�¼�>ҳ��ģ����Ϣ��ȡ
                    StartCoroutine(GetDeviceEvent());
                    // ����ѭ��
                    break;
                }
                else
                {
                    if (i == INITCOUNT - 1)
                    {
                        Debug.LogError($"Unity��ʼ��<{pageName}>ҳ��ģ���ȡʧ��.");
                        _isUpdate = false;  // ����������ˢ��
                    }
                }
            }
        }

        // �¼���ȡ
        IEnumerator GetDeviceEvent()
        {
            var pageName = "ʵʱ�¼�";
            // ����
            for (int i = 0; i < INITCOUNT; i++)
            {

                VueData.GetTemplateSend(pageName);
                yield return new WaitForSecondsRealtime(_intervalSendAndRecv);
                var templatesEvent = VueData.GetTemplateRecv();

                if (templatesEvent != null)
                {
                    // ����<ʵʱ�¼�>ҳ��ģ����Ϣ
                    var eventTemplateDict = new Dictionary<string, string>();
                    foreach (var item in templatesEvent)
                    {
                        if (!eventTemplateDict.ContainsKey(item.templateName))
                        {
                            Debug.Log($"�¼����棺{item.templateName}|{item.templateid}");
                            eventTemplateDict.Add(item.templateName, item.templateid);
                        }
                    }
                    // ��Ӻ��޸�Ϊ����(Ϊ�ֶ�ˢ��׼��)
                    if (TemplateInfoDict.ContainsKey(pageName))
                    {
                        TemplateInfoDict[pageName] = eventTemplateDict;
                    }
                    else
                    {
                        TemplateInfoDict.Add(pageName, eventTemplateDict);
                    }
                    // ��ʼ��������
                    SetIsUpdate(true);
                    //�˳�ѭ��
                    break;
                }
                else
                {
                    if (i == INITCOUNT - 1)
                    {
                        Debug.LogError($"Unity��ʼ��<{pageName}>ҳ��ģ���ȡʧ��.");
                    }
                }
            }
        }

        // ��ȡunity��������������
        _mianMenuNameList = new List<string>();
        foreach (var item in WindowManager.Instance.AllMainWindowDict.Keys)
        {
            _mianMenuNameList.Add(item);
        }
    }

    /// <summary>
    /// ��ȡ��ҳ����豸��Ϣ
    /// </summary>
    private void UpdateValue()
    {
        var pageName = "ʵʱ����";
        // ��ȡ<ʵʱ����>ҳ�浱ǰֵ
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
                        Debug.Log($"<{pageName}>ҳ��<{windowName}>ģ���ȡ�豸ֵ��Ϣʧ��");
                        return;
                    }
                    // ��������
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
    /// ���´򿪼���ҳ����豸��Ϣ
    /// </summary>
    private void UpdateUI()
    {
        var allWinInfo = WindowManager.Instance.AllMainWindowDict;
        for (int i = 0; i < _mianMenuNameList.Count; i++)
        {
            var name = _mianMenuNameList[i];
            var objWin = allWinInfo[name];
            // �������Ѵ򿪼����ҳ��
            if (objWin.IsActive 
                    && DeviceValueDict.TryGetValue(name,out var devicesInfo)
                    && DeviceIndexDict.TryGetValue(name, out var devicesIndex)
                )
            {
                for (int j = 0; j < objWin.AllDevices.Count; j++)
                {
                    var deviceBase = objWin.AllDevices[j];
                    var type = deviceBase.GetType();
                    // valve����
                    if (type == typeof(DeviceValve))
                    {
                        var device = deviceBase as DeviceValve;
                        // ��ɫ״̬
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    // pump����
                    if (type == typeof(DevicePump))
                    {
                        var device = deviceBase as DevicePump;
                        // ��ɫ״̬
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    // coolpump����
                    if (type == typeof(DeviceCoolPump))
                    {
                        var device = deviceBase as DeviceCoolPump;
                        // ��ɫ״̬
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out var index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                    }

                    //ģ��������
                    if (type == typeof(DeviceAI))
                    {
                        var device = deviceBase as DeviceAI;
                        int index;
                        //��ɫ״̬
                        if (devicesIndex.TryGetValue(device.EqName + _fnState, out index)
                            && index < devicesInfo.Length)
                        {
                            int.TryParse(devicesInfo[index], out var value);
                            device.SetStateColor(value);
                        }
                        //��ǰֵ
                        if (devicesIndex.TryGetValue(device.EqName + _fnValue, out index)
                            && index < devicesInfo.Length)
                        {
                            // ��ʽ�ж�
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
                "BZ1800.ʵʱ�¼�",
                //"BZ1000.ʵʱ�¼�"
            };
    /// <summary>
    /// ����ʵʱ�¼�
    /// </summary>
    protected void UpdateDeviceEvent()
    {
        // ���
        DeviceEventDict.Clear();
        
        // ˢ���¼�
        _sendEventInfo.pagename = "ʵʱ�¼�";
        for (int i = 0; i < _snedEventTemplate.Length; i++)
        {
            var templateName = _snedEventTemplate[i];
            if (TemplateInfoDict[_sendEventInfo.pagename].TryGetValue(templateName, out var id))
            {
                // ��ȡ�¼���Ϣ
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

                    // UI����>������ʾ����
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
                Debug.LogError($"������ <{templateName}> ģ��.");
            }
        }
    }


    /// <summary>
    /// ��ȡָ���豸ָ������ֵ
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
    /// ��ȡָ���豸��Ϣ
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
    /// �����豸����
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public string SetTemplateValue(SetTemplateValues item)
    {
        VueData.SetTemplateValueSend(item);
        return VueData.SetTemplateValueRecv();
    }

    /// <summary>
    /// ���ʵʱ�¼�
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public TemplateEvent[] GetTemplateEvent(TemplateDataItem item)
    {
        VueData.GetTemplateEventSend(item);
        return VueData.GetTemplateEventRecv();
    }

    /// <summary>
    /// ȷ��ʵʱ�¼�
    /// </summary>
    /// <param windowName="item"></param>
    /// <returns></returns>
    public string SetTemplateEvent(SetTemplateEvent item)
    {
        VueData.SetTemplateEventSend(item);
        return VueData.SetTemplateEventRecv();
    }

    /// <summary>
    /// ��õ�¼��
    /// </summary>
    /// <returns></returns>
    public void GetUserName()
    {
        VueData.GetUserNameSend();
        m_loginName.text = VueData.GetUserNameRecv();
    }

    public void SetLogout()
    {
        m_loginName.text = "���¼!";
        VueData.SetLogout();
    }


    /// <summary>
    /// ������Ļ���
    /// </summary>
    /// <param name="isMax"></param>
    public void SetWindowMax(bool isMax)
    {
        VueData.SetWindowMax(isMax);
    }
}
