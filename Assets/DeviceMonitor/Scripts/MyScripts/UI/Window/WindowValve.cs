using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class WindowValve : WindowSub
{
    [Header("Operation")]
    [SerializeField] protected TabSwitching m_Cmd1;     // 打开命令按钮
    [SerializeField] protected TabButtonState m_Reset;  // 复位按钮

    [Header("Conditon")]
    [SerializeField] protected TabSwitching m_ConditionButtonGroup;
    [SerializeField] protected GameObject m_ConditionDescGroup;
    [SerializeField] protected ConditionsType[] m_ConditionType;

    private bool _conditionIsInit = false;
    private List<TextMeshProUGUI> _uiConditionDesc;
    private ConditonsDescribe _dataConditionDesc;

    // fieldName预置值
    protected readonly string _fnCmd1 = "Cmd1";
    protected readonly string _fnReset = "Reset";

    protected override void OnEnable()
    {
        Init();
        InitCondition();
    }

    protected override void Init()
    {
        base.Init();
        // 更新cmd1按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnCmd1))
        {
            case "true":
                m_Cmd1.ToggleButtonState(0);
                break;
            case "false":
                m_Cmd1.ToggleButtonState(1);
                break;
        }
        // 更新reset按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnReset))
        {
            case "true":
                m_Reset.SetButtonState(true);
                break;
            case "false":
                m_Reset.SetButtonState(false);
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
            var array = m_ConditionDescGroup.GetComponentsInChildren<TextMeshProUGUI>();
            _uiConditionDesc.AddRange(array);
            foreach (var item in _uiConditionDesc)
            {
                item.text = "";
            }
            
            if (m_ConditionType.Length == m_ConditionButtonGroup.ButtonGroup.Length)
            {
                for (int i = 0; i < m_ConditionButtonGroup.ButtonGroup.Length; i++)
                {
                    m_ConditionButtonGroup.ButtonGroup[i].onClick.AddListener(OnConditionButtonClick);
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
        var t = m_ConditionType[m_ConditionButtonGroup.CurrentButtonIndex];
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

        // Cmd1
        var cmd = new SetTemplateValue();
        var cmdInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnCmd1);
        cmd.eqid = cmdInfo?.eqid;
        cmd.fieldid = cmdInfo?.fieldid;
        switch (m_Cmd1.CurrentButtonIndex)
        {
            case 0:
                cmd.value = "true";
                break;
            case 1:
                cmd.value = "false";
                break;
        }
        item.set.Add(cmd);

        // Reset
        var reset = new SetTemplateValue();
        var resetInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnReset);
        reset.eqid = resetInfo?.eqid;
        reset.fieldid = resetInfo?.fieldid;
        switch(m_Reset.CurrentButtonIndex)
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
