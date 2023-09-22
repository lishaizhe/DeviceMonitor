using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorMap : MonoBehaviour
{
    [SerializeField] private Slider m_Slider;
    [SerializeField] private TextMeshProUGUI m_Text;

    public void SetValue() 
    {
        m_Text.text = m_Slider.value.ToString("F1");
    }
}
