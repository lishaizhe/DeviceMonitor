using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorItem : MonoBehaviour
{
    public TMP_Text m_name;
    public Toggle m_checkBox;
    public TMP_Text m_temperature;
    private SensorItemData m_itemData;

    Color highLightColor = new Color(52f / 255f, 114f / 255f, 194f / 255f); //new Color(13f / 255f, 143f / 255f, 151f / 255f, 1f);
    Color selfColor;
    Color sourceColor;
    Image selfImage;
    
    [SerializeField] Image sourceImage;
    
    private void Awake()
    {
        m_checkBox.isOn = false;
        sourceColor = sourceImage.color;
        // selfImage = GetComponent<Image>();
        // selfColor = selfImage.color;
    }

    public string GetUuid()
    {
        if (m_itemData != null)
            return m_itemData.name;
        return "";
    }

    public void InitData(SensorItemData itemData)
    {
        SetToggle(false);
        m_itemData = itemData;
        m_name.text = m_itemData.name;
        m_temperature.text = m_itemData.temperature.ToString();
    }

    public void SetToggle(bool value)
    {
        m_checkBox.isOn = value;
    }

    public void UpdateData(SensorItemData itemData)
    {
        SetToggle(true);
        m_temperature.text = string.Format("{0:F1}", itemData.temperature);
    }

    public void SetHighLight(bool highLight)
    {
        if (highLight)
        {
            sourceImage.color = highLightColor;
        }
        else
        {
            sourceImage.color = sourceColor;
        }
    }

}
