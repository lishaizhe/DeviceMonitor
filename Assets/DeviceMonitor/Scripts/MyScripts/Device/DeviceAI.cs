using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ģ�����豸
/// </summary>
public class DeviceAI : DeviceBase
{
    [Header("AI Settings")]
    [SerializeField] protected TextMeshProUGUI m_DeviceValue;    //��ǰֵ

    protected override void Awake()
    {
        //ע�����¼�
        if (m_IsClick)
        {
            var button = GetComponent<Button>();
            if (button == null)
            {
                Debug.Log("û���ҵ�button");
                return;
            }
            button.onClick.AddListener(OnClickOpen);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    protected override void OnClickOpen()
    {
        if (m_ClickOpenWindow == null)
        {
            return;
        }
        var window = m_ClickOpenWindow.GetComponent<WindowAI>();
        if (window != null)
        {
            window.EqName = m_EqName;
            window.ShowName = m_DeviceShowName.text;
            window.OpenWindow();
        }
        else
        {
            Debug.Log("OpenWindow = null");
        }
    }

    private bool _state = false;
    public override void SetStateColor(int state)
    {
        //����
        if (256 <= state && state < 512)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceAlarm : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
        //����
        if (512 <= state)
        {
            m_DeviceStateColor.color = _state ? ColorManager.DeviceFault : ColorManager.DeviceDefault;
            _state = !_state;
            return;
        }
    }

    /// <summary>
    /// ���õ�ǰֵ
    /// </summary>
    /// <param name="value"></param>
    public void SetCurrentValue(string value)
    {
        m_DeviceValue.text = value;
    }
}
