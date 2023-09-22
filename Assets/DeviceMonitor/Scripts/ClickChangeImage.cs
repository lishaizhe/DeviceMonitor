using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickChangeImage : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject m_GameObject1;
    [SerializeField] private GameObject m_GameObject2;

    //private bool _isLogin = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_GameObject1 != null) m_GameObject1.SetActive(false);
        if (m_GameObject2 != null) m_GameObject2.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_GameObject1 != null) m_GameObject1.SetActive(true);
        if (m_GameObject2 != null) m_GameObject2.SetActive(false);
    }


}
