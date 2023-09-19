using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LoadingParam
{
    public int num1;
    public int num2;
}

public class UILoadingView : BaseUIForm
{
    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        LoadingParam param = (LoadingParam)userData;
        Debug.Log($"****param: {param.num1} - {param.num2}");
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
