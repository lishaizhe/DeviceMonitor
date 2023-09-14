/***
 * Created by Darcy
 * Github: https://github.com/Darcy97
 * Date: Tuesday, 14 December 2021
 * Time: 11:41:44
 ***/

using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace BaseLib.GameFramework.Runtime.UI
{
    public static class UIComponentUtils
    {
        public static (int, string) GetBaseSortingOrderAndLayer (this Component ui)
        {
            var  (order, layer) = (0, string.Empty);
            var uiForm = ui.GetComponent<UIForm>();
            if(null == uiForm)
            {
                var uiForms = ui.GetComponentsInParent<UIForm>();
                for(int i = 0; i < uiForms.Length; i++)
                {
                    var isRoot = uiForms[i].transform.parent.GetComponent<DefaultUIGroupHelper>();
                    if (isRoot)
                        uiForm = uiForms[i];
                }
            }

            if(null == uiForm)
            {
                Log.Error("UI√ª”–π“‘ÿUIForm,«ÎºÏ≤È! Name:{0}", ui.name);
                return (0, "Default");
            }

            var canvas = uiForm.GetComponent<Canvas>();
            if (null != canvas)
            {
                order = canvas.sortingOrder;
                layer = canvas.sortingLayerName;
                return (order, layer);
            }

            if (Application.isPlaying)
                Log.Error ($"The object without any father is a UIContainer -- path: {ui.transform.GetPath ()}");
            
            return (order, layer);
        }
    }
}