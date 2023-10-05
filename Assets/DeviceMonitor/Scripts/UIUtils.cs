using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class UIUtils
{
    public static void ShowTips(string msg)
    {
        GameEntry.UI.OpenUIDefaultForm(EntityAssets.UITips, msg);
    }
}
