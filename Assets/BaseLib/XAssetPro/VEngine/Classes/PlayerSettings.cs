using System.Collections.Generic;
using UnityEngine;

namespace VEngine
{
    public class PlayerSettings : ScriptableObject
    {
        public List<string> assets = new List<string>();

        public List<string> whiteList = new List<string>();
        
        /// <summary>
        ///     初始化的清单配置，配置包外的清单，底层会自动按需更新下载
        /// </summary>
        [Tooltip("初始化的清单配置，配置包外的清单，底层会自动按需更新下载")]
        public List<string> manifests = new List<string>();
    }
}