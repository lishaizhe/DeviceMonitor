using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VueData:MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern string UnityDataSend(string type, string item);
    [DllImport("__Internal")]
    public static extern string UnityDataRecv(string type);
    [DllImport("__Internal")]
    public static extern void SetWindowMax(bool isMax);

#endif

    private static string item;

    public static void UnityDataSend(string t, string i)
    {
        item = i;
    }
    public static string UnityDataRecv(string type)
    {
        switch (type)
        {
            case "GetTemplate":
                return VueDataSim.GetTemplates(item);
            case "GetTemplateData":
                return VueDataSim.GetTemplateDatas(item);
            case "GetTemplateValue":
                return VueDataSim.GetTemplateValue(item);
            case "SetTemplateValue":
                return VueDataSim.SetTemplateValue(item);
            case "GetTemplateEvent":
                return VueDataSim.GetTemplateEvent(item);
            case "SetTemplateEvent":
                return VueDataSim.SetTemplateEvent(item);
            case "Login":
                return VueDataSim.GetLogin(item);
            case "Loginout":
                return VueDataSim.SetLogout(item);
            default:
                return "";
        }
    }

    public static void SetWindowMax(bool isMax)
    {
        Debug.Log(isMax?"���": "��С��");
    }

    #region 1.���ģ����Ϣ
    // GetTemplatesSend��GetTemplatesRecv���ʹ��

    /// <summary>
    /// ���ָ��ҳ��ģ����Ϣ
    /// </summary>
    /// <param name="pageName">ҳ������(ʵʱ����)</param>
    /// <returns></returns>
    public static void GetTemplateSend(string pageName)
    {
        var type = "GetTemplate";
        var item = new TemplateItem();
        item.pagename = pageName;
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateSend ���ͣ�{send}");
        UnityDataSend(type, send);
    }
    public static Template[] GetTemplateRecv()
    {
        var recv = UnityDataRecv("GetTemplate");
        Debug.Log($"GetTemplateRecv ���գ�{recv}");
        Template[] result;
        try
        {
            result = JsonConvert.DeserializeObject<Template[]>(recv);
        }
        catch
        {
            result = null;
        }
        Debug.Log($"GetTemplateRecv �����л���{result == null}|{result.Length}");
        return result;
    }
    #endregion

    #region 2.��ö�Ӧģ������
    /// <summary>
    /// ���ָ��ģ����Ϣ
    /// </summary>
    /// <param name="pageName">ҳ������</param>
    /// <param name="templateid">ģ��id</param>
    /// <returns></returns>
    public static void GetTemplateDataSend(string pageName, string templateid)
    {
        var type = "GetTemplateData";
        var item = new TemplateDataItem();
        item.pagename = pageName;
        item.templateid = templateid;
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateDataSend ���ͣ�{send}");
        UnityDataSend(type, send);
    }
    public static TemplateData[] GetTemplateDataRecv()
    {
        var recv = UnityDataRecv("GetTemplateData");
        Debug.Log($"GetTemplateDataRecv ���գ�{recv}");
        TemplateData[] result;
        try
        {
            result = JsonConvert.DeserializeObject<TemplateData[]>(recv);
        }
        catch
        {
            result = null;
        }
        Debug.Log($"GetTemplateDataRecv �����л���{result == null}|{result?.Length}");
        return result;
    }
    #endregion

    #region 3.��ö�Ӧģ��ֵ
    /// <summary>
    /// ���ָ��ģ��ֵ��Ϣ
    /// </summary>
    /// <param name="pageName"></param>
    /// <param name="templateid"></param>
    /// <returns></returns>
    public static void GetTemplateValueSend(string pageName, string templateid)
    {
        var type = "GetTemplateValue";
        var item = new TemplateDataItem();
        item.pagename = pageName;
        item.templateid = templateid;
        var send = JsonConvert.SerializeObject(item);
        UnityDataSend(type, send);
        Debug.Log($"GetTemplateValueSend ���ͣ�{send}");
    }
    public static string[] GetTemplateValueRecv()
    {
        var recv = UnityDataRecv("GetTemplateValue");
        Debug.Log($"GetTemplateValueRecv ���գ�{recv}");
        string[] result;
        try
        {
            result = JsonConvert.DeserializeObject<string[]>(recv);
        }
        catch
        {
            Debug.Log("GetTemplateValueRecv ���л�ʧ��");
            result = null;
        }
        Debug.Log($"GetTemplateValueRecv �����л���{result == null}|{result.Length}");
        return result;
    }
    #endregion


    #region 4.�����豸����
    /// <summary>
    /// ����ָ��ģ����Ϣ
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static void SetTemplateValueSend(SetTemplateValues item)
    {
        var type = "SetTemplateValue";
        UnityDataSend(type, JsonConvert.SerializeObject(item));
        
    }
    public static string SetTemplateValueRecv()
    {
        return UnityDataRecv("SetTemplateValue");
    }
    #endregion

    #region 5.��ȡʵʱ�¼���Ϣ
    /// <summary>
    /// ��ȡʵʱ�¼���Ϣ
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static void GetTemplateEventSend(TemplateDataItem item)
    {
        var type = "GetTemplateEvent";
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateValueSend ���ͣ�{send}");
        UnityDataSend(type, send);
    }
    public static TemplateEvent[] GetTemplateEventRecv()
    {
        var recv = UnityDataRecv("GetTemplateEvent");
        Debug.Log($"GetTemplateEventRecv ���գ�{recv}");
        TemplateEvent[] result;
        try
        {
            result = JsonConvert.DeserializeObject<TemplateEvent[]>(recv);
        }
        catch
        {
            Debug.Log("GetTemplateEventRecv ���л�ʧ��");
            result = null;
        }
        Debug.Log($"GetTemplateEventRecv �����л���{result == null}|{result.Length}");
        return result;
    }
    #endregion

    #region 6.ȷ���¼�
    /// <summary>
    /// ȷ��ʵʱ�¼�
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static void SetTemplateEventSend(SetTemplateEvent item)
    {
        var type = "SetTemplateEvent";
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"SetTemplateEventSend ���ͣ�{send}");
        UnityDataSend(type, send);
    }
    public static string SetTemplateEventRecv()
    {
        var result = UnityDataRecv("SetTemplateEvent");
        Debug.Log($"SetTemplateEventRecv �����{result}");
        return result;
    }
    #endregion

    #region 7.��õ�¼��Ϣ
    /// <summary>
    /// ��õ�¼������
    /// </summary>
    public static void GetUserNameSend()
    {
        var type = "Login";
        UnityDataSend(type, "");
    }
    public static string GetUserNameRecv()
    {
        LoginName data;
        try
        {
            data = JsonConvert.DeserializeObject<LoginName>(UnityDataRecv("Login"));
        }
        catch
        {
            data = null;
        }
        return data?.name ?? "���¼!";
    }
    #endregion


    /// <summary>
    /// �˳���¼
    /// </summary>
    public static void SetLogout()
    {
        UnityDataSend("Logout", "");
    }

    /// <summary>
    /// ����¼�������ַ
    /// </summary>
    /// <returns>������ַ</returns>
    public static string GetEventSound()
    {
        var type = "EventSound";
        UnityDataSend(type, "");
        return UnityDataRecv(type);
    }


    /// <summary>
    /// ��JSON�ļ��������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath">StreamingAssets�ļ������ļ�·��,
    /// ���磺/devicesInfo/xxx.json</param>
    /// <returns></returns>
    public static T TestGetDataFromJson<T>(string filePath)
    {
        string jsonData;
        string filepath = Application.streamingAssetsPath + filePath;
        using (StreamReader reader = new StreamReader(filepath))
        {
            jsonData = reader.ReadToEnd();
            reader.Close();
        }
        return JsonConvert.DeserializeObject<T>(jsonData);
    }
}

//----------------------------------JSON----------------------------------//

[Serializable]
public class TemplateItem
{
    public string pagename;
}

[Serializable]
public class TemplateDataItem
{
    public string pagename;
    public string templateid;
}

[Serializable]
public class Template
{
    public string templateid;
    public string templateName;
}


[Serializable]
public class TemplateData
{
    public string eqid;
    public string fieldid;
    public string name;
    public string fieldName;
}

[Serializable]
public class SetTemplateValue
{
    public string eqid;
    public string fieldid;
    public string value;
}

public class SetTemplateValues
{
    public string pagename;
    public List<SetTemplateValue> set = new List<SetTemplateValue>();
}

[Serializable]
public class TemplateEvent
{
    public string id;
    public string starttime;
    public string endtime;
    public string name;
    public string allremarks;
    public string fieldrealname;
    public string msg;
}

[Serializable]
public class SetTemplateEvent
{
    public string pagename;
    public List<string> set = new List<string>();
}

[Serializable]
public class LoginName
{ 
    public string name;
}