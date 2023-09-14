using System;
using UnityEngine;

namespace BaseLib.GameFramework.Runtime.UI
{

    [RequireComponent (typeof (RectTransform))]
    public class FixScreenSizeHorizontal : MonoBehaviour
    {
        [SerializeField] private float _developScreenSizeX = 1136;
        [SerializeField] private float _developScreenSizeY = 640;
        
        [Tooltip("由于该脚本会覆盖原有的scale, 所以提供该属性控制原有的scale值")]
        [SerializeField] private float _extraScale         = 1;

        [SerializeField] private float _maxScale = 2;

        private void Start ()
        {
            //美术要求先只处理宽屏
            if ((float) Screen.width / Screen.height > _developScreenSizeX / _developScreenSizeY)
            {
                var rect  = GetComponent<RectTransform> ();
                var scale = Screen.width / _developScreenSizeX / (Screen.height / _developScreenSizeY) * _extraScale;
                if (scale > _maxScale)
                    scale = _maxScale;
                rect.localScale = new Vector2 (scale, scale);
            }
        }
    }
}