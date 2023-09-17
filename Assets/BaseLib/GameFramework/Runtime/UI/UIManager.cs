//

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameFramework.UI
{

	/// <summary>
	/// 界面管理器
	/// </summary>
	internal sealed partial class UIManager : GameFrameworkModule, IUIManager
	{
		private readonly Dictionary<string, UIGroup> m_UIGroups;
		private readonly List<int> m_UIFormsBeingLoaded;
		private readonly List<string> m_UIFormAssetNamesBeingLoaded;
		private readonly HashSet<int> m_UIFormsToReleaseOnLoad;

		private IUIFormHelper m_UIFormHelper;
		private int m_Serial;
		private EventHandler<OpenUIFormSuccessEventArgs> m_OpenUIFormSuccessEventHandler;
		private EventHandler<OpenUIFormFailureEventArgs> m_OpenUIFormFailureEventHandler;
		private EventHandler<CloseUIFormCompleteEventArgs> m_CloseUIFormCompleteEventHandler;

		private readonly Stack<IUIForm> m_UIFormOpenStack;
		private readonly Stack<IUIForm> m_TempStack;
		/// <summary>
		/// 初始化界面管理器的新实例。
		/// </summary>
		public UIManager()
		{
			m_UIFormOpenStack = new Stack<IUIForm>();
			m_TempStack = new Stack<IUIForm>();

			m_UIGroups = new Dictionary<string, UIGroup>();
			m_UIFormsBeingLoaded = new List<int>();
			m_UIFormAssetNamesBeingLoaded = new List<string>();
			m_UIFormsToReleaseOnLoad = new HashSet<int>();

			m_UIFormHelper = null;
			m_Serial = 0;
			m_OpenUIFormSuccessEventHandler = null;
			m_OpenUIFormFailureEventHandler = null;
			m_CloseUIFormCompleteEventHandler = null;

		}

		/// <summary>
		/// 获取界面组数量。
		/// </summary>
		public int UIGroupCount
		{
			get
			{
				return m_UIGroups.Count;
			}
		}

		/// <summary>
		/// 打开界面成功事件。
		/// </summary>
		public event EventHandler<OpenUIFormSuccessEventArgs> OpenUIFormSuccess
		{
			add
			{
				m_OpenUIFormSuccessEventHandler += value;
			}
			remove
			{
				m_OpenUIFormSuccessEventHandler -= value;
			}
		}

		/// <summary>
		/// 打开界面失败事件。
		/// </summary>
		public event EventHandler<OpenUIFormFailureEventArgs> OpenUIFormFailure
		{
			add
			{
				m_OpenUIFormFailureEventHandler += value;
			}
			remove
			{
				m_OpenUIFormFailureEventHandler -= value;
			}
		}

		/// <summary>
		/// 关闭界面完成事件。
		/// </summary>
		public event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete
		{
			add
			{
				m_CloseUIFormCompleteEventHandler += value;
			}
			remove
			{
				m_CloseUIFormCompleteEventHandler -= value;
			}
		}

		/// <summary>
		/// 界面管理器轮询。
		/// </summary>
		/// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
		/// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				uiGroup.Value.Update(elapseSeconds, realElapseSeconds);
			}
		}

		/// <summary>
		/// 关闭并清理界面管理器。
		/// </summary>
		internal override void Shutdown()
		{
			CloseAllLoadedUIForms();
			m_UIGroups.Clear();
			m_UIFormsBeingLoaded.Clear();
			m_UIFormAssetNamesBeingLoaded.Clear();
			m_UIFormsToReleaseOnLoad.Clear();
			m_UIFormOpenStack.Clear();
		}

		/// <summary>
		/// 设置界面辅助器。
		/// </summary>
		/// <param name="uiFormHelper">界面辅助器。</param>
		public void SetUIFormHelper(IUIFormHelper uiFormHelper)
		{
			if (uiFormHelper == null)
			{
				throw new GameFrameworkException("UI form helper is invalid.");
			}

			m_UIFormHelper = uiFormHelper;
		}

		//是否存在界面辅助器
		public bool IsExistFormHelper()
		{
			return m_UIFormHelper != null;
		}

		/// <summary>
		/// 是否存在界面组。
		/// </summary>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <returns>是否存在界面组。</returns>
		public bool HasUIGroup(string uiGroupName)
		{
			if (string.IsNullOrEmpty(uiGroupName))
			{
				throw new GameFrameworkException("UI group name is invalid.");
			}

			return m_UIGroups.ContainsKey(uiGroupName);
		}

		/// <summary>
		/// 获取界面组。
		/// </summary>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <returns>要获取的界面组。</returns>
		public IUIGroup GetUIGroup(string uiGroupName)
		{
			if (string.IsNullOrEmpty(uiGroupName))
			{
				throw new GameFrameworkException("UI group name is invalid.");
			}

			UIGroup uiGroup = null;
			if (m_UIGroups.TryGetValue(uiGroupName, out uiGroup))
			{
				return uiGroup;
			}

			return null;
		}

		/// <summary>
		/// 获取所有界面组。
		/// </summary>
		/// <returns>所有界面组。</returns>
		public IUIGroup[] GetAllUIGroups()
		{
			int index = 0;
			IUIGroup[] uiGroups = new IUIGroup[m_UIGroups.Count];
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				uiGroups[index++] = uiGroup.Value;
			}

			return uiGroups;
		}

		/// <summary>
		/// 增加界面组。
		/// </summary>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <param name="uiGroupHelper">界面组辅助器。</param>
		/// <returns>是否增加界面组成功。</returns>
		public bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper)
		{
			return AddUIGroup(uiGroupName, 0, "Default", uiGroupHelper);
		}

		/// <summary>
		/// 增加界面组。
		/// </summary>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <param name="uiGroupDepth">界面组深度。</param>
		/// <param name="uiGroupHelper">界面组辅助器。</param>
		/// <returns>是否增加界面组成功。</returns>
		public bool AddUIGroup(string uiGroupName, int uiGroupDepth, string sortingLayer, IUIGroupHelper uiGroupHelper)
		{
			if (string.IsNullOrEmpty(uiGroupName))
			{
				throw new GameFrameworkException("UI group name is invalid.");
			}

			if (uiGroupHelper == null)
			{
				throw new GameFrameworkException("UI group helper is invalid.");
			}

			if (HasUIGroup(uiGroupName))
			{
				return false;
			}

			m_UIGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, sortingLayer, uiGroupHelper));

			return true;
		}

		//移除UIGroup
		public void RemoveUIGroup(string uiGroupName)
		{
			if (string.IsNullOrEmpty(uiGroupName))
				return;
			if (m_UIGroups.ContainsKey(uiGroupName))
			{
				m_UIGroups.Remove(uiGroupName);
			}
		}

		/// <summary>
		/// 是否存在界面。
		/// </summary>
		/// <param name="serialId">界面序列编号。</param>
		/// <returns>是否存在界面。</returns>
		public bool HasUIForm(int serialId)
		{
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				if (uiGroup.Value.HasUIForm(serialId))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 是否存在界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <returns>是否存在界面。</returns>
		public bool HasUIForm(string uiFormAssetName)
		{
			if (string.IsNullOrEmpty(uiFormAssetName))
			{
				throw new GameFrameworkException("UI form asset name is invalid.");
			}

			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				if (uiGroup.Value.HasUIForm(uiFormAssetName))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 获取界面。
		/// </summary>
		/// <param name="serialId">界面序列编号。</param>
		/// <returns>要获取的界面。</returns>
		public IUIForm GetUIForm(int serialId)
		{
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				IUIForm uiForm = uiGroup.Value.GetUIForm(serialId);
				if (uiForm != null)
				{
					return uiForm;
				}
			}

			return null;
		}

		/// <summary>
		/// 获取界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <returns>要获取的界面。</returns>
		public IUIForm GetUIForm(string uiFormAssetName)
		{
			if (string.IsNullOrEmpty(uiFormAssetName))
			{
				throw new GameFrameworkException("UI form asset name is invalid.");
			}

			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				IUIForm uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
				if (uiForm != null)
				{
					return uiForm;
				}
			}

			return null;
		}

		/// <summary>
		/// 获取界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <returns>要获取的界面。</returns>
		public IUIForm[] GetUIForms(string uiFormAssetName)
		{
			if (string.IsNullOrEmpty(uiFormAssetName))
			{
				throw new GameFrameworkException("UI form asset name is invalid.");
			}

			List<IUIForm> uiForms = new List<IUIForm>();
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				uiForms.AddRange(uiGroup.Value.GetUIForms(uiFormAssetName));
			}

			return uiForms.ToArray();
		}

		/// <summary>
		/// 获取所有已加载的界面。
		/// </summary>
		/// <returns>所有已加载的界面。</returns>
		public IUIForm[] GetAllLoadedUIForms()
		{
			List<IUIForm> uiForms = new List<IUIForm>();
			foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
			{
				uiForms.AddRange(uiGroup.Value.GetAllUIForms());
			}

			return uiForms.ToArray();
		}


		/// <summary>
		/// 是否正在加载界面。
		/// </summary>
		/// <param name="serialId">界面序列编号。</param>
		/// <returns>是否正在加载界面。</returns>
		public bool IsLoadingUIForm(int serialId)
		{
			return m_UIFormsBeingLoaded.Contains(serialId);
		}

		/// <summary>
		/// 是否正在加载界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <returns>是否正在加载界面。</returns>
		public bool IsLoadingUIForm(string uiFormAssetName)
		{
			if (string.IsNullOrEmpty(uiFormAssetName))
			{
				throw new GameFrameworkException("UI form asset name is invalid.");
			}

			return m_UIFormAssetNamesBeingLoaded.Contains(uiFormAssetName);
		}

        /// <summary>
        /// 是否正在卸载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsToReleaseUIForm(int serialId)
        {
            return m_UIFormsToReleaseOnLoad.Contains(serialId);
        }

		/// <summary>
		/// 打开界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <returns>界面的序列编号。</returns>
		public int OpenUIForm(string uiFormAssetName, string uiGroupName)
		{
			return OpenUIForm(uiFormAssetName, uiGroupName, false, null, null, null);
		}


		/// <summary>
		/// 打开界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
		/// <returns>界面的序列编号。</returns>
		public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
		{
			return OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm, null, null, null);
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
			return OpenUIForm(uiFormAssetName, uiGroupName, false, userData, null, null);
		}


		/// <summary>
		/// 打开界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
		/// <param name="userData">用户自定义数据。</param>
		/// <returns>界面的序列编号。</returns>
		public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
		{
			return OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData, null, null);
		}

		//public int OpenUIForm(string uiKey, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData);
		//{

		//}

		/// <summary>
		/// 打开界面。
		/// </summary>
		/// <param name="uiFormAssetName">界面资源名称。</param>
		/// <param name="uiGroupName">界面组名称。</param>
		/// <param name="priority">加载界面资源的优先级。</param>
		/// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
		/// <param name="userData">用户自定义数据。</param>
		/// <param name="arga">界面返回到指定界面附加参数</param>
		/// <returns>界面的序列编号。</returns>
		public int OpenUIForm( string uiFormAssetName, string uiGroupName,  bool pauseCoveredUIForm, object userData,  Action<IUIForm> OnComplete, params object[] args )
        {

            if ( m_UIFormHelper == null )
            {
                throw new GameFrameworkException("You must set UI form helper first.");
            }

            if ( string.IsNullOrEmpty(uiFormAssetName) )
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if ( string.IsNullOrEmpty(uiGroupName) )
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if ( uiGroup == null )
            {
                throw new GameFrameworkException(string.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = m_Serial++;
            m_UIFormsBeingLoaded.Add(serialId);
            m_UIFormAssetNamesBeingLoaded.Add(uiFormAssetName);
            uiGroup.SetUIFormCount(uiGroup.UIFormCount + 1);
     
            //LSZ
    //         GameKit.Base.ResourceManager.Instance.LoadAssetAsync<GameObject>(uiFormAssetName,  ( key, asset, err ) =>
    //         {
				// string uiKey = Path.GetFileNameWithoutExtension(uiFormAssetName);
    //             var info = new OpenUIFormInfo(serialId, uiKey, uiGroup, pauseCoveredUIForm, userData, OnComplete, args);
    //             if (string.IsNullOrEmpty(err) )
    //             {
    //                 LoadUIFormSuccessCallback(uiFormAssetName, asset, info);
    //             }
    //             else
    //             {
    //                 LoadUIFormFailureCallback(uiFormAssetName,  err,info );
    //             }
    //
    //         });

            return serialId;
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="arga">界面返回到指定界面附加参数</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiKey, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData, Action<IUIForm> OnComplete, params object[] args)
        {

            if (m_UIFormHelper == null)
            {
                throw new GameFrameworkException("You must set UI form helper first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new GameFrameworkException(string.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = m_Serial++;
            m_UIFormsBeingLoaded.Add(serialId);
            m_UIFormAssetNamesBeingLoaded.Add(uiFormAssetName);
            uiGroup.SetUIFormCount(uiGroup.UIFormCount + 1);

            //LSZ
            // GameKit.Base.ResourceManager.Instance.LoadAssetAsync<GameObject>(uiFormAssetName, (key, asset, err) =>
            // {
            //     var info = new OpenUIFormInfo(serialId, uiKey, uiGroup, pauseCoveredUIForm, userData, OnComplete, args);
            //     if (string.IsNullOrEmpty(err))
            //     {
            //         LoadUIFormSuccessCallback(uiFormAssetName, asset, info);
            //     }
            //     else
            //     {
            //         LoadUIFormFailureCallback(uiFormAssetName, err, info);
            //     }
            //
            // });

            return serialId;
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIForm(int serialId)
		{
			CloseUIForm(serialId, null);
		}

		/// <summary>
		/// 关闭界面。
		/// </summary>
		/// <param name="serialId">要关闭界面的序列编号。</param>
		/// <param name="userData">用户自定义数据。</param>
		public void CloseUIForm(int serialId, object userData)
		{
			if (IsLoadingUIForm(serialId))
			{
				m_UIFormsToReleaseOnLoad.Add(serialId);
				return;
			}

			IUIForm uiForm = GetUIForm(serialId);
			if (uiForm == null)
			{
				throw new GameFrameworkException(string.Format("Can not find UI form '{0}'.", serialId.ToString()));
			}

			CloseUIForm(uiForm, userData);
		}

		/// <summary>
		/// 关闭界面。
		/// </summary>
		/// <param name="uiForm">要关闭的界面。</param>
		public void CloseUIForm(IUIForm uiForm)
		{
			CloseUIForm(uiForm, null);
		}

		/// <summary>
		/// 关闭界面。
		/// </summary>
		/// <param name="uiForm">要关闭的界面。</param>
		/// <param name="userData">用户自定义数据。</param>
		public void CloseUIForm(IUIForm uiForm, object userData)
		{
			if (uiForm == null)
			{
				throw new GameFrameworkException("UI form is invalid.");
			}

			UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
			if (uiGroup == null)
			{
				throw new GameFrameworkException("UI group is invalid.");
			}

            m_TempStack.Clear();
            while (m_UIFormOpenStack.Count > 0)
            {
                // Get last uiform
                IUIForm popUiform = m_UIFormOpenStack.Pop();

                if (popUiform == null)
                    continue;

                // 如果关闭的ui不是最后一个
                if (popUiform != uiForm)
                    m_TempStack.Push(popUiform);
                else
                    break;
            }
            while (m_TempStack.Count > 0)
            {
                m_UIFormOpenStack.Push(m_TempStack.Pop());
            }
            
			uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(userData);

            if (m_CloseUIFormCompleteEventHandler != null)
			{
                var args = ReferencePool.Acquire<CloseUIFormCompleteEventArgs>();
                args.SerialId = uiForm.SerialId;
                args.UIFormAssetName = uiForm.UIFormAssetName;
                args.UIGroup = uiGroup;
                args.UserData = userData;
				m_CloseUIFormCompleteEventHandler(this, args);
			}

            // 原本在Update里执行的OnRecycle挪到了这里
            uiForm.OnRecycle();
            uiGroup.Refresh();
        }

        public void CloseUIFormByStack()
        {
            while (m_UIFormOpenStack.Count > 0)
            {
                // Get last uiform
                IUIForm uiForm = m_UIFormOpenStack.Pop();

                if (uiForm == null)
                    continue;

                bool rs = uiForm.OnBack();
                if (!rs)
                    m_UIFormOpenStack.Push(uiForm);

                break;
            }
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
		{
			IUIForm[] uiForms = GetAllLoadedUIForms();
			foreach (IUIForm uiForm in uiForms)
			{
				CloseUIForm(uiForm);
			}
		}


		/// <summary>
		/// 关闭所有正在加载的界面。
		/// </summary>
		public void CloseAllLoadingUIForms()
		{
			foreach (int serialId in m_UIFormsBeingLoaded)
			{
				m_UIFormsToReleaseOnLoad.Add(serialId);
			}
		}

		private void InternalOpenUIForm(string uiFormAssetName,  object uiFormInstance, OpenUIFormInfo uiFormInfo)
		{
			var uiGroup = uiFormInfo.UIGroup;
			var userData = uiFormInfo.UserData;
			var serialId = uiFormInfo.SerialId;
			try
			{
				IUIForm uiForm = m_UIFormHelper.CreateUIForm(uiFormInstance, uiGroup, userData);
				if (uiForm == null)
				{
					throw new GameFrameworkException("Can not create UI form in helper.");
				}

				uiForm.OnInit(serialId, uiFormInfo.UIKey, uiFormAssetName, uiGroup, uiFormInfo.PauseCoveredUIForm, userData, uiFormInfo.BackArgs);
				uiGroup.AddUIForm(uiForm);
				uiForm.OnOpen(userData);
				uiGroup.Refresh();

				m_UIFormOpenStack.Push(uiForm);
                // FIXME: 从作者下面的eventhandler来看，作者本想实现的就是一套纯异步的打开窗口过程
                // 我们强行加一个OnComplete就是把异步变成较为方便的callback方式。
                uiFormInfo.OnLoadFromSuccessAction?.Invoke(uiForm);

                if (m_OpenUIFormSuccessEventHandler != null)
				{
                    var args = ReferencePool.Acquire<OpenUIFormSuccessEventArgs>();
                    args.UIForm = (UIForm)uiForm;
                    args.UserData = userData;
                    
					m_OpenUIFormSuccessEventHandler(this, args);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				if (m_OpenUIFormFailureEventHandler != null)
				{
                    var args = ReferencePool.Acquire<OpenUIFormFailureEventArgs>();
                    args.SerialId = serialId;
                    args.UIFormAssetName = uiFormAssetName;
                    args.UIGroupName = uiGroup.Name;
                    args.ErrorMessage = exception.ToString();
                    args.UserData = userData;

                    m_OpenUIFormFailureEventHandler(this, args);
					return;
				}

				throw;
			}
		}

		private void LoadUIFormSuccessCallback(string uiFormAssetName, object uiFormAsset, object userData)
		{
			OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
			if (openUIFormInfo == null)
			{
				throw new GameFrameworkException("Open UI form info is invalid.");
			}

			m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
			m_UIFormAssetNamesBeingLoaded.Remove(uiFormAssetName);
			if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
			{
				m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
				m_UIFormHelper.ReleaseUIForm(uiFormAsset, null);
				openUIFormInfo.UIGroup.SetUIFormCount(openUIFormInfo.UIGroup.UIFormCount - 1);
				return;
			}

            // 从池里获取一个uiform，目前所有ui都放到池里了；理论上这个uiFormAsset可以Destroy掉？
            var uiformInst = m_UIFormHelper.InstantiateUIForm(uiFormAsset);
            InternalOpenUIForm(uiFormAssetName, uiformInst, openUIFormInfo);
		}

		private void LoadUIFormFailureCallback(string uiFormAssetName,string errorMessage, object userData)
		{
			OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
			if (openUIFormInfo == null)
			{
				throw new GameFrameworkException("Open UI form info is invalid.");
			}

            openUIFormInfo.UIGroup.SetUIFormCount(openUIFormInfo.UIGroup.UIFormCount - 1);
            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
			m_UIFormAssetNamesBeingLoaded.Remove(uiFormAssetName);
			m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);

			string appendErrorMessage = string.Format("Load UI form failure, asset name '{0}' NotExist, error message '{1}'.", uiFormAssetName,  errorMessage);
			if (m_OpenUIFormFailureEventHandler != null)
			{
                var args = ReferencePool.Acquire<OpenUIFormFailureEventArgs>();
                args.SerialId = openUIFormInfo.SerialId;
                args.UIFormAssetName = uiFormAssetName;
                args.UIGroupName = openUIFormInfo.UIGroup.Name;
                args.ErrorMessage = appendErrorMessage;
                args.UserData = userData;

                m_OpenUIFormFailureEventHandler(this, args);
                return;
			}

			throw new GameFrameworkException(appendErrorMessage);
		}
	}

}
