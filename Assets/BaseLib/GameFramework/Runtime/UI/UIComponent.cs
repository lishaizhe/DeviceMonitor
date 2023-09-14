//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameFramework.UI;
using UnityEngine;
using System.Runtime.InteropServices;
using GameKit.Base;
using UnityEngine.UI;
using System.Collections;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/UI")]
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        
#if UNITY_IOS        
        [DllImport("__Internal")]
        static extern int isIphoneX();
#endif
        

        private IUIManager m_UIManager = null;
        private EventComponent m_EventComponent = null;

        [SerializeField]
        private bool m_EnableOpenUIFormSuccessEvent = true;

        [SerializeField]
        private bool m_EnableOpenUIFormFailureEvent = true;

        [SerializeField]
        private bool m_EnableCloseUIFormCompleteEvent = true;
        
        [SerializeField] private Camera m_UICamera = null;

        [SerializeField]
        private Transform m_InstanceRoot = null;

        // 截图(场景截图作为背景图)
        [SerializeField] private RawImage m_SceneBackground;

        // 截图(为了解决切换场景时闪一下天空盒的问题,此时会截屏覆盖在主界面上)
        [SerializeField] private RawImage m_ScreenCapture;

        [Header("GroupRoot节点和iphoneX适配有关，不可轻易改动")]
        [SerializeField] public Transform m_GroupRoot = null;
        [SerializeField] private GameObject m_IphoneXCover = null;
        [SerializeField] private bool m_TestIphoneX;



        [SerializeField]
        private string m_UIFormHelperTypeName = "UnityGameFramework.Runtime.DefaultUIFormHelper";

        [SerializeField]
        private UIFormHelperBase m_CustomUIFormHelper = null;

        [SerializeField]
        private string m_UIGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultUIGroupHelper";

        [SerializeField]
        private UIGroupHelperBase m_CustomUIGroupHelper = null;

        [SerializeField]
        private UIGroup[] m_UIGroups = null;

        private CanvasGroup canvasGroup;

// #if UNITY_EDITOR

        public List<string> CacheUINames = new List<string>();

