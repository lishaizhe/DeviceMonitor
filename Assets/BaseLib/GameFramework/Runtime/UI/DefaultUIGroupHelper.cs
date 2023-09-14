//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------


// 优化： 
// 基于现在的业务逻辑 做了一些补充优

/* 优化： 
 * 基于现在的业务逻辑 做了一些补充优
 * 在原来 UI 分组的基础上，组内分层
 * 这里的逻辑就是遍历已有层，如果有高于现在占用的最高层的容器的容器，则直接返回，没有就创建新的，加入缓存里
 * 这里有一点小漏洞，如果同时打开两个 UI ，在一个组的不同容器里，如果可以在不关闭较高层的 UI的前提下，关闭层级低的 UI
 * 这时候如果在打开一个新 UI，会在可用的最低层的容器里显示，但是这样就会有空层
 * 大概是这样     空
 *              ---
 *              非空
 *              ---
 *              非空
 * 
 * 其实这就相当于数组第一个元素被移除后，会把元素都向前移动一位，但是目前没有做这个操作，所以会出现空层，但是移动操作会造成UI重绘，所以不如不做
 * 而且数组可以可以移除第一个元素，但是 UI 理论上不能关闭更低层级的弹窗
 * 
 * 但是首先不说能在不关闭较高层 UI 的前提下，关闭下层 UI，就算可以，这样也不会累积很多，现在最大层数是 10，应该足够用
 *
 * 对于 Scene 组有一个特殊特殊处理！！！
 */

using System.Collections.Generic;
using BaseLib.GameFramework.Runtime.UI;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认界面组辅助器。
    /// </summary>
    public class DefaultUIGroupHelper : UIGroupHelperBase
    {
        public const int DepthFactor = 5000;

        private int _depth = 0;
        private Canvas _canvas = null;

        private GameObject _template;

        private void Awake ()
        {
            _canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = DepthFactor * _depth;

            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth (int depth)
        {
            _depth = depth;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = DepthFactor * depth;
        }

        public override void SetSoringLayer (string layer)
        {
            SortingLayer = layer;
        }

        public int SortingOrder { private set; get; }
        
        public string SortingLayer { private set; get; }
    }
}