using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework;
using Shelter.Scripts.Tools.BaseLib.GameFramework.Runtime.UI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace BaseLib.GameFramework.Runtime.UI
{
    [ExecuteInEditMode]
    public class UPParticlesSortingOrder : MonoBehaviour, IForceUpdateOrder
    {

        /// <summary>
        /// 相对于当前UI实际运行时的 Group 的层级提高的层级
        /// </summary>
        [Header ("相对于UI实际显示的 Group 的层级提高的层级")] [SerializeField]
        private int upCount;
        [Header ("设置拖尾层级")] [SerializeField]
        private string setSortingLayerConfig;

        [SerializeField] private List<ParticleSystem> particles;
        [SerializeField] private List<TrailRenderer> trailRenderers;

        private bool haveUpdateSortingOrder = false;

        public void UpdateSortingOrder(int baseSortingOrder, string sortingLayerName)
        {
            if (this == null)
                return;

            UpdateSortingOrderAndLayer(baseSortingOrder, sortingLayerName);
            UpdateTrailRendererSortingOrderAndLayer(baseSortingOrder, sortingLayerName);

            this.haveUpdateSortingOrder = true;
        }

        private void Start ()
        {
            if (this.haveUpdateSortingOrder)
                return;

            UpdateSortingOrderAndLayer ();
            UpdateTrailRendererSortingOrderAndLayer();
        }

        private void UpdateSortingOrderAndLayer(int baseSortingOrder = -1, string sortingLayerName = null)
        {
            if (particles == null || particles.Count < 1)
                return;

            int order;
            string layer;

            if(baseSortingOrder >= 0 && !string.IsNullOrEmpty(sortingLayerName))
            {
                order = baseSortingOrder;
                layer = sortingLayerName;
            }
            else
            {
                (order, layer) = this.GetBaseSortingOrderAndLayer();
            }

            var tOrder = order + upCount;

            if (particleSortingOrder == tOrder
                && string.CompareOrdinal (_sortingLayer, layer) == 0)
            {
                return;
            }
            
            particleSortingOrder  = tOrder;
            _sortingLayer = layer;

            foreach (var item in particles)
            {
                if (item == null)
                    continue;
                var tRenderer = item.GetComponent<Renderer> ();
                tRenderer.sortingOrder     = particleSortingOrder;
                tRenderer.sortingLayerName = layer;
            }
        }
        
        private void UpdateTrailRendererSortingOrderAndLayer (int baseSortingOrder = -1, string sortingLayerName = null)
        {
            if (trailRenderers == null || trailRenderers.Count < 1)
                return;

            int order;
            string layer;

            if (baseSortingOrder >= 0 && !string.IsNullOrEmpty(sortingLayerName))
            {
                order = baseSortingOrder;
                layer = sortingLayerName;
            }
            else
            {
                (order, layer) = this.GetBaseSortingOrderAndLayer();
            }

            var tOrder = order + upCount;

            if (trailSortingOrder == tOrder
                && string.CompareOrdinal (_sortingLayer, layer) == 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(setSortingLayerConfig))
            {
                layer = setSortingLayerConfig;
            }

            trailSortingOrder  = tOrder;
            _sortingLayer = layer;

            foreach (var item in trailRenderers)
            {
                if (item == null)
                    continue;
                item.sortingOrder     = trailSortingOrder;
                item.sortingLayerName = layer;
            }
        }

        public  int    particleSortingOrder { private set; get; }
        public  int    trailSortingOrder { private set; get; }
        private string _sortingLayer = string.Empty;

#if UNITY_EDITOR

        public void DrawButtons ()
        {
            if (GUILayout.Button ("FindParticles"))
            {
                if (particles == null)
                    particles = new List<ParticleSystem> ();
                particles.Clear ();
                particles = transform.GetComponentsInChildren<ParticleSystem> (true).ToList ();
            }
            if (GUILayout.Button ("FindTrailRenderer"))
            {
                if (trailRenderers == null)
                    trailRenderers = new List<TrailRenderer> ();
                trailRenderers.Clear ();
                trailRenderers = transform.GetComponentsInChildren<TrailRenderer> (true).ToList ();
            }

            if (GUILayout.Button ("Clear"))
            {
                if (particles != null)
                {
                    particles.Clear ();
                }
                if (trailRenderers != null)
                {
                    trailRenderers.Clear ();
                }
            }
        }

        /// <summary>
        /// 便于编辑模式时调试
        /// </summary>
        private void Update ()
        {
            if (Application.isPlaying)
                return;

            if (particleSortingOrder != upCount)
            {
                particleSortingOrder = upCount;
                if (particles != null || particles.Count > 0)
                {
                    foreach (var item in particles)
                    {
                        if (item == null)
                            continue;
                        var tRenderer = item.GetComponent<Renderer> ();
                        tRenderer.sortingOrder = particleSortingOrder;
                    }
                }
            }
            
            if (trailSortingOrder != upCount)
            {
                trailSortingOrder = upCount;
                if (trailRenderers != null || trailRenderers.Count > 0)
                {
                    foreach (var item in trailRenderers)
                    {
                        if (item == null)
                            continue;
                        item.sortingOrder = trailSortingOrder;
                    }
                }
            }
        }

#endif

    }
}