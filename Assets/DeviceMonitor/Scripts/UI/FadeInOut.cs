using UnityEngine;
using DG.Tweening;

public class FadeInOut : MonoBehaviour
{
    /*
    [SerializeField] private float m_FadeTime = 0.5f;
    [SerializeField] private WindowsPos m_PopType;
    [SerializeField] public RectTransform m_RectTransform;
    [SerializeField] public CanvasGroup m_CanvasGroup;

    public void PanelFadeIn()
    {
        gameObject.SetActive(true);
        m_CanvasGroup.alpha = 0;

        switch (m_PopType)
        {
            case WindowsPos.Right:             
                m_RectTransform.anchoredPosition = new Vector2(m_RectTransform.rect.width, 0);
                break;
            case WindowsPos.Left:
                m_RectTransform.anchoredPosition = new Vector2(-m_RectTransform.rect.width, 0);
                break;
        }
        m_RectTransform.DOAnchorPos(new Vector2(0, 0), m_FadeTime, false);
        m_CanvasGroup.DOFade(1, m_FadeTime);

    }
    public void PanelFadeOut()
    {   
        switch (m_PopType)
        {
            case WindowsPos.Right:
                m_RectTransform.DOAnchorPos(new Vector2(m_RectTransform.rect.width, 0), m_FadeTime, false);
                break;
            case WindowsPos.Left:
                m_RectTransform.DOAnchorPos(new Vector2(-m_RectTransform.rect.width, 0), m_FadeTime, false);
                break;
        }
        m_CanvasGroup.DOFade(0, m_FadeTime);
    }
    */
}
