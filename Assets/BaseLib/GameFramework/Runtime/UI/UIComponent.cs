//

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
        
        [SerializeField] public Transform m_GroupRoot = null;

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

            // m_EventComponent = GameEntry.GetComponent<EventComponent>();
            // if (m_EventComponent == null)
            // {
            //     Log.Fatal("Event component is invalid.");
            //     return;
            // }

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
            string groupName = "";

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
            return 0;
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
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return (UIForm)m_UIManager.GetUIForm(uiFormAssetName);
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
        public void CloseUIForm(UIForm uiForm)
        {
            if (null == uiForm) return;

            var uiGroup = uiForm.UIGroup;

            m_UIManager.CloseUIForm(uiForm);
        }

        /// <summary>
        /// 关闭所有
        /// </summary>
        public void ClosePopUpGroup()
        {
            
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
            // if (m_EnableOpenUIFormSuccessEvent)
            // {
            //     m_EventComponent.Fire(this, e);
            // }
        }

        private void OnOpenUIFormFailure(object sender, OpenUIFormFailureEventArgs e)
        {
            //Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetName, e.UIGroupName, e.PauseCoveredUIForm.ToString(), e.ErrorMessage);
            // Log.ReleaseWarning($"Open UI form failure, asset name : {e.UIFormAssetName}', UI group name :{e.UIGroupName}, error message : {e.ErrorMessage}.");
            // if (m_EnableOpenUIFormFailureEvent)
            // {
            //     m_EventComponent.Fire(this, e);
            // }
        }

        private void OnCloseUIFormComplete(object sender, CloseUIFormCompleteEventArgs e)
        {
            // if (m_EnableCloseUIFormCompleteEvent)
            // {
            //     m_EventComponent.Fire(this, e);
            // }
        }
        /// <summary>
        /// 是否屏蔽主界面点击
        /// </summary>
        /// <returns></returns>
        public void IsEnableClick(bool enable)
        {
            canvasGroup.interactable = enable;
        }
        
    }
}
