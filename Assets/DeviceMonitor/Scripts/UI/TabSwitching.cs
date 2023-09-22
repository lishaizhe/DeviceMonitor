
using UnityEngine;
using UnityEngine.UI;

public class TabSwitching : MonoBehaviour
{
    public Button[] ButtonGroup;
    private Color _downColor = ColorManager.ButtonDown;
    private Color _defaultColor = ColorManager.ButtonDefault;

    public int CurrentButtonIndex { get; set; }

    public void ToggleButtonState(int index)
    {
        if (index >= ButtonGroup.Length)
        {
            return;
        }
        CurrentButtonIndex = index;
        for (int i = 0; i < ButtonGroup.Length; i++)
        {
            var button = ButtonGroup[i];
            var image = button.GetComponent<Image>();
            if (index == i)
            {
                image.color = _downColor;
            }
            else
            {
                image.color = _defaultColor;
            }
        }
    }
}
