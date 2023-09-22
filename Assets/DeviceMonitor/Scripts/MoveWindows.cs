using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///  Û±ÍÕœ∂ØUI
/// </summary>
public class MoveWindows : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private Vector2 _localMousePos;
    private Vector3 _planeLocalPos;
    private RectTransform _target;
    private RectTransform _parentRectTransform;
    private RectTransform _targetRectTransform;

    private void Awake()
    {
        _target = this.transform.GetComponent<RectTransform>();
        if (_target == null)
        {
            _target = transform as RectTransform;
        }

        _parentRectTransform = _target.parent as RectTransform;
        _targetRectTransform = _target as RectTransform;
    }
    public void OnPointerDown(PointerEventData data)
    {
        _planeLocalPos = _targetRectTransform.localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, data.position, data.pressEventCamera, out _localMousePos);
        _target.gameObject.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData data)
    {
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - _localMousePos;
            _target.localPosition = _planeLocalPos + offsetToOriginal;
        }
    }
}