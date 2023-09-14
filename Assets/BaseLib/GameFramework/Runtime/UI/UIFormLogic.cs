//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using GameKit.Base;
using GameFramework;
using Shelter.Scripts.Tools.BaseLib.GameFramework.Runtime.UI;
using UnityEngine.UI;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面逻辑基类。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(UIForm))] // UI 加载部分只要没添加 UIForm 的界面都是手动 Add 的 UIForm，在这里打一个 Require 标签 只要添加了 UIFormLogic 的界面都会自动添加 UIForm
    public abstract class UIFormLogic : MonoBehaviour
    {
        public const int DepthFactor = 100;

        private int m_OriginalLayer = 0;
        private string uiKey = string.Empty;

        public Canvas canvas { get; private set; }
        public CanvasGroup canvasGroup { get; private set; }
        public RectTransform recrTransform { get; private set; }

        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIForm UIForm
        {
            get
            {
                if(m_uiForm==null)
                {
                    m_uiForm = GetComponent<UIForm>();
                }
                return m_uiForm;
            }
        }
        private UIForm m_uiForm = null;

        private object[] m_backArga = null;


        public object[] BackArga
        {
            get
            {
                return m_backArga;
            }
            set
            {
                m_backArga = value;
            }
        }
        /// <summary>
        /// 获取或设置界面名称。
        /// </summary>
        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        public string UIKey
        {
            get { return this.uiKey; }
            set { this.uiKey = value; }
        }



        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Transform CachedTransform
        {
            get;
            private set;
        }

        /// <summary>
        /// 界面初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnInit(object userData)
        {
            if (CachedTransform == null)
            {
                CachedTransform = transform;
            }

            m_OriginalLayer = gameObject.layer;

            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;

            canvas = gameObject.GetOrAddComponent<Canvas>();
            canvas.sortingOrder = 0;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            recrTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnOpen(object userData)
        {
            if (gameObject == null)
            {
                Log.Error("UIError: UIFormLogic OnOpen gameObject is null !");
                return;
            }

            ResetUIParams();
            
            gameObject.SetActiveEx(true);
            transform.localPosition = Vector3.zero;
        }

        protected internal void ResetUIParams()
        {
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.worldCamera = GameEntry.UI.UICamera;

                recrTransform.localScale = Vector3.one;
                recrTransform.offsetMin = Vector3.zero;
                recrTransform.offsetMax = Vector3.zero;
                recrTransform.anchorMin = Vector2.zero;
                recrTransform.anchorMax = Vector2.one;
                recrTransform.pivot = new Vector2(0.5f, 0.5f);
                recrTransform.sizeDelta = Vector2.zero;
                recrTransform.anchoredPosition = Vector2.zero;
                recrTransform.SetAsLastSibling();

                int uiLayerIndex = LayerMask.NameToLayer("UI");
                if (gameObject.layer != uiLayerIndex)
                    gameObject.layer = uiLayerIndex;
            }
        }

        private void UpdateChildSortingOrder ()
        {
            var forceUpdateOrders = GetComponentsInChildren<IForceUpdateOrder> ();
            if (forceUpdateOrders == null)
                return;

            foreach (var item in forceUpdateOrders)
                item.UpdateSortingOrder(canvas.sortingOrder, canvas.sortingLayerName);
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnClose(object userData)
        {
            if (this == null)
                return;
         
            if (gameObject == null)
            {
                Log.Error("UIError: UIFormLogic OnClose gameObject is null !");
                return;
            }

            if ( BackArga != null && BackArga.Length == 2)
            {
                string uiformStr = BackArga[0].ToString();
                string uiGroup = BackArga[1].ToString();
                if(string.IsNullOrEmpty(uiformStr) || string.IsNullOrEmpty(uiGroup))
                {
                    return;
                }
                if ( GameEntry.UI.IsLoadingUIByKey(uiformStr) || GameEntry.UI.HasUIByKey(uiformStr) )
                {
                    return;
                }
                if(GameEntry.UI.HasUIInConfig(uiformStr))
                {
                    GameEntry.UI.OpenUIByKey(uiformStr, null, uiGroup);
                }
                else
                {
                    LuaManager.Instance.OpenGameUI(BackArga[0].ToString() , uiGroup);
                }

            }

            gameObject.SetLayerRecursively(m_OriginalLayer);
            gameObject.Recycle();
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        protected internal virtual void OnPause()
        {
            if (this == null)
                return;

            if (gameObject == null)
            {
                Log.Error("UIError: UIFormLogic OnPause gameObject is null !");
                return;
            }

            gameObject.SetActiveEx(false);
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        protected internal virtual void OnResume()
        {
            if (gameObject == null)
            {
                Log.Error("UIError: UIFormLogic OnResume gameObject is null !");
                return;
            }

            gameObject.SetActiveEx(true);
        }

        /// <summary>
        /// 界面被遮挡。
        /// </summary>
        protected internal virtual void OnCover()
        {

        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        protected internal virtual void OnReveal()
        {

        }

        /// <summary>
        /// 界面被全屏遮挡
        /// </summary>
        protected internal virtual void OnFullCovered()
        {

        }

        /// <summary>
        /// 界面被全屏遮挡后恢复
        /// </summary>
        protected internal virtual void OnFullReveal()
        {

        }


        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnRefocus(object userData)
        {

        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {

        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int deltaDepth = DefaultUIGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup;

            if(null == this.canvas)
                this.canvas = gameObject.GetOrAddComponent<Canvas>();

            this.canvas.sortingOrder = deltaDepth;

            UpdateChildSortingOrder();
        }

        protected internal virtual bool OnBack()
        {
            Log.Debug(name + " OnBack");
            return false;
        }

        public void OnAfterOpenUI()
        {
            this.CheckAndOpenSenceCameraRender(false);
        }

        public void OnBeforeCloseUI()
        {
            this.CheckAndOpenSenceCameraRender(true);
        }

        /// <summary>
        /// 界面被全屏遮挡
        /// </summary>
        public void FullCovered()
        {
            if (null == canvasGroup)
                return;

            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;

            this.OnFullCovered();
        }

        /// <summary>
        /// 界面被全屏遮挡恢复
        /// </summary>
        /// <param name="notify">是否通知UI触发恢复逻辑</param>
        public void FullReveal(bool notify = true)
        {
            if (null == canvasGroup)
                return;

            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

            if(notify)
                this.OnFullReveal();
        }

        #region 全屏界面关闭场景相机渲染处理逻辑

        public static int g_CloseSenceCameraCount = 0;

        /// <summary>
        /// 还原场景相机状态
        /// </summary>
        public void RevertSenceCameraRender()
        {
            g_CloseSenceCameraCount = 0;
            this.ToggleBackgroundCamera(true);
        }

        /// <summary>
        /// 检查并打开或者关闭场景相机的渲染
        /// </summary>
        /// <param name="isOpen">true:打开 false:关闭</param>
        protected void CheckAndOpenSenceCameraRender(bool isOpen)
        {
            if (string.IsNullOrEmpty(this.uiKey))
                return;

            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(this.uiKey);
            if (null == datarow || (!datarow.IsFullScreen /*&& !datarow.IsCaptureSceneScreenshot*/))
                return;

            if (!isOpen)
            {
                g_CloseSenceCameraCount++;

                this.ToggleBackgroundCamera(false);
            }
            else
            {
                g_CloseSenceCameraCount--;

                if (g_CloseSenceCameraCount < 0)
                    g_CloseSenceCameraCount = 0;

                if (g_CloseSenceCameraCount <= 0)
                    this.ToggleBackgroundCamera(true);
            }
        }

        private void ToggleBackgroundCamera(bool bSet)
        {
            if (SceneContainer.Instance.IsInWorld())
                SceneContainer.Instance.WorldScene.ToggleCamera(bSet);
            else if (SceneContainer.Instance.IsInBattleScene())
                SceneContainer.Instance.BattleScene.ToggleCamera(bSet);
            else if (SceneContainer.Instance.IsInMainCity())
                SceneContainer.Instance.MainScene.ToggleScene(bSet);
            else if(GameEntry.SceneContainer.PveScene != null)
                GameEntry.SceneContainer.PveScene.ToggleScene(bSet);
        }

        #endregion
    }
}
