using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class UITips : BaseUIForm
{
    [SerializeField] private Text m_text;

    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        string tips = (string) userData;
        if (!string.IsNullOrEmpty(tips))
        {
            m_text.text = tips;
        }

        GameEntry.TimerManager.AddOneShotTask(2, () =>
        {
            CloseSelf();
        });
    }

    protected internal override void OnClose(object userData)
    {
        base.OnClose(userData);
    }
}
