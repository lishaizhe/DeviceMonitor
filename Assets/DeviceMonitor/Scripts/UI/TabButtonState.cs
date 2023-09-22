using UnityEngine;
using UnityEngine.UI;

public class TabButtonState : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool m_buttonDefaultState; //按钮默认状态

    private Color _selfColor;   //本身颜色
    private Color _downColor;   //按下颜色
    private Image _buttonImage; //按钮背景image

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
    /// 切换按钮当前状态
    /// </summary>
    public void ToggleButtonState()
    {
        CurrentButtonIndex = !CurrentButtonIndex;
        SetButtonState(CurrentButtonIndex);
    }

    /// <summary>
    /// 设置按钮当前状态
    /// </summary>
    /// <param name="b">按钮状态</param>
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
