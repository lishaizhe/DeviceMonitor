using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Localization;
using UnityEngine;
using GameKit.Base;
using UnityGameFramework.Runtime;
public class SettingProxy : Singleton<SettingProxy>
{
    // private string _gameUid = "";
    private string PrivateKey
    {
        get
        {
            return "";
        }
    }
    

    /// <summary>
    /// 加载配置。
    /// </summary>
    /// <returns>是否加载配置成功。</returns>
    public bool Load()
    {
        return true;
    }

    /// <summary>
    /// 保存配置。
    /// </summary>
    /// <returns>是否保存配置成功。</returns>
    public bool Save()
    {
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// 检查是否存在指定配置项。
    /// </summary>
    /// <param name="settingName">要检查配置项的名称。</param>
    /// <returns>指定的配置项是否存在。</returns>
    public bool HasPublicSetting(string settingName)
    {
        return PlayerPrefs.HasKey(settingName);
    }

    public bool HasPrivateSetting (string key)
    {
        return PlayerPrefs.HasKey (key + PrivateKey);
    }

    /// <summary>
    /// 移除指定配置项。
    /// </summary>
    /// <param name="settingName">要移除配置项的名称。</param>
    public void RemovePublicSetting(string settingName)
    {
        PlayerPrefs.DeleteKey(settingName);
    }

    public void RemovePrivateSetting( string settingName )
    {
        PlayerPrefs.DeleteKey(settingName+PrivateKey);
    }

    /// <summary>
    /// 清空所有配置项。
    /// </summary>
    public void RemoveAllSettings()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// 从指定配置项中读取布尔值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns>读取的布尔值。</returns>
    public bool GetPublicBool(string settingName)
    {
        return PlayerPrefs.GetInt(settingName) != 0;
    }

    /// <summary>
    /// 从指定配置项中读取布尔值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultValue">当指定的配置项不存在时，返回此默认值。</param>
    /// <returns>读取的布尔值。</returns>
    public bool GetPublicBool(string settingName, bool defaultValue)
    {
        return PlayerPrefs.GetInt(settingName, defaultValue ? 1 : 0) != 0;
    }

    public bool GetPrivateBool(string settingName)
    {
        return GetPublicBool(settingName+PrivateKey);
    }

    public bool GetPrivateBool(string settingName, bool defaultValue)
    {
        return GetPublicBool(settingName+PrivateKey, defaultValue);
    }

    /// <summary>
    /// 向指定配置项写入布尔值。
    /// </summary>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="value">要写入的布尔值。</param>
    public void SetPublicBool(string settingName, bool value)
    {
        PlayerPrefs.SetInt(settingName, value ? 1 : 0);
    }

    public void SetPrivateBool(string settingName, bool value)
    { 
        if (PrivateKey.IsNullOrEmpty())
        {
            GameFramework.Log.Info("<color=#ff0000> SetPrivateBool error, settingName: {0}", settingName);
        }
        SetPublicBool(settingName+PrivateKey, value);
    }

    /// <summary>
    /// 从指定配置项中读取整数值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns>读取的整数值。</returns>
    public int GetPublicInt(string settingName)
    {
        return PlayerPrefs.GetInt(settingName);
    }

    /// <summary>
    /// 从指定配置项中读取整数值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultValue">当指定的配置项不存在时，返回此默认值。</param>
    /// <returns>读取的整数值。</returns>
    public int GetPublicInt(string settingName, int defaultValue)
    {
        return PlayerPrefs.GetInt(settingName, defaultValue);
    }

    /// <summary>
    /// 向指定配置项写入整数值。
    /// </summary>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="value">要写入的整数值。</param>
    public void SetPublicInt(string settingName, int value)
    {
        PlayerPrefs.SetInt(settingName, value);
    }

    public void SetPrivateInt(string settingName, int value)
    {
        SetPublicInt(settingName+PrivateKey, value);
    }

    public int GetPrivateInt(string settingName, int value)
    {
        return GetPublicInt(settingName+PrivateKey, value);
    }

    public void SetPrivateFloat(string settingName, float value)
    {
        SetPublicFloat(settingName+PrivateKey, value);
    }

    public float GetPrivateFloat(string settingName, float value)
    {
        return GetPublicFloat(settingName+PrivateKey, value);
    }
    
    /// <summary>
    /// 从指定配置项中读取浮点数值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns>读取的浮点数值。</returns>
    public float GetPublicFloat(string settingName)
    {
        return PlayerPrefs.GetFloat(settingName);
    }

    /// <summary>
    /// 从指定配置项中读取浮点数值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultValue">当指定的配置项不存在时，返回此默认值。</param>
    /// <returns>读取的浮点数值。</returns>
    public float GetPublicFloat(string settingName, float defaultValue)
    {
        return PlayerPrefs.GetFloat(settingName, defaultValue);
    }

    /// <summary>
    /// 向指定配置项写入浮点数值。
    /// </summary>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="value">要写入的浮点数值。</param>
    public void SetPublicFloat(string settingName, float value)
    {
        PlayerPrefs.SetFloat(settingName, value);
    }

    /// <summary>
    /// 从指定配置项中读取字符串值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns>读取的字符串值。</returns>
    public string GetPublicString(string settingName)
    {
        return PlayerPrefs.GetString(settingName);
    }

    /// <summary>
    /// 从指定配置项中读取字符串值。
    /// </summary>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultValue">当指定的配置项不存在时，返回此默认值。</param>
    /// <returns>读取的字符串值。</returns>
    public string GetPublicString(string settingName, string defaultValue)
    {
        return PlayerPrefs.GetString(settingName, defaultValue);
    }

    public string GetPrivateString(string settingName, string defaultValue)
    {
        return GetPublicString(settingName+PrivateKey, defaultValue);
    }

    /// <summary>
    /// 向指定配置项写入字符串值。
    /// </summary>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="value">要写入的字符串值。</param>
    public void SetPublicString(string settingName, string value)
    {
        PlayerPrefs.SetString(settingName, value);
    }

    public void SetPrivateString(string settingName, string value)
    {
        SetPublicString(settingName+PrivateKey, value);
    }

    /// <summary>
    /// 从指定配置项中读取对象。
    /// </summary>
    /// <typeparam name="T">要读取对象的类型。</typeparam>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns>读取的对象。</returns>
    public T GetObject<T>(string settingName)
    {
        return GameFramework.Utility.Json.ToObject<T>(PlayerPrefs.GetString(settingName));
    }

    /// <summary>
    /// 从指定配置项中读取对象。
    /// </summary>
    /// <param name="objectType">要读取对象的类型。</param>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <returns></returns>
    public object GetObject(Type objectType, string settingName)
    {
        return GameFramework.Utility.Json.ToObject(objectType, PlayerPrefs.GetString(settingName));
    }

    /// <summary>
    /// 从指定配置项中读取对象。
    /// </summary>
    /// <typeparam name="T">要读取对象的类型。</typeparam>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultObj">当指定的配置项不存在时，返回此默认对象。</param>
    /// <returns>读取的对象。</returns>
    public T GetObject<T>(string settingName, T defaultObj)
    {
        string json = PlayerPrefs.GetString(settingName, null);
        if (json.IsNullOrEmpty())
        {
            return defaultObj;
        }

        return GameFramework.Utility.Json.ToObject<T>(json);
    }

    /// <summary>
    /// 从指定配置项中读取对象。
    /// </summary>
    /// <param name="objectType">要读取对象的类型。</param>
    /// <param name="settingName">要获取配置项的名称。</param>
    /// <param name="defaultObj">当指定的配置项不存在时，返回此默认对象。</param>
    /// <returns></returns>
    public object GetObject(Type objectType, string settingName, object defaultObj)
    {
        string json = PlayerPrefs.GetString(settingName, null);
        if (json == null)
        {
            return defaultObj;
        }

        return GameFramework.Utility.Json.ToObject(objectType, json);
    }

    /// <summary>
    /// 向指定配置项写入对象。
    /// </summary>
    /// <typeparam name="T">要写入对象的类型。</typeparam>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="obj">要写入的对象。</param>
    public void SetObject<T>(string settingName, T obj)
    {
        PlayerPrefs.SetString(settingName, GameFramework.Utility.Json.ToJson(obj));
    }

    /// <summary>
    /// 向指定配置项写入对象。
    /// </summary>
    /// <param name="settingName">要写入配置项的名称。</param>
    /// <param name="obj">要写入的对象。</param>
    public void SetObject(string settingName, object obj)
    {
        PlayerPrefs.SetString(settingName, GameFramework.Utility.Json.ToJson(obj));
    }
    
    public T GetPrivateObject<T>(string settingName)
    {
        var ret = GameFramework.Utility.Json.ToObject<T>(PlayerPrefs.GetString(settingName + PrivateKey));
        if (ret != null)
            return ret;
        return System.Activator.CreateInstance<T>();
    }
    
    public void SetPrivateObject<T>(string settingName, T obj)
    {
        PlayerPrefs.SetString(settingName + PrivateKey, GameFramework.Utility.Json.ToJson(obj));
    }

}
