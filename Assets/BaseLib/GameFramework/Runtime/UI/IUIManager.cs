//

using System;
using UnityGameFramework.Runtime;

namespace GameFramework.UI
{
    /// <summary>
    /// 界面管理器接口。
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        int UIGroupCount
        {
            get;
        }

        /// <summary>
        /// 打开界面成功事件。
        /// </summary>
        event EventHandler<OpenUIFormSuccessEventArgs> OpenUIFormSuccess;

        /// <summary>
        /// 打开界面失败事件。
        /// </summary>
        event EventHandler<OpenUIFormFailureEventArgs> OpenUIFormFailure;

        /// <summary>
        /// 关闭界面完成事件。
        /// </summary>
        event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete;

        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiFormHelper">界面辅助器。</param>
        void SetUIFormHelper(IUIFormHelper uiFormHelper);
        
        //是否存在界面辅助器
        bool IsExistFormHelper();
        

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        bool HasUIGroup(string uiGroupName);

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        IUIGroup GetUIGroup(string uiGroupName);

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        IUIGroup[] GetAllUIGroups();

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper);

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        bool AddUIGroup(string uiGroupName, int uiGroupDepth, string sortingLayer, IUIGroupHelper uiGroupHelper);
        
        
        //移除UIGroup
        void RemoveUIGroup(string uiGroupName);

		/// <summary>
		/// 是否存在界面。
		/// </summary>
		/// <param name="serialId">界面序列编号。</param>
		/// <returns>是否存在界面。</returns>
		bool HasUIForm(int serialId);

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        bool HasUIForm(string uiFormAssetName);

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm GetUIForm(int serialId);

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm GetUIForm(string uiFormAssetName);

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm[] GetUIForms(string uiFormAssetName);

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        IUIForm[] GetAllLoadedUIForms();

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        bool IsLoadingUIForm(int serialId);

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        bool IsLoadingUIForm(string uiFormAssetName);

        /// <summary>
        /// 是否正在卸载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        bool IsToReleaseUIForm(int serialId);

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiFormAssetName, string uiGroupName);


        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm);

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData);


	

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData);

    
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="args">要退回指定界面参数</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiFormAssetName, string uiGroupName,  bool pauseCoveredUIForm, object userData,Action<IUIForm> onComplete, params object[] args);

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="args">要退回指定界面参数</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIForm(string uiKey, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData, Action<IUIForm> onComplete, params object[] args);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        void CloseUIForm(int serialId);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void CloseUIForm(int serialId, object userData);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        void CloseUIForm(IUIForm uiForm);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        void CloseUIForm(IUIForm uiForm, object userData);

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        void CloseAllLoadedUIForms();


        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        void CloseAllLoadingUIForms();


        void CloseUIFormByStack();

    }
}
