using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 弹出窗口父类
/// </summary>
public class WindowSub : WindowBase
{
    [Header("Settings")]
    [SerializeField] protected TextMeshProUGUI m_DeviceName;       //设备名称
    [SerializeField] protected TextMeshProUGUI m_DeviceDescribe;   //设备描述

    [Header("Animation")]
    [SerializeField] protected GameObject m_AnimationWidonw;   //动画
    
    public override UITpye WindowType { get => UITpye.Sub; }

    public string ShowName { get; set; }     //设备名称(显示)
    public string EqName { get; set; }       //设备唯一标识 
    
    //fieldName预置值
    protected readonly string filedNameDesc = "Describle";

    /// <summary>
    /// 打开窗口
    /// </summary>
    public override void OpenWindow()
    {
        m_AnimationWidonw.SetActive(false);
        gameObject.SetActive(true);
        WindowManager.Instance.StartWindowAnimation(
            m_AnimationWidonw.transform as RectTransform,
            true,
            WindowAnimationType.Right2Left,
            0.3f
            );
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public override void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 初始化设备信息
    /// </summary>
    protected override void Init()
    {
        // 设备名称
        m_DeviceName.text = ShowName;
        // 设备描述
        m_DeviceDescribe.text = $"{ShowName}的描述信息";//DataManager.Instance.GetDeviceValue(EqName, filedNameDesc);
    }

    /// <summary>
    /// 提交数据
    /// </summary>
    public virtual void SubmitData()
    {
        
    }

    public void GetConditonFromJson(string fileName, Action<ConditonsDescribe> action)
    {
        string jsonData = "";
        string filepath = Application.streamingAssetsPath + "/Condition/" + fileName + ".json";
#if UNITY_EDITOR
        if (!File.Exists(filepath))
        {
            return;
        }
        using (StreamReader reader = new StreamReader(filepath))
        {
            jsonData = reader.ReadToEnd();
            reader.Close();
            var back = JsonConvert.DeserializeObject<ConditonsDescribe>(jsonData);
            action(back);
        }
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator GetData()
        {
            var uri = new Uri(filepath);
            var w = UnityWebRequest.Get(uri);
            yield return w.SendWebRequest();

            if (w.isNetworkError || w.isHttpError)
            {
                Debug.Log(w.error);
            }
            else
            {
                jsonData = w.downloadHandler.text;
                var back = JsonConvert.DeserializeObject<ConditonsDescribe>(jsonData);
                action(back);
            }
        }
        StartCoroutine(GetData());
#endif  
    }

}
