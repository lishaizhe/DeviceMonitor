using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class BaseUIForm : UIFormLogic
{
    public bool SkipDefaultUIAnimation = false;
    
    private bool isPlayBackSound = true;
    
    public void CloseBackSound(){
        isPlayBackSound = false;
    }

    /// <summary>
    /// 查找面板内组件
    /// </summary>
    /// <param name="path">面板内相对路径</param>
    /// <returns>对应组件</returns>
    protected T FindComponent<T>(string path, Transform tran = null) where T : Component
    {
        Transform trans = tran ? tran : transform;
        Transform result = trans.Find(path);
        if (result == null)
        {
            return null;
        }
        if (typeof(Transform) == typeof(T))
            return result as T;
        return result.GetComponent<T>();
    }

    protected internal override void OnInit(object userData)
    {
        base.OnInit(userData);
        CSInit(userData);
    }

    protected internal override void OnOpen(object userData)
    {
		base.OnOpen(userData);
        CSOpen(userData);
        
        this.OnAfterOpenUI();
    }

    protected internal override void OnClose(object userData)
    {
        base.OnClose(userData);
        CSClose(userData);
        this.OnBeforeCloseUI();
    }

    protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        CSUpdate(elapseSeconds, realElapseSeconds);
	}

    protected internal override void OnCover()
    {
        base.OnCover();
        CSCover();
    }

    protected internal override void OnPause()
    {
        base.OnPause();
        CSPause();
    }

    protected internal override void OnResume()
    {
        base.OnResume();
        CSResume();
    }

    protected internal override void OnRefocus(object userData)
    {
        base.OnRefocus(userData);
        CSRefocus(userData);
    }

    protected internal override void OnReveal()
    {
        base.OnReveal();
        CSReveal();
    }

    protected internal override bool OnBack()
    {
        CloseSelf();

        return true;
    }

    protected internal virtual void CSInit(object userData)
    {
    }

    protected internal virtual void CSOpen(object userData)
    {
    }

    protected internal virtual void CSUpdate(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 这个 CSClose 接口是在界面被关闭(SetActive(false)) 后调用的
    /// </summary>
    /// <param name="userData"></param>
    protected internal virtual void CSClose(object userData)
    {
    }

    protected internal virtual void CSCover()
    {
    }

    protected internal virtual void CSResume()
    {
    }

    protected internal virtual void CSPause()
    {
    }

    protected internal virtual void CSRefocus(object userData)
    {
    }

    protected internal virtual void CSReveal()
    {
    }
    
    public void CloseSelf()
    {
        if (this == null)
            return;
        
        GameEntry.UI.CloseUIForm(UIForm);
    }

    public void CloseAllDefaultUI()
    {
        GameEntry.UI.ClosePopUpGroup();
    }
}