// #endif


        public bool TestIphoneX
        {
            get => m_TestIphoneX;
            set
            {
                m_TestIphoneX = value;
#if UNITY_EDITOR
                //ResizeGroupRoot
                var rt = m_GroupRoot.GetComponent<RectTransform>();
                var offset = GetAdaptOffsetX();
                rt.offsetMin = new Vector2(offset, 0);
                rt.offsetMax = new Vector2(-offset, 0);
                
                // if(m_IphoneXCover) m_IphoneXCover.gameObject.SetActiveEx(TestIphoneX);
                // GameEntry.Event.Fire(this, EventId.OnTestIPhoneXChanged);
#endif
            }
        }

        public Canvas UICanvas
        {
            get
            {
                return uiCanvas;
            }
        }
        private Canvas uiCanvas;

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get
            {
                return m_UIManager.UIGroupCount;
            }
        }

        /// <summary>
        /// UI相机
        /// </summary>
        public Camera UICamera
        {
            get { return m_UICamera; }
        }

        /// <summary>
        /// 根节点
        /// </summary>
        public Transform InstanceRoot
        {
            get { return m_InstanceRoot; }
        }
        
        public RectTransform DefaultUIRoot;
        private TimerTask timerTask;
        private void OnEnable()
        {
            timerTask = TimerManager.Instance.AddFrameExecuteTask(TickUpdate);
        }

        private void OnDisable()
        {
            timerTask?.Cancel();
        }

        private void TickUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }
        }

        //退出清理
        public void Release()
        {
            CloseAllLoadingUIForms();
            CloseAllLoadedUIForms();
        }
        
        private void HandleEscapeKey()
        {
            //从后往前找 如果不是主UI则close掉
            for (var i = m_UIGroups.Length - 1; i > 0; i--)
            {
                var group = m_UIManager.GetUIGroup(m_UIGroups[i].Name);
                if (group.UIFormCount > 0)
                {
                    m_UIManager.CloseUIFormByStack();
                    return;
                }
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_UIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (m_UIManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }

            m_UIManager.OpenUIFormSuccess += OnOpenUIFormSuccess;
            m_UIManager.OpenUIFormFailure += OnOpenUIFormFailure;
            m_UIManager.CloseUIFormComplete += OnCloseUIFormComplete;

            uiCanvas = transform.Find("UIContainer").GetComponent<Canvas>();
            canvasGroup = transform.Find("UIContainer").GetComponent<CanvasGroup>();

        }

        private void OnDestroy()
        {
            if (m_UIManager == null)
                return;
            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                m_UIManager.RemoveUIGroup(m_UIGroups[i].Name);
            }
            m_UIManager.OpenUIFormSuccess -= OnOpenUIFormSuccess;
            m_UIManager.OpenUIFormFailure -= OnOpenUIFormFailure;
            m_UIManager.CloseUIFormComplete -= OnCloseUIFormComplete;
        }


        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            if (!m_UIManager.IsExistFormHelper())
            {
                UIFormHelperBase uiFormHelper = Helper.CreateHelper(m_UIFormHelperTypeName, m_CustomUIFormHelper);
                if (uiFormHelper == null)
                {
                    Log.Error("Can not create UI form helper.");
                    return;
                }

                uiFormHelper.name = string.Format("UI Form Helper");
                Transform transform = uiFormHelper.transform;
                transform.SetParent(this.transform);
                transform.localScale = Vector3.one;

                m_UIManager.SetUIFormHelper(uiFormHelper);
            }


            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = (new GameObject("UI Form Instances")).transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            m_InstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            if (m_GroupRoot == null)
            {
                m_GroupRoot = (new GameObject("GroupRoot")).transform;
                m_GroupRoot.SetParent(m_InstanceRoot.transform);
                m_GroupRoot.localScale = Vector3.one;
            }
            
            m_GroupRoot.gameObject.layer = LayerMask.NameToLayer("UI");
            var rt = m_GroupRoot.gameObject.GetOrAddComponent<RectTransform>();
            var offset = GetAdaptOffsetX();
            rt.offsetMin = new Vector2(offset, 0);
            rt.offsetMax = new Vector2(-offset, 0);
            
            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                var group = m_UIGroups[i];
                if (!AddUIGroup(group.Name, group.Depth, group.SortingLayer))
                {
                    Log.Warning("Add UI group '{0}' failure.", m_UIGroups[i].Name);
                    continue;
                }
            }
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            return m_UIManager.HasUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            return m_UIManager.GetUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            return m_UIManager.GetAllUIGroups();
        }
        

        /// <summary>
        /// 获得某个节点所在的UI Group层
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public string GetUIGruopName(Transform trans)
        {
            string groupName = LFDefines.UIGroup.Default;

            int count = 0;
            int _loop_max_count = 200;
            var parentTransform = trans.parent;
            while (null != parentTransform && count < _loop_max_count)
            {
                var tmpForm = parentTransform.GetComponent<BaseUIForm>();
                if (null != tmpForm)
                {
                    groupName = tmpForm.UIForm.UIGroup.Name;
                    break;
                }
                else
                {
                    parentTransform = parentTransform.parent;
                }

                count++;
            }

#if UNITY_EDITOR
            if (null == parentTransform)
                Log.Error("UI有问题,向上没有查找到继承自BaseUIForm的节点,请检查!");

            if (count >= _loop_max_count)
                Log.Error("向上查找了100个父节点,没有找到继承自BaseUIForm的节点,请检查!");
#endif

            return groupName;
        }

        /// <summary>
        /// 获取SafeArea据左右的边距
        /// </summary>
        /// <returns>左右边距</returns>
        public float GetAdaptOffsetX()
        {
            #if UNITY_EDITOR
                return m_TestIphoneX ? 50 : 0;
            #elif UNITY_ANDROID
                //todo:
                return 0;
            #elif UNITY_IOS
                return isIphoneX() == 1 ? 50 : 0; 
            #else
                return 0;
            #endif
        }


        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <param name="sortingLayer"></param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int depth = 0, string sortingLayer = "Default")
        {
            if (m_UIManager.HasUIGroup(uiGroupName))
            {
                return false;
            }

            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(m_UIGroupHelperTypeName, m_CustomUIGroupHelper, UIGroupCount);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = string.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            if (m_InstanceRoot.GetComponent<Canvas>() != null)
            {
                uiGroupHelper.gameObject.AddComponent<RectTransform>();
            }
            RectTransform rt = uiGroupHelper.GetComponent<RectTransform>();
            float offsetY = 0;
            if (rt != null)
            {
                rt.SetParent(m_GroupRoot);
                rt.localScale = Vector3.one;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector3.zero;
                rt.offsetMax = Vector3.zero;
            }
            else
            {
                var trans = uiGroupHelper.transform;
                trans.SetParent(m_GroupRoot);
                trans.localScale = Vector3.one;
            }

            rt.localPosition = new Vector2(0, offsetY / 2);
            return m_UIManager.AddUIGroup (uiGroupName, depth, sortingLayer, uiGroupHelper);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            return m_UIManager.HasUIForm(serialId);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            return m_UIManager.HasUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        [GenerateWrap]
        public bool HasUIByKey(string uiKey)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if (null == datarow)
                return false;

            return m_UIManager.HasUIForm(datarow.AssetName);
        }

        /// <summary>
        /// 是否存在于UI配置里，
        /// 如果没有再使用lua调用
        /// </summary>
        /// <param name="uiKey"></param>
        /// <returns></returns>
        public bool HasUIInConfig( string uiKey )
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if ( null == datarow )
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        [GenerateWrap]
        public UIForm GetUIForm(int serialId)
        {
            return (UIForm)m_UIManager.GetUIForm(serialId);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        [GenerateWrap]
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return (UIForm)m_UIManager.GetUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIByKey(string uiKey)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if (null == datarow)
                return null;

            return (UIForm)m_UIManager.GetUIForm(datarow.AssetName);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm[] GetUIForms(string uiFormAssetName)
        {
            IUIForm[] uiForms = m_UIManager.GetUIForms(uiFormAssetName);
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public UIForm[] GetAllLoadedUIForms()
        {
            IUIForm[] uiForms = m_UIManager.GetAllLoadedUIForms();
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return m_UIManager.IsLoadingUIForm(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>,
        [GenerateWrap]
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            return m_UIManager.IsLoadingUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 是否正在卸载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsToReleaseUIForm(int serialId)
        {
            return m_UIManager.IsToReleaseUIForm(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIByKey(string uiKey)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if (null == datarow)
                return false;

            return m_UIManager.IsLoadingUIForm(datarow.AssetName);
        }

      

        public int OpenUIFromAndGetObject(string uiFormAssetName, string uiGroupName,Action<IUIForm> onComplete)
        {

            return OpenUIForm(uiFormAssetName, uiGroupName,  false, null, onComplete,null);
        }


        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName,  false, null, null,null);
        }
        /// <summary>
        /// 打开界面并传递可以返回界面数据--要返回必须要求args满足描述条件
        /// </summary>
        /// <param name="uiKey"></param>
        /// <param name="userData"></param>
        /// <param name="uiGroupName"></param>
        /// <param name="onComplete"></param>
        /// <param name="args"></param>(object[]{界面名字，界面层次} 至少结构)
        /// <returns></returns>
         public int OpenUIByKeyWithBack(string uiKey, object userData, string uiGroupName, Action<IUIForm> onComplete,params object[] args)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if ( null == datarow )
            {
                Log.Error("{0}在UI配表中不存在,请检查配置!", uiKey);
                return 0;
            }

            var isLoading = GameEntry.UI.IsLoadingUIForm(datarow.AssetName);
            var hasUI = GameEntry.UI.HasUIForm(datarow.AssetName);
            if (!datarow.IsMultipleInstance && !datarow.IsRefreshOnReopenning
                && (isLoading || hasUI))
                return -1;

            if (!datarow.IsMultipleInstance && datarow.IsRefreshOnReopenning)
            {
                if (hasUI)
                    GameEntry.UI.CloseUIForm(datarow.AssetName);
            }

            //if (datarow.IsCaptureSceneScreenshot)
            //    this.StartSceneScreenCapture();

            string groupName = uiGroupName;
            if (string.IsNullOrEmpty(groupName))
                groupName = datarow.UIGroupName;

            return OpenUIForm(uiKey, datarow.AssetName, groupName,  datarow.IsPauseCoveredUI, userData, onComplete, args);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiKey">对应ui配置表里的ID</param>
        /// <param name="userData">打开界面的参数</param>
        /// <param name="uiGroupName">界面分组名称,如果为null,那么以配置表为准</param>
        /// <returns>界面的序列编号。</returns>
        [GenerateWrap]
        public int OpenUIByKey(string uiKey, object userData = null, string uiGroupName = null, Action<IUIForm> onComplete = null)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if (null == datarow)
            {
                Log.Error("{0}在UI配表中不存在,请检查配置!", uiKey);
                return 0;
            }

            var isLoading = GameEntry.UI.IsLoadingUIForm(datarow.AssetName);
            var hasUI = GameEntry.UI.HasUIForm(datarow.AssetName);
            if (!datarow.IsMultipleInstance && !datarow.IsRefreshOnReopenning
                && (isLoading || hasUI))
                return -1;

            if(!datarow.IsMultipleInstance && datarow.IsRefreshOnReopenning)
            {
                if(hasUI)
                    GameEntry.UI.CloseUIForm(datarow.AssetName);
            }

            //if (datarow.IsCaptureSceneScreenshot)
            //    this.StartSceneScreenCapture();

            string groupName = uiGroupName;
            if (string.IsNullOrEmpty(groupName))
                groupName = datarow.UIGroupName;

            return OpenUIForm(uiKey, datarow.AssetName, groupName, datarow.IsPauseCoveredUI, userData, onComplete, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, false, userData, null,null);
        }

      

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData,  Action<IUIForm> onComplete , params object[] args)
        {
            if (uiGroupName == "Dialog")
            {
                // 避免ui重复打开的提示
                if (HasUIForm(uiFormAssetName) && uiFormAssetName != LF.Constant.UIAssets.Tips)
                {
                    CloseUIForm(GetUIForm(uiFormAssetName));
                }

                // 如果有消息球，关闭消息球
                if (HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }
            if (uiGroupName == "Scene")
            {
                // 如果有消息球，关闭消息球
                if ((uiFormAssetName.Contains("LFViewPickTip") || uiFormAssetName.Contains("LFRoomOperator")) && HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }

            if (uiGroupName == "Default")
            {
                // 如果有消息球，关闭消息球
                if (HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }

            //王晨要求打开任意界面退出挖地模式 (房间的操作台除外)
            if (GameEntry.UI != null && GameEntry.SceneContainer.MainScene != null && GameEntry.SceneContainer.MainScene.SceneState == LFSceneState.Dig)
            {
                if (!uiFormAssetName.Equals(LF.Constant.UIAssets.RoomOperator2)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFDigingTip)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFDigTipsPanel)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFUIDetailsView)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFSelectWorkerPanel)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.Tips)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    GameEntry.Event.Fire(this, EventId.TZ_SET_SCENE_STATE,new LFMainScene.SceneStateParam { state = LFSceneState.Normal });
                }
            }

            //var uiKey = Path.GetFileNameWithoutExtension(uiFormAssetName);
            //if(!string.IsNullOrEmpty(uiKey))
            //{
            //    var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            //    if(null != datarow && datarow.IsCaptureSceneScreenshot)
            //        this.StartSceneScreenCapture();
            //}

