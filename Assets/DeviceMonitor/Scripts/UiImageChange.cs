using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiImageChange : Button, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite m_ImageSelf;
    public Sprite m_ImageHover;
    public GameObject m_TipUI = null;
    public GameObject m_OpenUI = null;

    [SerializeField] private bool isclick = false;
    private Image image;

    private void Awake()
    {
        image = transform.GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        image.sprite = m_ImageHover;
        if (m_TipUI != null) m_TipUI.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (isclick == false)
        {
            image.sprite = m_ImageSelf;
            if (m_TipUI != null) m_TipUI.SetActive(false);
        }
        else
        {
            image.sprite = m_ImageHover;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        //切换窗口
        if (m_OpenUI != null) m_OpenUI.SetActive(!m_OpenUI.activeSelf);
    }
}
