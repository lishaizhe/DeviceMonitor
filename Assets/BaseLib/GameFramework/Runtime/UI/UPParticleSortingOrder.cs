using Shelter.Scripts.Tools.BaseLib.GameFramework.Runtime.UI;
using UnityEngine;

namespace BaseLib.GameFramework.Runtime.UI
{
    [RequireComponent (typeof (ParticleSystem))]
    [ExecuteInEditMode]
    public class UPParticleSortingOrder : MonoBehaviour, IForceUpdateOrder
    {

        /// <summary>
        /// 相对于当前UI实际运行时的 Group 的层级提高的层级
        /// </summary>
        [Header ("相对于UI实际显示的 Group 的层级提高的层级")] [SerializeField]
        private int upCount;

        private bool haveUpdateSortingOrder = false;

        public void UpdateSortingOrder(int baseSortingOrder, string sortingLayerName)
        {
            var particle = GetComponent<ParticleSystem>();
            if (null == particle)
                return;

            var pRenderer = particle.GetComponent<Renderer>();
            pRenderer.sortingOrder = baseSortingOrder + upCount;
            pRenderer.sortingLayerName = sortingLayerName;

            this.haveUpdateSortingOrder = true;
        }

        private void Start ()
        {
            if (this.haveUpdateSortingOrder)
                return;

            UpdateSortingOrderAndLayer ();
        }

        private void UpdateSortingOrderAndLayer ()
        {
            var (order, layer) = this.GetBaseSortingOrderAndLayer ();

            var tOrder = order + upCount;

            if (SortingOrder == tOrder
                && string.CompareOrdinal (_sortingLayer, layer) == 0)
                return;

            var particle = GetComponent<ParticleSystem> ();

            if (particle == null)
                return;

            var pRenderer = particle.GetComponent<Renderer> ();
            SortingOrder               = tOrder;
            _sortingLayer              = layer;
            pRenderer.sortingOrder     = SortingOrder;
            pRenderer.sortingLayerName = layer;
        }

        public  int    SortingOrder { private set; get; }
        private string _sortingLayer = string.Empty;

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

            var particle = GetComponent<ParticleSystem> ();

            if (particle == null)
                return;

            var pRenderer = particle.GetComponent<Renderer> ();
            SortingOrder           = upCount;
            pRenderer.sortingOrder = SortingOrder;
        }

#endif
    }
}