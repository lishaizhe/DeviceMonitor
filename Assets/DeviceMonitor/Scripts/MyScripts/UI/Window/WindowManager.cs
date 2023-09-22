using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowManager : MonoSingleton<WindowManager>
{
    [SerializeField] private TextMeshProUGUI m_PageName;//标题栏导航名称
    
    public Dictionary<string, WindowMain> AllMainWindowDict;
    public Dictionary<string, WindowSub> AllSubWindowDict;
    /// <summary>
    /// 初始化UI
    /// </summary>
    public override void Init()
    {
        //
        AllMainWindowDict = new Dictionary<string, WindowMain>();
        AllSubWindowDict = new Dictionary<string, WindowSub>();
        var array = Resources.FindObjectsOfTypeAll<WindowBase>();
        foreach (var item in array)
        {
            if (item.gameObject.scene.name != null)
            {
                if (item.WindowType == UITpye.Main)
                {
                    AllMainWindowDict.Add(item.gameObject.name, item as WindowMain);
                }
                if (item.WindowType == UITpye.Sub)
                {
                    AllSubWindowDict.Add(item.gameObject.name, item as WindowSub);
                }
            }
        }
        //初始化导航栏名称
        foreach (var item in AllMainWindowDict)
        {
            if (item.Value.IsActive)
            {
                m_PageName.text = $"{item.Value.SystemName} / {item.Value.PageName}";
            }
        }
    }

    public void OnClickOpenMainWindow(string name)
    {
        foreach (var item in AllMainWindowDict)
        {
            if (item.Key == name)
            {
                if (!item.Value.IsActive)
                {
                    item.Value.OpenWindow();
                    m_PageName.text = $"{item.Value.SystemName} / {item.Value.PageName}";
                } 
            }
            else
            {
                if (item.Value.IsActive)
                {
                    item.Value.CloseWindow();
                }
            }
        }
    }

    /// <summary>
    /// 打开UI界面
    /// </summary>
    /// <param name="name">UI名称</param>
    /// <returns>UIBase(打开的UI)</returns>    
    public void OpenWindow(string name)
    {

    }

    /// <summary>
    /// 关闭UI界面
    /// </summary>
    /// <param name="uiName">UI名称</param>    
    public void CloseWindow(string name)
    {

    }
    
    /// <summary>
    /// 隐藏指定类型的所有UI
    /// </summary>
    /// <param name="uiType">UI类型</param>
    /// <param name="isDestory">是否销毁对象</param>
    public void CloseAllWindow()
    {
        foreach (var item in AllMainWindowDict.Values)
        {
                item.CloseWindow();
        }
        foreach (var item in AllSubWindowDict.Values)
        {
            item.CloseWindow();
        }
    }


    public void StartWindowAnimation(RectTransform tf, bool isOpen, WindowAnimationType animationType = WindowAnimationType.None, float openTime = 1, float closeTime = 0.5f)
    {
        StartCoroutine(WindowAnimation(tf, isOpen, animationType, openTime, closeTime));
    }

    /// <summary>
    /// 窗口打开动画
    /// </summary>
    /// <param name="tf">窗口对象</param>
    /// <param name="isOpen">打开或者关闭</param>
    /// <param name="animationType">动画类型</param>
    /// <param name="openTime">打开动画时间</param>
    /// <param name="closeTime">关闭动画时间</param>
    /// <returns></returns>
    private IEnumerator WindowAnimation(RectTransform tf, bool isOpen, WindowAnimationType animationType = WindowAnimationType.None, float openTime = 1, float closeTime = 0.5f)
    {

        switch (animationType)
        {
            case WindowAnimationType.None:
                tf.gameObject.SetActive(isOpen);
                break;
            case WindowAnimationType.Right2Left:
                if (isOpen)
                {
                    tf.anchoredPosition = new Vector2(tf.rect.width, 0);
                    tf.gameObject.SetActive(isOpen);
                    tf.DOAnchorPos(new Vector2(0, 0), openTime, false);
                }
                else
                {
                    tf.DOAnchorPos(new Vector2(-tf.rect.width, 0), closeTime, false);
                    yield return new WaitForSeconds(closeTime);
                    tf.gameObject.SetActive(isOpen);
                }
                break;
            case WindowAnimationType.Left2Right:
                if (isOpen)
                {
                    tf.anchoredPosition = new Vector2(-tf.rect.width, 0);
                    tf.gameObject.SetActive(isOpen);
                    tf.DOAnchorPos(new Vector2(0, 0), openTime, false);
                }
                else
                {
                    tf.DOAnchorPos(new Vector2(tf.rect.width, 0), closeTime, false);
                    yield return new WaitForSeconds(closeTime);
                    tf.gameObject.SetActive(isOpen);
                }
                break;
            case WindowAnimationType.Fade:
                break;
        }
    }
}
