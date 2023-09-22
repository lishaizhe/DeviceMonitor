using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 定义委托事件类型
/// </summary>
/// <param name="eventData"></param>
public delegate void PointerEventHandler(PointerEventData eventData);

public class UIEventListener : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event PointerEventHandler PointerClick;  //点击
    public event PointerEventHandler PointerEnter;  //移入
    public event PointerEventHandler PointerExit;   //移出

    /// <summary>
    /// 通过变换组件获得事件监听器
    /// </summary>
    /// <param name="tf"></param>
    /// <returns></returns>
    public static UIEventListener GetListener(Transform tf)
    { 
        var uiEvent = tf.GetComponent<UIEventListener>();
        if (uiEvent == null)
        {
            uiEvent = tf.gameObject.AddComponent<UIEventListener>();
        }
        return uiEvent;
    }

    /// <summary>
    /// 点击事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (PointerEnter != null)
        {
            PointerEnter(eventData);
        }
    }

    /// <summary>
    /// 鼠标移入事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PointerClick != null)
        {
            PointerClick(eventData);
        }
    }

    /// <summary>
    /// 鼠标移出事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (PointerExit != null)
        {
            PointerExit(eventData);
        }
    }
}
