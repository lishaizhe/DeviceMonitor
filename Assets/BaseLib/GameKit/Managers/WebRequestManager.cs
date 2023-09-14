using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityEngine.Networking;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GameKit.Base
{
    public class WebRequestManager : SingletonBehaviour<WebRequestManager>
    {
        [System.Serializable]
        struct WebRequestParams
        {
            public OnWebRequestCallback calback;
            //public int requestTimes;
            public int priority;
            public object userdata;
        }
        
        public delegate void OnWebRequestCallback(UnityWebRequest request, bool hasErr, object userdata);

#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        private readonly Dictionary<UnityWebRequest, UnityWebRequestAsyncOperation> m_WorkingRequests = new Dictionary<UnityWebRequest, UnityWebRequestAsyncOperation>();
        private readonly List<UnityWebRequest> m_WaitingRequests = new List<UnityWebRequest>();
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        private readonly Dictionary<UnityWebRequest, WebRequestParams> m_ParamsStack = new Dictionary<UnityWebRequest, WebRequestParams>();
        [SerializeField]
        private int maxWorkingWebRequestThread = 3;

        private Dictionary<UnityWebRequest, UnityWebRequestAsyncOperation>.Enumerator enumeratorWorking;

        private readonly List<UnityWebRequest> keysToRemove = new List<UnityWebRequest>();

        //有了Instance 为什么要加GetInstance ？ 因为要暴露给lua用
        public static WebRequestManager GetInstance()
        {
            return Instance;
        }

        public void Initialize()
        {
            Initialize(3);
        }

        public void Initialize(int maxRequestThread)
        {
            maxWorkingWebRequestThread = maxRequestThread;
            System.Net.ServicePointManager.DefaultConnectionLimit = maxRequestThread;
        }

        public override void Release()
        {
            base.Release();

            enumeratorWorking = m_WorkingRequests.GetEnumerator();
            while (enumeratorWorking.MoveNext())
            {
                UnityWebRequest request = enumeratorWorking.Current.Value.webRequest;
                Dispose(request);
            }
            m_WorkingRequests.Clear();
        }

        public bool IsDownloadResult(UnityWebRequest request, string result)
        {
            if (string.IsNullOrEmpty(result))
                return request.downloadedBytes == 0;

            return request.downloadedBytes == (ulong)result.Length && request.downloadHandler.text == result;
        }

        public void LoadAssetBundle(string uri, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequestAssetBundle.GetAssetBundle(uri, 0), callback, priority, timeout, userdata);
        }

        public void LoadAssetBundle(string uri, Hash128 hash, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequestAssetBundle.GetAssetBundle(uri, hash, 0), callback, priority, timeout, userdata);
        }

        public void LoadTexture(string uri, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequestTexture.GetTexture(uri, false), callback, priority, timeout, userdata);
        }

        public void LoadTexture(string uri, bool nonReadable, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequestTexture.GetTexture(uri, nonReadable), callback, priority, timeout, userdata);
        }

        public void LoadMultimedia(string uri, AudioType audioType, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequestMultimedia.GetAudioClip(uri, audioType), callback, priority, timeout, userdata);
        }

        public void Get(string uri, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Get(uri), callback, priority, timeout, userdata);
        }

        public void Post(string uri, string postData, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Post(uri, postData), callback, priority, timeout, userdata);
        }

        public void Post(string uri, WWWForm formData, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Post(uri, formData), callback, priority, timeout, userdata);
        }

        public void Post(string uri, List<IMultipartFormSection> multipartFormSections, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Post(uri, multipartFormSections), callback, priority, timeout, userdata);
        }

        public void Post(string uri, Dictionary<string, string> formFields, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Post(uri, formFields), callback, priority, timeout, userdata);
        }

        public void Head(string uri, OnWebRequestCallback callback, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Head(uri), callback, priority, timeout, userdata);
        }

        public void Put(string uri, string bodyData, OnWebRequestCallback callback = null, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Put(uri, bodyData), callback, priority, timeout, userdata);
        }

        public void Put(string uri, byte[] bodyData, OnWebRequestCallback callback = null, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Put(uri, bodyData), callback, priority, timeout, userdata);
        }

        public void Delete(string uri, OnWebRequestCallback callback = null, int priority = 0, int timeout = 0, object userdata = null)
        {
            Request(UnityWebRequest.Delete(uri), callback, priority, timeout, userdata);
        }

        private void Request(UnityWebRequest request, OnWebRequestCallback callback = null, int priority = 0, int timeout = 0, object userdata = null)
        {
            if (request != null)
            {
                request.timeout = timeout;
                m_WaitingRequests.Add(request);
                m_ParamsStack.Add(request, new WebRequestParams { calback = callback, priority = priority, userdata = userdata });
                if (m_WorkingRequests.Count >= maxWorkingWebRequestThread)
                {
                    Log.ReleaseWarning($"maxWorkingWebRequestThread = {maxWorkingWebRequestThread} ,WorkingRequestsCount = {m_WorkingRequests.Count},WaitRequestsCount = {m_WaitingRequests.Count} ");
                }
            }
        }

        protected override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);

            enumeratorWorking = m_WorkingRequests.GetEnumerator();
            while (enumeratorWorking.MoveNext())
            {
                UnityWebRequest key = enumeratorWorking.Current.Key;
                UnityWebRequestAsyncOperation operation = enumeratorWorking.Current.Value;

                try
                {
                    if (m_ParamsStack.TryGetValue(operation.webRequest, out WebRequestParams param))
                    {
                        bool isError = false;
                        if (operation.webRequest.isHttpError || operation.webRequest.isNetworkError)
                        {
                            isError = true;
                            Log.ReleaseWarning($"{operation.webRequest.error} : {key}, url = {operation.webRequest.url}, " +
                                                  $"httpError = {operation.webRequest.isHttpError}, netError = {operation.webRequest.isNetworkError}");
                        }

                        if (operation.isDone)
                        {
                            // 稍后从working中移除
                            keysToRemove.Add(key);
                        }
                        // 通知最终用户请求进度
                        param.calback?.Invoke(operation.webRequest, isError, param.userdata);
                    }
                    else
                    {
                        keysToRemove.Add(key);
                        Log.ReleaseError($"{operation.webRequest.url} is out of control");
                    }
                }
                catch (System.Exception e)
                {
                    keysToRemove.Add(key);
                    Log.ReleaseError(e.ToString());
                }
            }

            for (int i = 0; i < keysToRemove.Count; ++i)
            {
                if (m_WorkingRequests.TryGetValue(keysToRemove[i], out UnityWebRequestAsyncOperation operation))
                {
                    m_WorkingRequests.Remove(keysToRemove[i]);
                    m_ParamsStack.Remove(operation.webRequest);
                    Dispose(operation.webRequest);
                }
            }
            keysToRemove.Clear();

            if (m_WorkingRequests.Count < maxWorkingWebRequestThread)
            {
                for (int i = 0; i < m_WaitingRequests.Count;)
                {
                    if (m_WaitingRequests[i] != null)
                    {
                        UnityWebRequest request = m_WaitingRequests[i];
                        if (!m_WorkingRequests.ContainsKey(request))
                        {
                            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                            operation.priority = m_ParamsStack[request].priority;
                            m_WorkingRequests.Add(request, operation);
                            m_WaitingRequests.RemoveAt(i);

                            if (m_WorkingRequests.Count >= maxWorkingWebRequestThread)
                            {
                                Log.ReleaseWarning($"maxWorkingWebRequestThread = {maxWorkingWebRequestThread} ,WorkingRequestsCount = {m_WorkingRequests.Count},WaitRequestsCount = {m_WaitingRequests.Count} ");
                                break;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        m_WaitingRequests.RemoveAt(i);
                    }
                }
            }

            //if (m_WorkingRequests.Count > 0)
                //Debugger.LogFormat("{0} webrequest thread in working!", m_WorkingRequests.Count);
        }

        private void Dispose(UnityWebRequest request)
        {
            if (request != null)
            {
                request.Dispose();
                System.GC.SuppressFinalize(request);
            }
        }
    }
}
