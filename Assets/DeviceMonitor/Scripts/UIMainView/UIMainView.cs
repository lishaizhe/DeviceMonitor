using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainView : BaseUIForm
{
    /// <summary>
    /// 关闭系统
    /// </summary>
    public void OnClickShutdown()
    {
        Application.Quit();
    }
}
