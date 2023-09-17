using System.Collections.Generic;
using UnityEngine;

namespace VEngine
{
    /// <summary>
    ///     资源的依赖，管理资源依赖的 Bundles 的加载和回收。
    /// </summary>
    public class Dependencies : Loadable
    {
        /// <summary>
        ///     资源所有依赖的 bundle
        /// </summary>
        internal readonly List<Bundle> bundles = new List<Bundle>();


        /// <summary>
        ///     资源所在的 bundle
        /// </summary>
        internal Bundle bundle;

        internal AssetBundle assetBundle => bundle?.assetBundle;

        protected override void OnLoad()
        {
            if (!Versions.GetDependencies(pathOrURL, out var info, out var infos))
            {
                Finish("Dependencies.OnLoad, Dependencies not found");
                return;
            }

            if (info == null)
            {
                Finish("Dependencies.OnLoad, info == null");
                return;
            }

            bundle = Bundle.LoadInternal(info, mustCompleteOnNextFrame);
            bundles.Add(bundle);
            if (infos != null && infos.Length > 0)
            {
                foreach (var item in infos)
                {
                    bundles.Add(Bundle.LoadInternal(item, mustCompleteOnNextFrame));
                }
            }
        }

        public override void LoadImmediate()
        {
            if (isDone)
            {
                return;
            }

            foreach (var request in bundles)
            {
                request.LoadImmediate();
            }
        }

        protected override void OnUnload()
        {
            if (bundles.Count > 0)
            {
                foreach (var item in bundles)
                {
                    if (string.IsNullOrEmpty(item.error))
                    {
                        item.Release();
                    }
                }

                bundles.Clear();
            }

            bundle = null;
        }

        protected override void OnUpdate()
        {
            switch (status)
            {
                case LoadableStatus.Loading:
                    var totalProgress = 0f;
                    var allDone = true;
                    foreach (var child in bundles)
                    {
                        totalProgress += child.progress;
                        if (!string.IsNullOrEmpty(child.error))
                        {
                            status = LoadableStatus.FailedToLoad;
                            error = child.error;
                            progress = 1;
                            return;
                        }

                        if (child.isDone)
                        {
                            continue;
                        }

                        allDone = false;
                        break;
                    }

                    progress = totalProgress / bundles.Count * 0.5f;
                    if (allDone)
                    {
                        if (assetBundle == null)
                        {
                            Finish("Dependencies.OnLoad, assetBundle == null");
                            return;
                        }

                        Finish();
                    }

                    break;
            }
        }
    }
}