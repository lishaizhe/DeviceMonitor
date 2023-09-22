using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowDValve : WindowSub
{
    [Header("Operation")]
    [SerializeField] protected TMP_InputField m_opening;    // 调节阀门开度
    [SerializeField] protected TabButtonState m_reset;      // 复位按钮组件

    [Header("Conditon")]
    [SerializeField] protected TabSwitching m_conditionButtonGroup;
    [SerializeField] protected GameObject m_conditionDescGroup;
    [SerializeField] protected ConditionsType[] m_conditionType;

    private bool _conditionIsInit = false;
    private List<TextMeshProUGUI> _uiConditionDesc;
    private ConditonsDescribe _dataConditionDesc;

    //fieldName预置值
    protected readonly string _fnOpening = "OpenSet";
    protected readonly string _fnReset = "Reset";

    protected override void OnEnable()
    {
        Init();
        InitCondition();
    }

    /// <summary>
    /// 设置当前开度
    /// </summary>
    /// <param name="open"></param>
    public void SetOpening(float open)
    {
        if(open>=0 && open <= 100)
            m_opening.text = open.ToString();
    }

 
    /// <summary>
    /// 核对开度输入值
    /// </summary>
    public void OpeningInputCheck()
    {
        if (float.TryParse(m_opening.text, out var open))
        {
            if (open > 100 || open < 0)
            {
                m_opening.text = "0.0";
            }
        }
    }

    protected override void Init()
    {
        base.Init();
        // 更新当前设置开度
        m_opening.text = DataManager.Instance.GetDeviceValue(EqName, _fnOpening);
        
        // 更新reset按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnReset))
        {
            case "true":
                m_reset.SetButtonState(true);
                break;
            case "false":
                m_reset.SetButtonState(false);
                break;
        }
    }

    private void InitCondition() 
    {
        // 更新condition
        GetConditonFromJson(EqName, CallBack);

        void CallBack(ConditonsDescribe con)
        {
            _dataConditionDesc = con;

            if (_dataConditionDesc == null)
            {
                return;
            }
            if (_conditionIsInit) return;
            _uiConditionDesc = new List<TextMeshProUGUI>();
            var array = m_conditionDescGroup.GetComponentsInChildren<TextMeshProUGUI>();
            _uiConditionDesc.AddRange(array);
            foreach (var item in _uiConditionDesc)
            {
                item.text = "";
            }
            
            if (m_conditionType.Length == m_conditionButtonGroup.ButtonGroup.Length)
            {
                for (int i = 0; i < m_conditionButtonGroup.ButtonGroup.Length; i++)
                {
                    m_conditionButtonGroup.ButtonGroup[i].onClick.AddListener(OnConditionButtonClick);
                }
                OnConditionButtonClick();
            }
            else
            {
                Debug.Log("按钮数量和配置的类型不一致！");
            }
        }
    }

    private void OnConditionButtonClick()
    {
        var t = m_conditionType[m_conditionButtonGroup.CurrentButtonIndex];
        switch (t)
        {
            case ConditionsType.ActCon1_0:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.ActCon1_0[j];
                }
                break;
            case ConditionsType.ActCon1_1:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.ActCon1_1[j];
                }
                break;
            case ConditionsType.ActCon2_0:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.ActCon2_0[j];
                }
                break;
            case ConditionsType.ActCon2_1:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.ActCon2_1[j];
                }
                break;
            case ConditionsType.LinkCon1_0:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.LinkCon1_0[j];
                }
                break;
            case ConditionsType.LinkCon1_1:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.LinkCon1_1[j];
                }
                break;
            case ConditionsType.LinkCon2_0:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.LinkCon2_0[j];
                }
                break;
            case ConditionsType.LinkCon2_1:
                for (int j = 0; j < 8; j++)
                {
                    _uiConditionDesc[j].text = _dataConditionDesc.LinkCon2_1[j];
                }
                break;
        }
    }

    public override void SubmitData()
    {
        var item = new SetTemplateValues();
        item.pagename = "实时数据";

        // opening
        var opening = new SetTemplateValue();
        var cmdInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnOpening);
        opening.eqid = cmdInfo?.eqid;
        opening.fieldid = cmdInfo?.fieldid;
        opening.value = m_opening.text;

        item.set.Add(opening);

        // reset
        var reset = new SetTemplateValue();
        var resetInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnReset);
        reset.eqid = resetInfo?.eqid;
        reset.fieldid = resetInfo?.fieldid;
        switch(m_reset.CurrentButtonIndex)
        {
            case false:
                reset.value = "false";
                break;
            case true:
                reset.value = "true";
                break;
        }
        item.set.Add(reset);
        var result = DataManager.Instance.SetTemplateValue(item);
        if (result != null)
        {
            if (result == "false")
            {
                Debug.Log("提交数据失败");
            }
            CloseWindow();
        }
    }
}
