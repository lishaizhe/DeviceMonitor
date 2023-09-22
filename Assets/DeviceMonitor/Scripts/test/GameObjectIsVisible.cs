using System;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameObjectIsVisible : MonoBehaviour
{
    private GameObject m_rootObj;
    private string m_strSensorId = "";
    private float m_dt = 0.0f;
    private bool m_curVisible = true;
    public void SetData(string sensorId, GameObject rootObj)
    {
        m_strSensorId = sensorId;
        m_rootObj = rootObj;
    }

    private void Update()
    {
        if (m_dt > 0.1)
        {
            CheckIsVisible();
            m_dt = 0.0f;
        }

        m_dt += Time.deltaTime;
    }

    void CheckIsVisible()
    {
        if (transform.position.z <= m_rootObj.transform.position.z)
        {
            if (m_curVisible == false)
            {
                m_curVisible = true;
                OnBecameVisible1();
            }
        }
        else
        {
            if (m_curVisible)
            {
                m_curVisible = false;
                OnBecameInvisible1();
            }
        }
    }

    [SerializeField]
    GameObject image;
    private void OnBecameVisible1()
    {
        if (image)
            image.SetActive(true);
        if(!string.IsNullOrEmpty(m_strSensorId))
            GameEntry.Event.Fire(EventId.E_HotPointShow, m_strSensorId);
        Debug.Log($">>>visible: {m_strSensorId}");
    }
    private void OnBecameInvisible1()
    {
        if (image)
            image.SetActive(false);
        if(!string.IsNullOrEmpty(m_strSensorId))
            GameEntry.Event.Fire(EventId.E_HotPointHide, m_strSensorId);
        Debug.Log($">>>hide: {m_strSensorId}");
    }
}