#if UNITY_EDITOR
            if (CacheUINames.Contains(uiFormAssetName))
                CacheUINames.Remove(uiFormAssetName);

            if (CacheUINames.Count > 5)
                CacheUINames.RemoveAt(0);

            CacheUINames.Add(uiFormAssetName);
#endif

            return m_UIManager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData,  onComplete,args);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiKey, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData, Action<IUIForm> onComplete, params object[] args)
        {
            if (uiGroupName == "Dialog")
            {
                // 避免ui重复打开的提示
                if (HasUIForm(uiFormAssetName) && uiFormAssetName != LF.Constant.UIAssets.Tips)
                {
                    CloseUIForm(GetUIForm(uiFormAssetName));
                }

                // 如果有消息球，关闭消息球
                if (HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }
            if (uiGroupName == "Scene")
            {
                // 如果有消息球，关闭消息球
                if ((uiFormAssetName.Contains("LFViewPickTip") || uiFormAssetName.Contains("LFRoomOperator")) && HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }

            if (uiGroupName == "Default")
            {
                // 如果有消息球，关闭消息球
                if (HasUIForm(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    CloseUIForm(LF.Constant.UIAssets.LFMsgBallView1);
                }
            }

            //王晨要求打开任意界面退出挖地模式 (房间的操作台除外)
            if (GameEntry.UI != null && GameEntry.SceneContainer.MainScene != null && GameEntry.SceneContainer.MainScene.SceneState == LFSceneState.Dig)
            {
                if (!uiFormAssetName.Equals(LF.Constant.UIAssets.RoomOperator2)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFDigingTip)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFDigTipsPanel)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFUIDetailsView)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFSelectWorkerPanel)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.Tips)
                    && !uiFormAssetName.Equals(LF.Constant.UIAssets.LFMsgBallView1))
                {
                    GameEntry.Event.Fire(this, EventId.TZ_SET_SCENE_STATE, new LFMainScene.SceneStateParam { state = LFSceneState.Normal });
                }
            }

