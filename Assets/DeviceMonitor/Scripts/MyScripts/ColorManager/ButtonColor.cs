using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonColor : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Operation")]
    [SerializeField] private bool m_IsHoldPressed;

    private Image _image;
    private bool _isDown;

    private Color _selfColor;
    private Color _hoverColor;
    private Color _downColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        //
        _hoverColor = ColorManager.ConfirmButtonHover;
        _downColor = ColorManager.ConfirmButtonDown;
        _selfColor = ColorManager.ConfirmButtonDefault;
        //

    }

    private void OnEnable()
    {
        if (!m_IsHoldPressed || (m_IsHoldPressed && !_isDown))
        {
            _image.color = _selfColor;
        }
    }

    public void SetButtonState(bool state)
    {
        if (m_IsHoldPressed)
        {
            _image.color = state ? _downColor : _selfColor;
            _isDown = state;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _image.color = _selfColor;
        if (m_IsHoldPressed)
        {
            _image.color = _isDown ? _selfColor : _downColor;
            _isDown = !_isDown;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {       
        if (!_isDown)
        {
            _image.color = _hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDown)
        {
            _image.color = _selfColor;
        }
    }
}
