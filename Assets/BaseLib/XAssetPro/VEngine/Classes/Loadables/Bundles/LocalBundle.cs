using UnityEngine;

namespace VEngine
{
    /// <summary>
    ///     从本地加载的Bundle，默认使用 LoadFromFile 这个最高效的接口。
    /// </summary>
    internal class LocalBundle : Bundle
    {
        private AssetBundleCreateRequest request;

        protected override void OnLoad()
        {
            if (!Versions.CheckWhiteList || Versions.IsInWhiteList(GetOriginBundleName(info.name)))
            {
                request = AssetBundle.LoadFromFileAsync(pathOrURL);
            }
            else
            {
                Versions.WhiteListFailed.Add(info.name);
                Finish("LocalBundle.OnLoad, not in whitelist");
            }
        }

        private string GetOriginBundleName(string nameWithAppendHash)
        {
            int i = nameWithAppendHash.LastIndexOf('_');
            if (i <= 0)
                return nameWithAppendHash;
            i = nameWithAppendHash.LastIndexOf('_', i - 1);
            if (i <= 0)
                return nameWithAppendHash;
            return nameWithAppendHash.Substring(0, i);
        }

        public override void LoadImmediate()
        {
            if (isDone)
            {
                return;
            }

            assetBundle = request.assetBundle;
            if (assetBundle == null)
            {
                Finish("LocalBundle.LoadImmediate, assetBundle == null");
                return;
            }

            Finish();
            request = null;
        }

        private void OnLoaded()
        {
            if (request == null)
            {
                Finish("LocalBundle.OnLoaded, request == null");
                return;
            }

            assetBundle = request.assetBundle;
            request = null;
            if (assetBundle == null)
            {
                Finish("LocalBundle.OnLoaded, assetBundle == null");
                return;
            }

            Finish();
        }

        protected override void OnUpdate()
        {
            if (status != LoadableStatus.Loading)
            {
                return;
            }

            if (request == null)
            {
                OnLoad();
                return;
            }

            progress = request.progress;
            if (request.isDone)
            {
                OnLoaded();
            }
        }
    }
}