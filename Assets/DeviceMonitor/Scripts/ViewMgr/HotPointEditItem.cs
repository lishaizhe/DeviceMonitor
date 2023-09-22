using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HotPointEditItem : MonoBehaviour
{
    public Toggle m_toggle;
    public TMP_Text m_name;
    public TMP_Dropdown m_sensor;
    public InputField m_range;
    private List<TMP_Dropdown.OptionData> m_dropDown = new List<TMP_Dropdown.OptionData>(100);
    public void Awake()
    {
        for (int i = 0; i < 100; ++i)
        {
            var optionData = new TMP_Dropdown.OptionData();
            optionData.text = string.Format("CW{0:D2}", i);
            m_dropDown.Add(optionData);
        }

        m_sensor.options = m_dropDown;
    }

    public string GetName()
    {
        return m_name.text;
    }

    public string GetSensor()
    {
        var index = m_sensor.value;
        if (index < 0 || index >= m_dropDown.Count)
            return "error";
        var sen = m_dropDown[index].text;
        return sen;
    }

    public float GetRange()
    {
        var _range = m_range.text;
        _range = String.Format("{0:N1}", _range);
        var _result = float.Parse(_range);
        return _result;
    }

    public bool GetIsOn()
    {
        return m_toggle.isOn;
    }

    public void SetIndex(int index)
    {
        m_sensor.value = index;
    }

    public void SetData(HotPointInfo hotPoint)
    {
        m_toggle.isOn = hotPoint.@select;
        m_name.text = hotPoint.name;
        m_sensor.value = Int32.Parse(hotPoint.sensorName.Substring(2));
        m_range.text = hotPoint.range.ToString();
    }
}


















