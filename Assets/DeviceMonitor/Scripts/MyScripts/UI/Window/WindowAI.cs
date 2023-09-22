using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowAI : WindowSub
{
    [Header("Operation")]
    [SerializeField] protected TMP_InputField m_AlarmHiValue;    //������������
    [SerializeField] protected TMP_InputField m_AlarmLoValue;    //������������
    [SerializeField] protected Toggle m_AlarmHiEnable;  //����/���ø��ޱ���
    [SerializeField] protected Toggle m_AlarmLoEnable;  //����/���õ��ޱ���

    //fieldNameԤ��ֵ
    protected readonly string _fnHiValue = "HiSet";
    protected readonly string _fnLoValue = "LoSet";
    protected readonly string _fnHiEnable = "HiEnable";
    protected readonly string _fnLoEnable = "LoEnable";

    public override void CloseWindow()
    {
        base.CloseWindow();
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
    }

    protected override void Init()
    {
        // �豸����
        m_DeviceName.text = EqName.Substring(EqName.LastIndexOf('.')+1) + " " + ShowName;
        // �豸����
        m_DeviceDescribe.text = DataManager.Instance.GetDeviceValue(EqName, filedNameDesc);

        // ����HiValue
        m_AlarmHiValue.text = DataManager.Instance.GetDeviceValue(EqName, _fnHiValue) ?? m_AlarmHiValue.text;
        // ����LoValue
        m_AlarmLoValue.text = DataManager.Instance.GetDeviceValue(EqName, _fnLoValue) ?? m_AlarmLoValue.text;
        // ����HiEnable
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnHiEnable))
        {
            case "true":
                m_AlarmHiEnable.isOn = true;
                break;
            case "false":
                m_AlarmHiEnable.isOn = false;
                break;
        }
        // ����LoEnable
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnLoEnable))
        {
            case "true":
                m_AlarmLoEnable.isOn = true;
                break;
            case "false":
                m_AlarmLoEnable.isOn = false;
                break;
        }
    }

    protected override void OnAddListener()
    {

    }

    protected override void OnRemoveListener()
    {

    }

    public override void SubmitData()
    {
        var item = new SetTemplateValues();
        item.pagename = "ʵʱ����";

        // Hi
        var hi = new SetTemplateValue();
        var hiInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnHiValue);
        hi.eqid = hiInfo?.eqid;
        hi.fieldid = hiInfo?.fieldid;
        hi.value = m_AlarmHiValue.text;
        item.set.Add(hi);

        // Lo
        var lo = new SetTemplateValue();
        var loInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnLoValue);
        lo.eqid = loInfo?.eqid;
        lo.fieldid = loInfo?.fieldid;
        lo.value = m_AlarmLoValue.text;
        item.set.Add(lo);

        // HiEnable
        var hiEnable = new SetTemplateValue();
        var hiEnableInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnHiEnable);
        hiEnable.eqid = hiEnableInfo?.eqid;
        hiEnable.fieldid = hiEnableInfo?.fieldid;
        hiEnable.value = m_AlarmHiEnable.isOn ? "true" : "false";
        item.set.Add(hiEnable);

        // LoEnable
        var loEnable = new SetTemplateValue();
        var loEnableInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnLoEnable);
        loEnable.eqid = loEnableInfo?.eqid;
        loEnable.fieldid = loEnableInfo?.fieldid;
        loEnable.value = m_AlarmLoEnable.isOn ? "true":"false";
        item.set.Add(loEnable);

        // �ύ����
        var result = DataManager.Instance.SetTemplateValue(item);
        if (result != null)
        {
            if (result == "false")
            {
                Debug.Log("�ύ����ʧ��");
            } 
            CloseWindow();
        }
    }
}
