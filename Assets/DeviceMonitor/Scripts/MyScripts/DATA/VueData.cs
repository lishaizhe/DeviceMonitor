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
#if UNITY_EDITOR
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
        Debug.Log(isMax?"最大化": "最小化");
    }
#endif

    #region 1.获得模板信息
    // GetTemplatesSend和GetTemplatesRecv配合使用

    /// <summary>
    /// 获得指定页面模板信息
    /// </summary>
    /// <param name="pageName">页面名称(实时数据)</param>
    /// <returns></returns>
    public static void GetTemplateSend(string pageName)
    {
        var type = "GetTemplate";
        var item = new TemplateItem();
        item.pagename = pageName;
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateSend 发送：{send}");
        UnityDataSend(type, send);
    }
    public static Template[] GetTemplateRecv()
    {
        var recv = UnityDataRecv("GetTemplate");
        Debug.Log($"GetTemplateRecv 接收：{recv}");
        Template[] result;
        try
        {
            result = JsonConvert.DeserializeObject<Template[]>(recv);
        }
        catch
        {
            result = null;
        }
        Debug.Log($"GetTemplateRecv 反序列化：{result == null}|{result.Length}");
        return result;
    }
    #endregion

    #region 2.获得对应模板内容
    /// <summary>
    /// 获得指定模板信息
    /// </summary>
    /// <param name="pageName">页面名称</param>
    /// <param name="templateid">模板id</param>
    /// <returns></returns>
    public static void GetTemplateDataSend(string pageName, string templateid)
    {
        var type = "GetTemplateData";
        var item = new TemplateDataItem();
        item.pagename = pageName;
        item.templateid = templateid;
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateDataSend 发送：{send}");
        UnityDataSend(type, send);
    }
    public static TemplateData[] GetTemplateDataRecv()
    {
        var recv = UnityDataRecv("GetTemplateData");
        Debug.Log($"GetTemplateDataRecv 接收：{recv}");
        TemplateData[] result;
        try
        {
            result = JsonConvert.DeserializeObject<TemplateData[]>(recv);
        }
        catch
        {
            result = null;
        }
        Debug.Log($"GetTemplateDataRecv 反序列化：{result == null}|{result?.Length}");
        return result;
    }
    #endregion

    #region 3.获得对应模板值
    /// <summary>
    /// 获得指定模板值信息
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
        Debug.Log($"GetTemplateValueSend 发送：{send}");
    }
    public static string[] GetTemplateValueRecv()
    {
        var recv = UnityDataRecv("GetTemplateValue");
        Debug.Log($"GetTemplateValueRecv 接收：{recv}");
        string[] result;
        try
        {
            result = JsonConvert.DeserializeObject<string[]>(recv);
        }
        catch
        {
            Debug.Log("GetTemplateValueRecv 序列化失败");
            result = null;
        }
        Debug.Log($"GetTemplateValueRecv 反序列化：{result == null}|{result.Length}");
        return result;
    }
    #endregion


    #region 4.设置设备参数
    /// <summary>
    /// 设置指定模板信息
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

    #region 5.获取实时事件信息
    /// <summary>
    /// 获取实时事件信息
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static void GetTemplateEventSend(TemplateDataItem item)
    {
        var type = "GetTemplateEvent";
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"GetTemplateValueSend 发送：{send}");
        UnityDataSend(type, send);
    }
    public static TemplateEvent[] GetTemplateEventRecv()
    {
        var recv = UnityDataRecv("GetTemplateEvent");
        Debug.Log($"GetTemplateEventRecv 接收：{recv}");
        TemplateEvent[] result;
        try
        {
            result = JsonConvert.DeserializeObject<TemplateEvent[]>(recv);
        }
        catch
        {
            Debug.Log("GetTemplateEventRecv 序列化失败");
            result = null;
        }
        Debug.Log($"GetTemplateEventRecv 反序列化：{result == null}|{result.Length}");
        return result;
    }
    #endregion

    #region 6.确认事件
    /// <summary>
    /// 确认实时事件
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static void SetTemplateEventSend(SetTemplateEvent item)
    {
        var type = "SetTemplateEvent";
        var send = JsonConvert.SerializeObject(item);
        Debug.Log($"SetTemplateEventSend 发送：{send}");
        UnityDataSend(type, send);
    }
    public static string SetTemplateEventRecv()
    {
        var result = UnityDataRecv("SetTemplateEvent");
        Debug.Log($"SetTemplateEventRecv 结果：{result}");
        return result;
    }
    #endregion

    #region 7.获得登录信息
    /// <summary>
    /// 获得登录者名称
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
        return data?.name ?? "请登录!";
    }
    #endregion


    /// <summary>
    /// 退出登录
    /// </summary>
    public static void SetLogout()
    {
        UnityDataSend("Logout", "");
    }

    /// <summary>
    /// 获得事件声音地址
    /// </summary>
    /// <returns>声音地址</returns>
    public static string GetEventSound()
    {
        var type = "EventSound";
        UnityDataSend(type, "");
        return UnityDataRecv(type);
    }


    /// <summary>
    /// 从JSON文件获得数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath">StreamingAssets文件夹下文件路径,
    /// 例如：/devicesInfo/xxx.json</param>
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