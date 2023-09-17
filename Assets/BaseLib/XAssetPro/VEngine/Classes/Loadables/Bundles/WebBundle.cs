using UnityEngine;
using UnityEngine.Networking;

namespace VEngine
{
    /// <summary>
    ///     从服务器加载的 Bundle，针对 WebGL 平台提供支持
    /// </summary>
    internal class WebBundle : Bundle
    {
        private AsyncOperation operation;
        private UnityWebRequest request;

        protected override void OnLoad()
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle(pathOrURL);
            operation = request.SendWebRequest();
        }

        protected override void OnUpdate()
        {
            if (status != LoadableStatus.Loading)
            {
                return;
            }

            if (request == null || operation == null)
            {
                return;
            }

            progress = operation.progress;
            if (!string.IsNullOrEmpty(request.error))
            {
                Finish(request.error);
                request.Dispose();
                request = null;
                operation = null;
                return;
            }

            if (!operation.isDone)
            {
                return;
            }

            assetBundle = DownloadHandlerAssetBundle.GetContent(request);
            Finish(assetBundle == null ? "assetBundle == null" : null);
            request.Dispose();
            request = null;
            operation = null;
        }
    }
}