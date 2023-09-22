using System;
using UnityEngine;
using UnityEngine.UI;

public class WebGLTest : MonoBehaviour
{
    public InputField m_Type;
    public InputField m_Item;
    public InputField m_Result;

    public InputField m_Time;
    public Text m_Count;

    private float _updateTime = 1f;
    private float _sumTime;
    private long _recvCount;

    private bool _isUpdating = false;

    public void SetIsUpdate(bool b)
    {
        _isUpdating = b;
    }

    public void SetUpDateTime()
    {
        if (int.TryParse(m_Time.text, out var _time))
        {
            if (_time < 100)
            {
                _updateTime = 0.1f;
            }
            else
            {
                _updateTime = _time / 1000f;
            }
        }
    }

    void Update()
    {
        _sumTime += Time.deltaTime;
        if (_isUpdating && _sumTime >= _updateTime)
        {
            _sumTime = 0;
            OnSendClick();
            _recvCount += 1;
            m_Count.text = _recvCount.ToString();
        }
    }

    public void ClearCount()
    {
        _recvCount = 0;
        m_Count.text = _recvCount.ToString();
        m_Result.text = "";
    }

    public void OnSendClick()
    {
        try
        {
            //Debug.Log(m_Type.text + "----" + m_Item.text);
            //m_Result.text = m_Type.text + m_Item.text;‘
            m_Result.text = "";
            VueData.UnityDataSend(m_Type.text, m_Item.text);
            m_Result.text = VueData.UnityDataRecv(m_Type.text);
        }
        catch (Exception e)
        {
            m_Result.text = e.ToString();
        }
    }

}
