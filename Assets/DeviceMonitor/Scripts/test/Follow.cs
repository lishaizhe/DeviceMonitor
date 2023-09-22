
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityGameFramework.Runtime;

public class Follow : MonoBehaviour
{
    public Transform m_canvasRoot;
    public GameObject m_objPrefab3DUI;
    [SerializeField]
    Camera UI_Camera;//UI���
    [SerializeField]
    Canvas ui_Canvas;
    List<RectTransform> image = new List<RectTransform>();//UIԪ��
    List<GameObject> objlist = new List<GameObject>();//3D����
    
    private void Start()
    {
        GameEntry.Event.Subscribe(EventId.E_BeginToRender, ReInitObj);
        GameEntry.Event.Subscribe(EventId.E_HotPointShow, HotPointShow);
        GameEntry.Event.Subscribe(EventId.E_HotPointHide, HotPointHide);
        GameEntry.Event.Subscribe(EventId.E_ClearAllFollowItem, ClearAll);
    }

    private void OnDestroy()
    {
        GameEntry.Event.Unsubscribe(EventId.E_BeginToRender, ReInitObj);
        GameEntry.Event.Unsubscribe(EventId.E_HotPointShow, HotPointShow);
        GameEntry.Event.Unsubscribe(EventId.E_HotPointHide, HotPointHide);
        GameEntry.Event.Unsubscribe(EventId.E_ClearAllFollowItem, ClearAll);
    }

    public void HotPointShow(object o)
    {
        string sensorName = o as string;
        for (int i = 0; i < image.Count; ++i)
        {
            if (image[i].transform.name == sensorName)
                image[i].gameObject.SetActive(true);
        }
    }
    
    public void HotPointHide(object o)
    {
        string sensorName = o as string;
        for (int i = 0; i < image.Count; ++i)
        {
            if (image[i].transform.name == sensorName)
                image[i].gameObject.SetActive(false);
        }
    }

    public void ClearAll(object o)
    {
        for (int i = 0; i < image.Count; ++i)
        {
            DestroyImmediate(image[i].gameObject);
        }
        image.Clear();
        objlist.Clear();
    }

    //开始捞取需要渲染的节点
    private void ReInitObj(object o)
    {
        ClearAll(null);
        //按照之前的做法是一直更新UI坐标,需要初始化UIObject和hotPointObject两个要匹配
        //因为需要参与渲染的都会挂一个HeatMapFactor脚本,所以直接捞取就行
        HeatMapFactor[] allFactors = GetComponentsInChildren<HeatMapFactor>();
        for (int i = 0; i < allFactors.Length; ++i)
        {
            var obj = Instantiate(m_objPrefab3DUI, m_canvasRoot);
            obj.name = allFactors[i].SensorName;
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            image.Add(rectTransform);
            //同时将3D附属节点进行保存
            objlist.Add(allFactors[i].gameObject);
            var text = obj.transform.Find("Button/Text").GetComponent<TMP_Text>();
            text.text = allFactors[i].SensorName;
            var script = obj.GetComponentInChildren<HighLight>();
            if (script)
            {
                script.SetSensorKey(allFactors[i].SensorName);
            }

            obj.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNamePosition();
    }
    /// <summary>
    /// ����imageλ��
    /// </summary>
    void UpdateNamePosition()
    {
        for (int i = 0; i < image.Count; i++)
        {
            Vector2 mouseDown = UI_Camera.WorldToScreenPoint(objlist[i].transform.position);
            // Debug.Log($">>> X: {mouseDown.x} - Y: {mouseDown.y}");
            Vector2 mouseUGUIPos = new Vector2();
            bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(ui_Canvas.transform as RectTransform, mouseDown, UI_Camera, out mouseUGUIPos);
            if (isRect && image[i])
            {
                image[i].localPosition = mouseUGUIPos;
                // image[i].transform.localPosition = new Vector3(mouseUGUIPos.x, mouseUGUIPos.y, 0);
            }
        }
    }
}