#if UNITY_EDITOR
            if (CacheUINames.Contains(uiFormAssetName))
                CacheUINames.Remove(uiFormAssetName);

            if (CacheUINames.Count > 5)
                CacheUINames.RemoveAt(0);

            CacheUINames.Add(uiFormAssetName);
#endif

            return m_UIManager.OpenUIForm(uiKey, uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData, onComplete, args);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIForm(int serialId)
        {
            m_UIManager.CloseUIForm(serialId);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData)
        {
            m_UIManager.CloseUIForm(serialId, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIByKey(string uiKey)
        {
            var form = this.GetUIByKey(uiKey);
            if (null == form)
                return;

            this.CloseUIForm(form);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIForm(UIForm uiForm)
        {
            if (null == uiForm) return;

            var uiGroup = uiForm.UIGroup;

            m_UIManager.CloseUIForm(uiForm);
            //just for test 先这么测试下
            if (uiGroup != null && uiGroup.Name == "Default")
            {
                if (uiGroup.UIFormCount == 0)
                {
                    if (SceneContainer.Instance != null && SceneContainer.Instance.IsInWorld())
                    {
                        var camera = GameEntry.SceneContainer.WorldScene.Camera;
                        if (camera.CurrentLod <= 4)
                        {
                            GameEntry.Event.Fire(this, EventId.Show_Main, null);
                        }
                    }
                    else
                    {
                        //新任务书中有系列任务详情界面，详情界面关闭时返回到任务书中
                        if (NewBookController.Instance.BookType == BookType.None)
                        {
                            //显示主UI
                            GameEntry.Event.Fire(this, EventId.Show_Main, null);
                        }
                    }

                    uiForm.Logic.RevertSenceCameraRender();
                }
            }

            //var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiForm.Logic.UIKey);
            //if (null != datarow && datarow.IsCaptureSceneScreenshot)
            //    this.ReleaseSceneScreenCapture();
        }

        /// <summary>
        /// 根据 asset name 关闭界面
        /// </summary>
        /// <param name="uiFormAssetName"></param>
        /// <returns></returns>
        public bool CloseUIForm (string uiFormAssetName)
        {
            var form = GetUIForm (uiFormAssetName);
            if (form == null) 
                return false;
            CloseUIForm (form);
            return true;

        }

        /// <summary>
        /// 根据 asset name 关闭界面
        /// </summary>
        /// <param name="uiFormAssetName"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool CloseUIForm (string uiFormAssetName, object userData)
        {
            var form = GetUIForm (uiFormAssetName);
            if (form == null) 
                return false;
            CloseUIForm (form, userData);
            return true;
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(UIForm uiForm, object userData)
        {
            if (null == uiForm) return;

            m_UIManager.CloseUIForm(uiForm, userData);
        }

        public void CloseUIFormByStack()
        {
            m_UIManager.CloseUIFormByStack();
        }

        /// <summary>
        /// 关闭某组所有的界面
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="filter">过滤哪些form Filter.</param>
        public void CloseByGroup(string groupName, List<IUIForm> filter = null, bool isTest = true)
        {
            var group = GetUIGroup(groupName);
            if (group == null)
                return;
            
            var uiForms = group.GetAllUIForms();
            foreach (var form in uiForms)
            {
                if (filter != null && filter.Contains(form))
                {
                    continue;
                }

                CloseUIForm(form.SerialId);
            }

            //just for test 先这么测试下
            if (isTest && groupName.Equals(LFDefines.UIGroup.Default))
            {
                //显示主UI
                GameEntry.Event.Fire(this, EventId.Show_Main, null);
            }
        }

        public void CloseByGroupFilterName(string groupName, List<string> names = null)
        {
            List<IUIForm> filter = new List<IUIForm>();
            for (int i = 0; i < names?.Count; i++)
            {
                var form = GetUIForm(names[i]);
                if (form != null)
                {
                    filter.Add(form);
                }
            }
            CloseByGroup(groupName, filter);
        }

        /// <summary>
        /// 关闭所有打开界面
        /// </summary>
        public void ClosePopUpGroup()
        {
            CloseByGroup(LFDefines.UIGroup.Default);
            CloseByGroup(LFDefines.UIGroup.Dialog);
        }
        public void ClosePopUpGroup(List<string> list)
        {
            List<IUIForm> filter = new List<IUIForm>();
            for (int i = 0; i < list.Count; i++)
            {
                var form = GetUIForm(list[i]);
                if (form != null)
                {
                    filter.Add(form);
                }
            }
            CloseByGroup("Default",filter);
            CloseByGroup("Dialog", filter);
        }
        public void CloseAllGroup ()
        {
            CloseByGroup (LFDefines.UIGroup.Default);
            CloseByGroup (LFDefines.UIGroup.Dialog);
            CloseByGroup (LFDefines.UIGroup.Global);
        }

        // 关闭除UIMain之外的所有UI窗口
        public void CloseSceneGroupExcludeMain()
        {
            var group = GetUIGroup(LFDefines.UIGroup.Scene);
            if (group == null)
                return;
            
            var uiForms = group.GetAllUIForms();
            foreach (var form in uiForms)
            {
                if (form.UIFormAssetName.Equals(GameEntry.UI.GetAssetName("UIMain")))
                    continue;
                CloseUIForm(form.SerialId);
            }
        }
        
        public void CloseAllGroupExceptGlobal()
        {           
            CloseByGroup (LFDefines.UIGroup.Default);
            CloseByGroup (LFDefines.UIGroup.Dialog);
        }

        /// <summary>
        /// 关闭某组所有的界面
        /// </summary>
        public void CloseByGroup(string groupName, object userData)
        {
            IUIGroup[] groups = GetAllUIGroups();
            for (int index = 0; index < groups.Length; index++)
            {
                IUIGroup group = groups[index];
                if (group.Name == groupName)
                {
                    IUIForm[] uIForms = group.GetAllUIForms();
                    for (int i = 0; i < uIForms.Length; i++)
                    {
                        CloseUIForm(uIForms[i].SerialId, userData);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            m_UIManager.CloseAllLoadingUIForms();
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>ed
        public void CloseAllLoadedUIForms()
        {
            m_UIManager.CloseAllLoadedUIForms();
        }

        private void OnOpenUIFormSuccess(object sender, OpenUIFormSuccessEventArgs e)
        {
            if (m_EnableOpenUIFormSuccessEvent)
            {
                m_EventComponent.Fire(this, e);
            }
        }

        private void OnOpenUIFormFailure(object sender, OpenUIFormFailureEventArgs e)
        {
            //Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetName, e.UIGroupName, e.PauseCoveredUIForm.ToString(), e.ErrorMessage);
            Log.ReleaseWarning($"Open UI form failure, asset name : {e.UIFormAssetName}', UI group name :{e.UIGroupName}, error message : {e.ErrorMessage}.");
            if (m_EnableOpenUIFormFailureEvent)
            {
                m_EventComponent.Fire(this, e);
            }
        }

        private void OnCloseUIFormComplete(object sender, CloseUIFormCompleteEventArgs e)
        {
            if (m_EnableCloseUIFormCompleteEvent)
            {
                m_EventComponent.Fire(this, e);
            }
        }
        /// <summary>
        /// 是否屏蔽主界面点击
        /// </summary>
        /// <returns></returns>
        public void IsEnableClick(bool enable)
        {
            canvasGroup.interactable = enable;
        }

        /// <summary>
        /// 获得界面的资源
        /// </summary>
        /// <param name="uiKey"></param>
        /// <returns></returns>
        public string GetAssetName(string uiKey)
        {
            var datarow = GameEntry.Table.GetDataRow<LF.UiDataRow>(uiKey);
            if (null == datarow)
                return null;

            return datarow.AssetName;
        }

        #region 截图(解决闪现天空盒的问题)

        public void StartScreenCapture(Action callBack = null)
        {
            this.StartCoroutine(this.DoScreenCapture(() =>
            {
                callBack?.Invoke();
            }));
        }

        public IEnumerator DoScreenCapture(Action callBack = null)
        {
            //等到帧结束，不然会报错
            yield return YieldUtils.WaitForEndOfFrame ();

            this.m_ScreenCapture.texture = ScreenCapture.CaptureScreenshotAsTexture();
            this.m_ScreenCapture.gameObject.SetActiveEx(true);

            yield return null;

            callBack?.Invoke();
        }

        public void ReleaseScreenCapture()
        {
            this.m_ScreenCapture.gameObject.SetActiveEx(false);
            GameObject.DestroyImmediate(this.m_ScreenCapture.texture);
            this.m_ScreenCapture.texture = null;
        }

        #endregion

        #region 场景截图

        private RenderTexture rtScene = null;
        private Camera currSceneCamera = null;

        public void StartSceneScreenCapture(Action callBack = null)
        {
            // 获得当前场景相机
            currSceneCamera = this.GetCurrentSceneCamera();
            if (null == currSceneCamera)
                return;

            //创建一个RenderTexture对象
            if (null == rtScene)
                rtScene = new RenderTexture(Screen.width, Screen.height, 0);

            //临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
            currSceneCamera.targetTexture = rtScene;
            currSceneCamera.Render();

            //激活这个rt, 并从中中读取像素
            RenderTexture.active = rtScene;
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            //注：这个时候，它是从RenderTexture.active中读取像素
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenShot.Apply();

            this.m_SceneBackground.texture = screenShot;
            this.m_SceneBackground.gameObject.SetActiveEx(true);

            callBack?.Invoke();
        }

        private Camera GetCurrentSceneCamera()
        {
            if (SceneContainer.Instance.IsInWorld())
                return SceneContainer.Instance.WorldScene.GetCamera();
            else if (SceneContainer.Instance.IsInBattleScene())
                return SceneContainer.Instance.BattleScene.GetCamera();
            else if (SceneContainer.Instance.IsInMainCity())
                return SceneContainer.Instance.MainScene.GetCamera();
            else if (GameEntry.SceneContainer.PveScene != null)
                return GameEntry.SceneContainer.PveScene.GetCamera();

            return null;
        }

        public void ReleaseSceneScreenCapture()
        {
            //重置相关参数，以使用camera继续在屏幕上显示
            if(null != this.currSceneCamera)
                this.currSceneCamera.targetTexture = null;

            RenderTexture.active = null;
            GameObject.Destroy(rtScene);
            this.rtScene = null;

            this.m_SceneBackground.gameObject.SetActiveEx(false);
            GameObject.DestroyImmediate(this.m_SceneBackground.texture);
            this.m_SceneBackground.texture = null;
        }

        #endregion

        #region 支持鼠标滚轮和WASD键

        /// <summary>
        /// 是否支持鼠标滚轮和WASD按键，大部分ui需要屏蔽，有些不需要
        /// </summary>
        /// <returns></returns>
        public bool IsSupportForPCBehaviour()
        {
            for (var i = 0; i < m_UIGroups.Length; i++)
            {
                var groupName = m_UIGroups[i].Name;
                var group = m_UIManager.GetUIGroup(groupName);
                if (group != null && group.UIFormCount > 0)
                {
                    if (!UIUtils.NeedSkipCheckForPC(group, groupName))
                        return false;
                }
            }

            return true;
        }


        #endregion
    }
}
