using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using TriLibCore;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class DeviceAdjustView : MonoBehaviour
{
    private UIMeasureSystem m_mainView;
    private DeviceData m_dInfo;
    private string m_oldKey = ""; //如果是修改的情况下,当名字换掉了,默认是删除之前的,新建一个

    public GameObject m_objPrefabHotPointItem;
    //名称
    public InputField m_inputName;
    // 描述
    public InputField m_inputDesc;
    //模型路径
    public InputField m_inputModelPath;
    //节点列表根节点
    public Transform m_transHotPointListRoot;
    //宽 高
    public InputField m_inputWidth;

    public InputField m_inputHeight;
    //节点列表
    private List<HotPointEditItem> m_itemList = new List<HotPointEditItem>();
    //错误提示
    public Text m_txtError;
    private string m_tmpFbxPath;

    
    public void Init(UIMeasureSystem mainView)
    {
        m_mainView = mainView;
    }

    private void OnDisable()
    {
        DestroyAllHotItem();
        m_oldKey = "";
    }

    public void ShowData(string modelKey = "")
    {
        this.gameObject.SetActive(true);
        DestroyAllHotItem();
        m_dInfo = new DeviceData();
        var _tmpDeviceInfo = DataInterface.GetInst().GetDInfoByKey(modelKey);
        if (_tmpDeviceInfo != null)
        {
            _tmpDeviceInfo.CopyTo(m_dInfo);
            m_oldKey = m_dInfo.name;
        }
        ShowByDInfo();
    }

    void ShowByDInfo()
    {
        if (m_dInfo == null)
            return;
        m_inputDesc.text = m_dInfo.desc;
        m_inputName.text = m_dInfo.name;
        m_inputModelPath.text = m_dInfo.modelPath;
        m_inputWidth.text = m_dInfo.modelWidth.ToString();
        m_inputHeight.text = m_dInfo.modelHeight.ToString();
        InstantiateHotPointItem();
    }

    //打开目录设置模型路径
    public void OnClickOpenFolder()
    {
        var FilePickerAssetLoader = AssetLoaderFilePicker.Create();
        FilePickerAssetLoader.LoadModelFromFilePickerAsync("IsSelect a File",
            OnLoad,
            null,
            null,
            OnBeginLoad,
            null,
            gameObject,
            null);
    }

    private void OnBeginLoad(bool filesSelected)
    {
        if (filesSelected)
        {
            m_txtError.gameObject.SetActive(true);
            m_txtError.text = "loading...";
        }
        else
        {
            m_txtError.gameObject.SetActive(false);
        }
    }

    // protected void OnProgress(AssetLoaderContext assetLoaderContext, float value)
    // {
    //     m_txtError.gameObject.SetActive(true);
    //     m_txtError.text = value.ToString();
    //     Debug.Log(value);
    // }

    public byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary>
    /// 表示重新导入
    /// </summary>
    /// <param name="assetLoaderContext"></param>
    protected void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        m_txtError.gameObject.SetActive(false);


#if UNITY_EDITOR
        if (assetLoaderContext.RootGameObject == null)
        {
            return;
        }

        GameObject objRoot = assetLoaderContext.RootGameObject;
        objRoot.transform.parent = gameObject.transform;
        objRoot.transform.position = new Vector3(1000, 1000, 1000);
        m_inputModelPath.text = assetLoaderContext.Filename;
        m_dInfo.modelData = StreamToBytes(assetLoaderContext.Stream);
#else
        AssetLoaderContextSub contextSub = assetLoaderContext as AssetLoaderContextSub;

        if (contextSub.RootGameObject == null)
        {
            return;
        }

        GameObject objRoot = contextSub.RootGameObject;
        objRoot.transform.parent = gameObject.transform;
        objRoot.transform.position = new Vector3(1000, 1000, 1000);
        m_inputModelPath.text = contextSub.Filename;
        m_dInfo.modelData = contextSub.Bytes;
#endif

        ParseGameObject(objRoot.transform);
    }

    Transform GetHotPointRoot(Transform obj, string name)
    {
        int childCnt = obj.childCount;
        if (childCnt == 0)
        {
            if (obj.name == name)
                return obj;
        }
        else
        {
            for (int i = 0; i < childCnt; ++i)
            {
                Transform subChild = obj.GetChild(i);
                return GetHotPointRoot(subChild, name);
            }
        }

        return null;
    }

    void ParseGameObject(Transform obj)
    {
        m_dInfo.m_hotpointList.Clear();
        Transform[] allChild = obj.GetComponentsInChildren<Transform>();
        int index = 0;
        for (int i = 0; i < allChild.Length; ++i)
        {
            if (allChild[i].name.Contains("hotpoint"))
            {
                Debug.Log($">>> add hotpoint");
                HotPointInfo _hotPoint = new HotPointInfo();
                _hotPoint.@select = true;
                _hotPoint.name = allChild[i].name;
                _hotPoint.sensorName = string.Format("CW{0:D2}", index);
                _hotPoint.range = 3.0f;
                m_dInfo.m_hotpointList.Add(_hotPoint);
                index++;
            }
        }

        InstantiateHotPointItem();
    }

    public void InstantiateHotPointItem()
    {
        DestroyAllHotItem();
        if (m_dInfo == null)
            return;
        for (int i = 0; i < m_dInfo.m_hotpointList.Count; ++i)
        {
            var obj = Instantiate(m_objPrefabHotPointItem, m_transHotPointListRoot);
            var editItem = obj.GetComponent<HotPointEditItem>();
            editItem.SetData(m_dInfo.m_hotpointList[i]);
            m_itemList.Add(editItem);
        }
    }

    void DestroyAllHotItem()
    {
        foreach (var objItem in m_itemList)
        {
            DestroyImmediate(objItem.gameObject);
        }
        m_itemList.Clear();
    }

    // 确认
    public void OnClickBtnOk()
    {
        //检测传感器设置是否重复
        Dictionary<string, bool> tmpCheck = new Dictionary<string, bool>();
        foreach (var item in m_itemList)
        {
            if (tmpCheck.TryGetValue(item.GetSensor(), out _))
            {
                ShowError("传感器设置重复");
                return;
            }

            tmpCheck[item.GetSensor()] = true;
        }
        //检测是否设置名字
        if (string.IsNullOrEmpty(m_inputName.text))
        {
            ShowError("未设置名字");
            return;
        }

        //检测宽高
        string strWidth = m_inputWidth.text;
        string strHeight = m_inputHeight.text;
        if (string.IsNullOrEmpty(strWidth) ||
            string.IsNullOrEmpty(strHeight) ||
            strWidth.Equals("0") ||
            strHeight.Equals("0"))
        {
            ShowError("请正确设置宽高");
            return;
        }

        m_dInfo.name = m_inputName.text;
        m_dInfo.desc = m_inputDesc.text;
        m_dInfo.modelPath = m_inputModelPath.text;
        m_dInfo.modelWidth = float.Parse(strWidth);
        m_dInfo.modelHeight = float.Parse(strHeight);
        //处理hotpoint点的数据。传感器名称以及范围处理
        foreach (var hotPoint in m_dInfo.m_hotpointList)
        {
            var objHotPoint = m_itemList.Find(x => x.GetName() == hotPoint.name);
            if (objHotPoint)
            {
                hotPoint.sensorName = objHotPoint.GetSensor();
                hotPoint.range = objHotPoint.GetRange();
                hotPoint.@select = objHotPoint.GetIsOn();
            }
        }

        //看前后的名字是否相同,不相同删除之前的
        if (!string.IsNullOrEmpty(m_oldKey) && !m_oldKey.Equals(m_dInfo.name))
        {
            ShowError("不可以修改名字");
            return;
            // DataInterface.GetInst().DeleteDInfoFromDB(m_oldKey);
        }
        DataInterface.GetInst().SaveToData(m_dInfo);
        GameEntry.Event.Fire(EventId.E_EditDeviceDone, m_dInfo);
        this.gameObject.SetActive(false);
    }

    public void ShowError(string errorMsg)
    {
        m_txtError.gameObject.SetActive(true);
        m_txtError.text = errorMsg;
        StartCoroutine(HideErrorMsg());
    }

    IEnumerator HideErrorMsg()
    {
        yield return new WaitForSeconds(2);
        m_txtError.gameObject.SetActive(false);
    }

    //取消
    public void OnClickCancel()
    {
        this.gameObject.SetActive(false);
    }
}
