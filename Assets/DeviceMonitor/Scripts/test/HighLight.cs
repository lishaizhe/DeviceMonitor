using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class HighLight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    [SerializeField] Image sourceImage;
    Color sourceColor;

    Color highLightColor;
    
    Image selfImage;
    Color selfColor;

    TextMeshProUGUI sourceText;
    Color soureTextColor;

    TextMeshProUGUI selfText;
    Color selfTextColor;

    //Image backgroundImage;
    //Color backgroundImageColor;


    private void Awake()
    {
        // 
        highLightColor = new Color(52f / 255f, 114f / 255f, 194f / 255f);

        // image
        sourceColor = sourceImage.color;
        selfImage = GetComponent<Image>();
        selfColor = selfImage.color;
        
        // font
        //sourceText = sourceImage.GetComponentInChildren<TextMeshProUGUI>();
        //soureTextColor = sourceText.color;
        
        // 
        //selfText = GetComponentInChildren<TextMeshProUGUI>();
        //selfTextColor = selfText.color;

        // background Image
        //backgroundImage = transform.parent.GetComponentInChildren<Image>();
        //if(backgroundImage is not null)
        //    backgroundImageColor = backgroundImage.color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        sourceImage.color = highLightColor;
        selfImage.color= highLightColor;
        //
        //sourceText.color = Color.black;
        //selfText.color = Color.black;
        if (!string.IsNullOrEmpty(m_sensorKey))
        {
            GameEntry.Event.Fire(EventId.E_TouchEnterSensorTips, m_sensorKey);
        }
        
        //
        //if (backgroundImage is not null)
        //    backgroundImage.color = Color.white;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        sourceImage.color = sourceColor;
        selfImage.color= selfColor;
        //
        //sourceText.color = soureTextColor;
        //selfText.color = selfTextColor;
        if (!string.IsNullOrEmpty(m_sensorKey))
        {
            GameEntry.Event.Fire(EventId.E_TouchExitSensorTips, m_sensorKey);
        }
        //if (backgroundImage is not null)
        //    backgroundImage.color = backgroundImageColor;
    }

    private string m_sensorKey = "";
    public void SetSensorKey(string key)
    {
        m_sensorKey = key;
    }
}
