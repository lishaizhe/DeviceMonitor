using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class DeviceItem : MonoBehaviour
{
    public TMP_Text m_name;
    public Toggle m_checkBox;
    private DeviceData m_dInfo;
    public Button m_btn;
    Color highLightColor = new Color(52f / 255f, 114f / 255f, 194f / 255f);
    private Color sourceColor;
    public Image m_btnImage;

    private void Awake()
    {
        sourceColor = m_btnImage.color;
        SetCheckBoxSelect(false);
    }

    public void SetData(DeviceData dInfo)
    {
        m_dInfo = dInfo;
        m_name.text = dInfo.name;
    }

    public string GetDeviceId()
    {
        if (m_dInfo != null)
            return m_dInfo.name;
        return "";
    }

    public void SetCheckBoxSelect(bool select)
    {
        m_checkBox.isOn = select;
    }

    public void SetInteractable( bool data )
    {
        if (data == true) //表示选中 - 高亮
        {
            m_btnImage.color = sourceColor;
        }
        else
        {
            m_btnImage.color = highLightColor;
        }

        m_btn.interactable = data;
    }

    public void OnClickBtn()
    {
        SetInteractable(false);
        //to send
        GameEntry.Event.Fire(EventId.E_SelectDevice, m_dInfo);
    }
}
