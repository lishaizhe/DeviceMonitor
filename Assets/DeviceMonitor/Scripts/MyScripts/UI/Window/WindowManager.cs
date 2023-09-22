using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowManager : MonoSingleton<WindowManager>
{
    [SerializeField] private TextMeshProUGUI m_PageName;//��������������
    
    public Dictionary<string, WindowMain> AllMainWindowDict;
    public Dictionary<string, WindowSub> AllSubWindowDict;
    /// <summary>
    /// ��ʼ��UI
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
        //��ʼ������������
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
    /// ��UI����
    /// </summary>
    /// <param name="name">UI����</param>
    /// <returns>UIBase(�򿪵�UI)</returns>    
    public void OpenWindow(string name)
    {

    }

    /// <summary>
    /// �ر�UI����
    /// </summary>
    /// <param name="uiName">UI����</param>    
    public void CloseWindow(string name)
    {

    }
    
    /// <summary>
    /// ����ָ�����͵�����UI
    /// </summary>
    /// <param name="uiType">UI����</param>
    /// <param name="isDestory">�Ƿ����ٶ���</param>
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
    /// ���ڴ򿪶���
    /// </summary>
    /// <param name="tf">���ڶ���</param>
    /// <param name="isOpen">�򿪻��߹ر�</param>
    /// <param name="animationType">��������</param>
    /// <param name="openTime">�򿪶���ʱ��</param>
    /// <param name="closeTime">�رն���ʱ��</param>
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
