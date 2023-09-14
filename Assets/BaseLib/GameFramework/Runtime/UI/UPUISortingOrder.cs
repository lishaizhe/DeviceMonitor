using GameFramework;
using Shelter.Scripts.Tools.BaseLib.GameFramework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace BaseLib.GameFramework.Runtime.UI
{
    [DisallowMultipleComponent]
    [RequireComponent (typeof (Canvas))]
    [ExecuteInEditMode]
    public class UPUISortingOrder : MonoBehaviour, IForceUpdateOrder
    {
        /// <summary>
        /// 相对于当前UI实际运行时的 Group 的层级提高的层级
        /// </summary>
        [Header ("相对于UI实际显示时所在的 Group 的层级需要提高的层级")] [SerializeField]
        private int upCount;

        /// <summary>
        /// 该UI下面的元素是否需要UI交互
        /// </summary>
        [Header ("是否需要UI交互")] [SerializeField] 
        private bool needGraphicRaycaster;

        private bool haveUpdateSortingOrder = false;

        public void UpdateSortingOrder(int baseSortingOrder, string sortingLayerName)
        {
            var canvas = gameObject.GetOrAddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = baseSortingOrder + upCount;
            canvas.sortingLayerName = sortingLayerName;

            if (needGraphicRaycaster)
            {
                if (!gameObject.GetComponent<GraphicRaycaster>())
                    gameObject.AddComponent<GraphicRaycaster>();
            }

            this.haveUpdateSortingOrder = true;
        }

        private void Start ()
        {
            if (this.haveUpdateSortingOrder)
                return;

            UpdateSortingOrderAndLayer();
        }

        private void UpdateSortingOrderAndLayer()
        {
#if UNITY_EDITOR
            if (upCount > UIFormLogic.DepthFactor)
            {
                Log.Warning("层级提升超标,可能覆盖上层UI! name:{0}", this.gameObject.name);
            }
#endif

            var (order, layer) = this.GetBaseSortingOrderAndLayer ();
            var tSortingOrder = order + upCount;

            //说明层级没变
            if (SortingOrder == tSortingOrder
                && string.CompareOrdinal (_sortingLayer, layer) == 0)
                return;

            var canvas = gameObject.GetOrAddComponent<Canvas> ();
            canvas.overrideSorting = true;

            _sortingLayer           = layer;
            SortingOrder            = tSortingOrder;
            canvas.sortingOrder     = SortingOrder;
            canvas.sortingLayerName = layer;

            if (!needGraphicRaycaster)
                return;
            if (!gameObject.GetComponent<GraphicRaycaster> ())
                gameObject.AddComponent<GraphicRaycaster> ();
        }

#if UNITY_EDITOR

        /// <summary>
        /// 便于编辑模式时调试
        /// </summary>
        private void Update ()
        {
            if (Application.isPlaying)
                return;
            if (SortingOrder == upCount)
                return;
            var canvas = gameObject.GetOrAddComponent<Canvas> ();
            canvas.overrideSorting = true;
            SortingOrder           = upCount;
            canvas.sortingOrder    = SortingOrder;
        }

#endif

        public  int    SortingOrder { private set; get; }
        private string _sortingLayer = string.Empty;
    }
}