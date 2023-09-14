//Created by zhangliheng on 2020/08/21.
//Copyright © 2019 com.im30.net. All rights reserved.

using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class UITreeWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>UITree Information</b>");
                GUILayout.BeginVertical("box");
                {
                    var groups = GameEntry.UI.GetAllUIGroups();
                    foreach (var gp in groups)
                    {
                        var count = gp.UIFormCount;
                        DrawItem($"Group:[{gp.Name}]      Child Count:{count}", "");
                        foreach (var uiForm in gp.GetAllUIForms())
                        {
                            DrawItem($"------{uiForm.UIFormAssetName}", "");
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
