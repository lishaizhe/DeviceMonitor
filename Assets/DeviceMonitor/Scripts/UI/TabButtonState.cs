using UnityEngine;
using UnityEngine.UI;

public class TabButtonState : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool m_buttonDefaultState; //��ťĬ��״̬

    private Color _selfColor;   //������ɫ
    private Color _downColor;   //������ɫ
    private Image _buttonImage; //��ť����image

    public bool CurrentButtonIndex { get; set; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_buttonImage == null) 
        {
            _buttonImage = GetComponent<Image>();
            _selfColor = ColorManager.ButtonDefault;
            _downColor = ColorManager.ButtonDown;
            _buttonImage.color = _selfColor;
            SetButtonState(m_buttonDefaultState);
        }
    }

    /// <summary>
    /// �л���ť��ǰ״̬
    /// </summary>
    public void ToggleButtonState()
    {
        CurrentButtonIndex = !CurrentButtonIndex;
        SetButtonState(CurrentButtonIndex);
    }

    /// <summary>
    /// ���ð�ť��ǰ״̬
    /// </summary>
    /// <param name="b">��ť״̬</param>
    public void SetButtonState(bool b)
    {
        if (_buttonImage == null)
        {
            Init();
        }
        _buttonImage.color = b ? _downColor : _selfColor;
        CurrentButtonIndex = b;
    }
}
