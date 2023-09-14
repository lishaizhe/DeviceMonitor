//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using AssetBundles;
using GameFramework;
using GameKit.Base;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class ObjectPoolInformationWindow : ScrollableDebuggerWindowBase
        {
            //private ObjectPoolComponent m_ObjectPoolComponent = null;

            private Dictionary<GameObject, int> spawnObjs = new Dictionary<GameObject, int>();

            public override void Initialize(params object[] args)
            {
                TakeSample();
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Object Pool Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Object Pool Count", ObjectPool.CountAllPooled().ToString());
                }
                GUILayout.EndVertical();
                //ObjectPoolBase[] objectPools = ObjectPool.Instance.CountAllPooled().ToString();
                //for (int i = 0; i < objectPools.Length; i++)
                //{
                //    DrawObjectPool(objectPools[i]);
                //}

                if (GUILayout.Button("Take Snap for Spawn objects", GUILayout.Height(30f)))
                {
                    TakeSample();
                }

                DrawObjectPool();
            }

            private void TakeSample()
            {
                spawnObjs.Clear();

                //遍历Key和Value
                foreach (var item in ObjectPool.Instance.spawnedObjects)
                {
                    if (spawnObjs.ContainsKey(item.Value))
                    {
                        spawnObjs[item.Value]++;
                    }
                    else
                    {
                        spawnObjs.Add(item.Value, 1);
                    }
                }
            }


            private void DrawObjectPool()
            {

#if UNITY_EDITOR
                GUILayout.Label(string.Format("<b>Bundle Size: {0}</b>", AssetBundleManager.m_SimulateAssetBundleList.Count.ToString()));
                GUILayout.BeginVertical("box");
                {
                    //遍历Key和Value
                    for (int i=0;i<AssetBundleManager.m_SimulateAssetBundleList.Count;++i)
                    {
                        DrawItem(AssetBundleManager.m_SimulateAssetBundleList[i], "0");
                    }
                }
                GUILayout.EndVertical();
#else
                GUILayout.Label(string.Format("<b>Bundle Size: {0}</b>", AssetBundleManager.m_LoadedAssetBundles.Count.ToString()));
                GUILayout.BeginVertical("box");
                {
                    //遍历Key和Value
                    foreach (var item in AssetBundleManager.m_LoadedAssetBundles)
                    {
                        DrawItem(item.Key, item.Value.m_ReferencedCount.ToString());
                    }
                }
                GUILayout.EndVertical();
#endif



                GUILayout.Label(string.Format("<b>Object Spawn Size: {0}</b>", spawnObjs.Count.ToString()));
                GUILayout.BeginVertical("box");
                {
                    //遍历Key和Value
                    foreach (var item in spawnObjs)
                    {
                        DrawItem(item.Key.name, item.Value.ToString());
                    }
                }
                GUILayout.EndVertical();

                GUILayout.Label(string.Format("<b>Object Pool Size: {0}</b>", ObjectPool.Instance.pooledObjects.Count.ToString()));
                GUILayout.BeginVertical("box");
                {
                    //遍历Key和Value
                    foreach (var item in ObjectPool.Instance.pooledObjects)
                    {
                        DrawItem(item.Key.name, item.Value.Count.ToString());
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
