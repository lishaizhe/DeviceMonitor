using System.Collections.Generic;
using GameFramework.Event;


public class CommonEventArgs : GameEventArgs
{
    private int m_EventId;
    private object m_UserData;
    private int m_intPara1;
    private string m_strPara1;

    // 提供两个字典，方便传递多样参数；int比较常用，特殊处理一下，其他直接用object处理即可
    private Dictionary<string, object> m_objDict;
    private Dictionary<string, int> m_intDict;

    public CommonEventArgs()
    {

    }

    public CommonEventArgs(EventId eventId, object userData = null)
    {
        m_EventId = (int)eventId;
        m_UserData = userData;
        m_intPara1 = 0;
        m_strPara1 = null;
    }

    // 填充基本数据
    public CommonEventArgs FillBasic(EventId eventId, object userData = null)
    {
        m_EventId = (int)eventId;
        m_UserData = userData;
        m_intPara1 = 0;
        m_strPara1 = null;

        return this;
    }

    // 填充基本数据
    public CommonEventArgs FillInt(EventId eventId, int para)
    {
        m_EventId = (int)eventId;
        m_UserData = null;
        m_intPara1 = para;
        m_strPara1 = null;

        return this;
    }

    // 填充基本数据
    public CommonEventArgs FillString(EventId eventId, string para)
    {
        m_EventId = (int)eventId;
        m_UserData = null;
        m_intPara1 = 0;
        m_strPara1 = para;

        return this;
    }

    public override int Id
    {
        get { return m_EventId; }
    }

    public void setEventId(EventId eventId)
    {
        m_EventId = (int)eventId;
    }

    public object UserData
    {
        get { return m_UserData; }
        set { m_UserData = value; }
    }

    public int IntPara1
    {
        get { return m_intPara1; }
        set { m_intPara1 = value; }
    }

    public string StrPara1
    {
        get { return m_strPara1; }
        set { m_strPara1 = value; }
    }

    // 在进入回收池的时候会被调用，用来清除数据
    public override void Clear()
    {
        m_EventId = 0;
        m_UserData = null;
        m_intPara1 = 0;
        m_strPara1 = null;

        m_objDict?.Clear();
        m_intDict?.Clear();
    }

    // 放入一个string/string 的kv
    public void putString(string key, string str)
    {
        putObject(key, str);
    }

    public string getString(string key)
    {
        string str = getObject(key) as string;
        return str ?? "";
    }

    public void putObject(string key, object value)
    {
        if (m_objDict == null)
        {
            m_objDict = new Dictionary<string, object>();
        }

        m_objDict[key] = value;
    }

    public object getObject(string key)
    {
        object value = null;
        m_objDict?.TryGetValue(key, out value);        

        return value;
    }

    public void putInt(string key, int value)
    {
        if (m_intDict == null)
        {
            m_intDict = new Dictionary<string, int>();
        }

        m_intDict[key] = value;
    }

    public int getInt(string key)
    {
        int value = 0;
        m_intDict?.TryGetValue(key, out value);

        return value;
    }

}