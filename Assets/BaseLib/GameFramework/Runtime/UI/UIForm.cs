//

using BaseLib.GameFramework.Runtime.UI;
using GameFramework;
using GameFramework.UI;
using UnityEngine;
using Logger = VEngine.Logger;


//#if ODIN_INSPECTOR
//using Sirenix.OdinInspector;
//#endif

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面。
    /// </summary>
    public sealed class UIForm : MonoBehaviour, IUIForm
    {
        [Tooltip("关闭界面后立马卸载")]
        [SerializeField] private bool UnloadImmediately = false;
        
        public int m_SerialId;
        private string m_UIFormAssetName;
        private IUIGroup m_UIGroup;
        private int m_DepthInUIGroup;
        private bool m_PauseCoveredUIForm;
        private UIFormLogic m_UIFormLogic;
		private bool m_Inited;
		private RectTransform rectTransform;

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get
            {
                return m_UIFormAssetName;
            }
        }

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        public object Handle
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public IUIGroup UIGroup
        {
            get
            {
                return m_UIGroup;
            }
        }

        /// <summary>
        /// 获取界面深度。
        /// </summary>
        public int DepthInUIGroup
        {
            get
            {
                return m_DepthInUIGroup;
            }
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get
            {
                return m_PauseCoveredUIForm;
            }
        }

        /// <summary>
        /// 获取界面逻辑。
        /// </summary>
        public UIFormLogic Logic
        {
            get
            {
                return m_UIFormLogic;
            }
        }

		private void Awake()
        {
            m_Inited = false;
        }
        void Start()
        {
         
        }
        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所处的界面组。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int serialId, string uiKey, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, object userData, params object[] backArgs)
        {
            rectTransform = GetComponent<RectTransform>();
			m_SerialId = serialId;
            m_UIFormAssetName = uiFormAssetName;

			if (!m_Inited)
            {
                m_UIGroup = uiGroup;
            }
            else if (m_UIGroup != uiGroup)
            {
                m_UIGroup = uiGroup;
                /*Log.Error("UI group is inconsistent for non-new-instance UI form.");
                return;*/
            }

            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = pauseCoveredUIForm;

            if (m_Inited)
            {
                return;
            }
            m_Inited = true;
            transform.localPosition = Vector3.zero;
            m_UIFormLogic = GetComponent<UIFormLogic>();
            if (m_UIFormLogic == null)
            {
                Logger.E("UI form '{0}' can not get UI form logic.", uiFormAssetName);
                return;
            }
            if ( backArgs != null )
            {
                m_UIFormLogic.BackArga = backArgs;
            }
            m_UIFormLogic.UIKey = uiKey;
            m_UIFormLogic.OnInit(userData);
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        public void OnRecycle()
        {
            m_SerialId = 0;
            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = true;
            //LSZ
            // ResourceUtils.UnloadAssetWithPath<GameObject>(m_UIFormAssetName, UnloadImmediately);
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnOpen(object userData)
        {
            m_UIFormLogic.OnOpen(userData);
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnClose(object userData)
        {
			m_UIFormLogic.OnClose(userData);
            m_UIFormLogic.FullReveal(false);
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        public void OnPause()
        {
            m_UIFormLogic.OnPause();
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        public void OnResume()
        {
            m_UIFormLogic.OnResume();
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        public void OnCover()
        {
            m_UIFormLogic.OnCover();
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        public void OnReveal()
        {
            m_UIFormLogic.OnReveal();
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnRefocus(object userData)
        {
            m_UIFormLogic.OnRefocus(userData);
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            m_UIFormLogic.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_DepthInUIGroup = depthInUIGroup;
            m_UIFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

		public void SetInvisible(bool isInvisible)
		{
			if (rectTransform == null)
			{
				return;
			}

			if (rectTransform.gameObject.activeSelf)
			{
				if (isInvisible)
				{
					rectTransform.localScale = Vector3.zero;
				}
				else
				{
					rectTransform.localScale = Vector3.one;
				}
			}

		}

        public bool OnBack()
        {
            return m_UIFormLogic.OnBack(); 
        }

        public void FullCovered()
        {
            m_UIFormLogic.FullCovered();
        }

        public void FullReveal()
        {
            m_UIFormLogic.FullReveal();
        }

        public string GetUIKey()
        {
            return m_UIFormLogic.UIKey;
        }

        public int GetDepth()
        {
            return m_UIFormLogic.canvas.sortingOrder;
        }
    }
}
