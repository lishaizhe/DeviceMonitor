using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct LoadingParam
{
    public int num1;
    public int num2;
}

public class UILoadingView : BaseUIForm
{
    [SerializeField] private TMP_InputField m_inputUserName;
    [SerializeField] private TMP_InputField m_inputPassword;
    [SerializeField] private Button m_btnLogin;
    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        LoadingParam param = (LoadingParam)userData;
        m_inputUserName.
    }

    protected internal override void OnClose(object userData)
    {
        base.OnClose(userData);
    }

    public void OnClickBtn()
    {
        Debug.Log("****bt");
    }
}
