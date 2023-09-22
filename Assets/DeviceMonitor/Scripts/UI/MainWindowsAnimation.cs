using UnityEngine;
using DG.Tweening;

public class MainWindowsAnimation : MonoBehaviour
{
    public void OpenGo()
    {
        var rectTs = transform as RectTransform;
        rectTs.DOMoveX(rectTs.rect.width,20);
    }

    public void CloseGo()
    {
        var rectTs = transform as RectTransform;
        rectTs.DOMoveX(-rectTs.rect.width, 20);
    }
}
