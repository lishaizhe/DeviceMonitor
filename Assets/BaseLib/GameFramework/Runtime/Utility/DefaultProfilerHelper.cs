﻿//

using GameFramework;
using System.Threading;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 性能分析辅助器。
    /// </summary>
    public class DefaultProfilerHelper : Utility.Profiler.IProfilerHelper
    {
        private Thread m_MainThread = null;

        /// <summary>
        /// 初始化性能分析辅助器的新实例。
        /// </summary>
        /// <param name="mainThread">当前进程的主线程。</param>
        public DefaultProfilerHelper(Thread mainThread)
        {
            if (mainThread == null)
            {
                Log.Error("Main thread is invalid.");
                return;
            }

            m_MainThread = mainThread;
        }

        /// <summary>
        /// 开始采样。
        /// </summary>
        /// <param name="name">采样名称。</param>
        public void BeginSample(string name)
        {
            if (Thread.CurrentThread != m_MainThread)
            {
                return;
            }

#if UNITY_5_5_OR_NEWER
            UnityEngine.Profiling.Profiler.BeginSample(name);
#else
            UnityEngine.Profiler.BeginSample(name);
#endif
        }

        /// <summary>
        /// 结束采样。
        /// </summary>
        public void EndSample()
        {
            if (Thread.CurrentThread != m_MainThread)
            {
                return;
            }

#if UNITY_5_5_OR_NEWER
            UnityEngine.Profiling.Profiler.EndSample();
#else
            UnityEngine.Profiler.EndSample();
#endif
        }
    }
}
