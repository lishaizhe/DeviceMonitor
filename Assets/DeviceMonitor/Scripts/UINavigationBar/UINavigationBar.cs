using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class UINavigationBar : BaseUIForm
{
    [SerializeField] private GameObject m_objMenuBtn;


    protected internal override void OnOpen(object userData)
    {
        GameEntry.Event.Subscribe(EventId.E_DeActiveNavMenuBtn, DeActiveNavMenuBtn);
    }

    protected internal override void OnClose(object userData)
    {
        GameEntry.Event.Unsubscribe(EventId.E_DeActiveNavMenuBtn, DeActiveNavMenuBtn);
    }

    private void DeActiveNavMenuBtn(object o)
    {
        m_objMenuBtn.SetActive(false);
    }

    /// <summary>
    /// 关闭系统
    /// </summary>
    public void OnClickShutdown()
    {
        Debug.Log($">>>shutdown");
        Application.Quit();
    }
}
