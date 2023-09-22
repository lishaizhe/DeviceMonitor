using System.Runtime.InteropServices;
using UnityEngine;

public class WebFunction : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void SetWindowMax(int isFullScreen);

    public void OnClickMaxbutton(bool isFullScreen)
    {
        SetWindowMax(isFullScreen?1:0);
    }
}
