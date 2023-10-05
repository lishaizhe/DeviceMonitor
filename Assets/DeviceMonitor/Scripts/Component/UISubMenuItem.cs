using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class UISubMenuItem : MonoBehaviour
{
    [Tooltip("子菜单类型")]
    [SerializeField] public int subMenuType;
    
    public void OnClickBtn()
    {
        switch ((SubMenuType)subMenuType)
        {
            case SubMenuType.ZhenKong:
            {
                
            }
                break;
            case SubMenuType.Diwen:
            {
                
            }
                break;
            case SubMenuType.Measure:
            {
                if (!GameEntry.UI.HasUIForm(EntityAssets.UIMeasureSystem))
                {
                    GameEntry.Event.Fire(EventId.E_DeActiveNavMenuBtn);
                    GameEntry.UI.OpenUIForm(EntityAssets.UIMeasureSystem, "Default");
                }
            }
                break;
            default:
                break;
        }
    }
}
