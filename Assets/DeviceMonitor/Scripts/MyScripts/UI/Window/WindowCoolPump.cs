using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowCoolPump : WindowSub
{
    [Header("Operation")]
    [SerializeField] protected TabSwitching m_Cmd1;                // 预冷命令按钮组件
    [SerializeField] protected TabButtonState m_Cmd1Reset;         // Cmd1复位按钮组件
    [SerializeField] protected TabSwitching m_Cmd2;                // 再生命令按钮组件
    [SerializeField] protected TabButtonState m_Cmd2Reset;         // Cmd2复位按钮组件

    [Header("Conditon")]
    [SerializeField] protected TabSwitching m_conditionButtonGroup;
    [SerializeField] protected GameObject m_conditionDescGroup;
    [SerializeField] protected ConditionsType[] m_conditionType;

    private bool _conditionIsInit = false;
    private List<TextMeshProUGUI> _uiConditionDesc;
    private ConditonsDescribe _dataConditionDesc;

    //fieldName预置值
    protected readonly string _fnCmd1 = "Cmd1";
    protected readonly string _fnCmd1Reset = "Cmd1Reset";
    protected readonly string _fnCmd2 = "Cmd2";
    protected readonly string _fnCmd2Reset = "Cmd2Reset";

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
        // 更新cmd2按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnCmd2))
        {
            case "true":
                m_Cmd2.ToggleButtonState(0);
                break;
            case "false":
                m_Cmd2.ToggleButtonState(1);
                break;
        }
        // 更新Cmd1Reset按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnCmd1Reset))
        {
            case "true":
                m_Cmd1Reset.SetButtonState(true);
                break;
            case "false":
                m_Cmd1Reset.SetButtonState(false);
                break;
        }
        // 更新Cmd2Reset按钮状态
        switch (DataManager.Instance.GetDeviceValue(EqName, _fnCmd2Reset))
        {
            case "true":
                m_Cmd2Reset.SetButtonState(true);
                break;
            case "false":
                m_Cmd2Reset.SetButtonState(false);
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

    /// <summary>
    /// 提交修改数据
    /// </summary>
    public override void SubmitData()
    {
        var item = new SetTemplateValues();
        item.pagename = "实时数据";

        // Cmd1
        var cmd1 = new SetTemplateValue();
        var cmd1Info = DataManager.Instance.GetDeviceInfo(EqName, _fnCmd1);
        cmd1.eqid = cmd1Info?.eqid;
        cmd1.fieldid = cmd1Info?.fieldid;
        switch (m_Cmd1.CurrentButtonIndex)
        {
            case 0:
                cmd1.value = "true";
                break;
            case 1:
                cmd1.value = "false";
                break;
        }
        item.set.Add(cmd1);

        // Cmd2
        var cmd2 = new SetTemplateValue();
        var cmd2Info = DataManager.Instance.GetDeviceInfo(EqName, _fnCmd1);
        cmd2.eqid = cmd2Info?.eqid;
        cmd2.fieldid = cmd2Info?.fieldid;
        switch (m_Cmd1.CurrentButtonIndex)
        {
            case 0:
                cmd2.value = "true";
                break;
            case 1:
                cmd2.value = "false";
                break;
        }
        item.set.Add(cmd2);

        // Cmd1Reset
        var cmd1Reset = new SetTemplateValue();
        var cmd1ResetInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnCmd1Reset);
        cmd1Reset.eqid = cmd1ResetInfo?.eqid;
        cmd1Reset.fieldid = cmd1ResetInfo?.fieldid;
        switch (m_Cmd1Reset.CurrentButtonIndex)
        {
            case false:
                cmd1Reset.value = "false";
                break;
            case true:
                cmd1Reset.value = "true";
                break;
        }
        item.set.Add(cmd1Reset);

        // Cmd2Reset
        var cmd2Reset = new SetTemplateValue();
        var cmd2ResetInfo = DataManager.Instance.GetDeviceInfo(EqName, _fnCmd1Reset);
        cmd2Reset.eqid = cmd2ResetInfo?.eqid;
        cmd2Reset.fieldid = cmd2ResetInfo?.fieldid;
        switch (m_Cmd1Reset.CurrentButtonIndex)
        {
            case false:
                cmd2Reset.value = "false";
                break;
            case true:
                cmd2Reset.value = "true";
                break;
        }
        item.set.Add(cmd2Reset);

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
