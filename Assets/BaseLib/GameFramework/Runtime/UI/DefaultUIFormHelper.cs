//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.UI;
using GameKit.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认界面辅助器。
    /// </summary>
    public class DefaultUIFormHelper : UIFormHelperBase
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        public override object InstantiateUIForm(object uiFormAsset)
        {
            var obj = uiFormAsset as GameObject;
            obj.CreatePool();
            return obj.Spawn();
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public override IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData , params object[] backArga)
        {
            GameObject go = uiFormInstance as GameObject;
            if (go == null)
            {
                Log.Error("UI form instance is invalid.");
                return null;
            }

            Transform transform = go.transform;
            transform.SetParent(((MonoBehaviour)uiGroup.Helper).transform, false);
            transform.localScale = Vector3.one;

            var _allGraphics = go.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < _allGraphics.Length; i++)
            {
                if (_allGraphics[i].raycastTarget == false)
                {
                    var _currCanvas = GameEntry.UI.UICanvas;
                    if(_currCanvas!=null)
                    {
                        GraphicRegistry.UnregisterGraphicForCanvas(_currCanvas, _allGraphics[i]);
                    }
                 
                }
            }

            return go.GetOrAddComponent<UIForm>();
        }

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public override void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            var obj = uiFormInstance as GameObject;
            if (obj != null)
            {
                obj.Recycle();
            }
            ResourceUtils.UnloadAssetWithObject(uiFormAsset, true);
        }
    }
}
