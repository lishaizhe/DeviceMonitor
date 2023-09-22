using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseEnter : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {

    public GameObject tips;
    public bool _mouseEnter = false;
    public static UIMouseEnter _intencity;
    void Awake()
    {
        _intencity = this;
        tips.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        tips.SetActive(true);
        _mouseEnter = true;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
       
        tips.SetActive(false);
        _mouseEnter = false;
    }
}
