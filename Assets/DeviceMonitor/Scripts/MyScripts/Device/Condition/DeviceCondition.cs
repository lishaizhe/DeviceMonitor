
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class DeviceCondition : MonoBehaviour
{
    [Header("Settings")]
    public TabSwitching m_buttonGroup;
    public GameObject m_conditonGroup;
    public ConditionsType[] m_type;

    private List<TextMeshProUGUI> _describle;
    private bool _isInit = false;

    private void Init()
    {
        if (_isInit) return;
        //
        var cArray = m_conditonGroup.GetComponentsInChildren<TextMeshProUGUI>();
        if (cArray != null)
        {
            _describle = new List<TextMeshProUGUI>();
            _describle.AddRange(cArray);
        }
        //

    }

    /// <summary>
    /// 获得条件信息数组
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <returns>字符串数组</returns>
    private ConditonsDescribe GetDescribeFromJson(string fileName)
    {   
        if(fileName == "") return null;
        if (!fileName.EndsWith(".json")) fileName = fileName + ".json";
        string jsonData;
        string filepath = Application.streamingAssetsPath + "/Condition/" + fileName;
        using (StreamReader reader = new StreamReader(filepath, Encoding.Default))
        {
            jsonData = reader.ReadToEnd();
            reader.Close();
        }
        return JsonConvert.DeserializeObject<ConditonsDescribe>(jsonData);
    }
}
