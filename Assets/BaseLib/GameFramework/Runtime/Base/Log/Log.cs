//

using System;
using System.Diagnostics;
using GameFramework;
using UnityGameFramework.Runtime;
using System.Collections;
using System.Text;

namespace GameFramework
{
    /// <summary>
    /// 日志类。
    /// </summary>
    public static partial class Log
    {
        private static ILogHelper s_LogHelper = null;
        public static  bool       Write       = false;

        /// <summary>
        /// 设置日志辅助器。
        /// </summary>
        /// <param name="logHelper">要设置的日志辅助器。</param>
        public static void SetLogHelper (ILogHelper logHelper)
        {
            s_LogHelper = logHelper;
        }

        public static bool IsCloseLog()
        {
#if CLOSE_LOG
            return true;
#endif
            return false;
        }

        // 日志总开关, 现在只用这一个开关
        public static void SetLogEnabled(bool isOpen)
        {
            Write = isOpen;
            //UnityEngine.Debug.unityLogger.logEnabled = isOpen;
        }

        // 最终调用底层写log，这个写log调用了N层函数。。。
        private static void _Log (LogLevel level, object message)
        {
            if (!Write)
            {
                return;
            }

            s_LogHelper?.Log (level, message);
        }
        
        // release分支打印日志的底层接口
        private static void _RlsLog (LogLevel level, object message)
        {
            // 先去掉标志位的限制吧
            // if (IsReleaseLog() == false)
            //     return;
            
            if (s_LogHelper != null)
            {
                s_LogHelper.Log (level, message);
                return;
            }
            
            // 如果对象没有被初始化， 直接使用unity引擎中的日志输出
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(message.ToString());
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(message.ToString());
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message.ToString());
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(message.ToString());
                    break;
                default:
                    throw new GameFrameworkException(message.ToString());
            }
        }


        [Conditional ("DEBUG_LOG")]
        public static void Info (string format, params object[] args)
        {
            _Log (LogLevel.Info, string.Format (format, args));
        }

        [Conditional ("DEBUG_LOG")]
        public static void Info (string str)
        {
            _Log (LogLevel.Info, str);
        }

        //这个函数是为了兼容之前的代码。。。
        [Conditional ("DEBUG_LOG")]
        public static void Info (object obj)
        {
            _Log (LogLevel.Info, obj);
        }

        [Conditional ("DEBUG_LOG")]
        public static void Debug (string format, params object[] args)
        {
            _Log (LogLevel.Debug, string.Format (format, args));
        }

        [Conditional ("DEBUG_LOG")]
        public static void Debug (string str)
        {
            _Log (LogLevel.Debug, str);
        }

        [Conditional ("DEBUG_LOG")]
        public static void Warning (string format, params object[] args)
        {
            _Log (LogLevel.Warning, string.Format (format, args));
        }

        [Conditional ("DEBUG_LOG")]
        public static void Warning (string format)
        {
            _Log (LogLevel.Warning, format);
        }

        //这个函数是为了兼容之前的代码。。。
        [Conditional ("DEBUG_LOG")]
        public static void Warning (object str)
        {
            _Log (LogLevel.Warning, str);
        }

        [Conditional ("DEBUG_LOG")]
        public static void Error (string format, params object[] args)
        {
            _Log (LogLevel.Error, string.Format (format, args));
        }
        
        [Conditional ("DEBUG_LOG")]
        public static void Error (string str)
        {
            _Log (LogLevel.Error, str);
        }

        //这个函数是为了兼容之前的代码。。。
        [Conditional ("DEBUG_LOG")]
        public static void Error (object obj)
        {
            _Log (LogLevel.Error, obj);
        }

        [Conditional ("DEBUG_LOG")]
        public static void Fatal (string str)
        {
            _Log (LogLevel.Fatal, str);
        }
        
        #region 正式版本上打印日志接口  [不加条件编译，预留出来之后 Release包不屏蔽此类日志] 

        // release debug 线上调试日志输出
        public static void ReleaseDebug(string str)
        {
            _RlsLog(LogLevel.Debug, str);
        }
        
        public static void ReleaseDebug(string tag, string message)
        {
            ReleaseDebug ($"#{tag}# {message}");
        }

        // release warning 线上警告日志输出
        public static void ReleaseWarning(string str)
        {
            _RlsLog(LogLevel.Warning, str);
        }

        // release error 线上报错日志输出
        public static void ReleaseError(string str)
        {
            _RlsLog(LogLevel.Error, str);
        }
        
        public static void ReleaseError(string tag, string message)
        {
            ReleaseError ($"#{tag}# {message}");
        }
        
        #endregion
    }
}

public static class LuaHelperLog
{

    public static void Info (string str)
    {
        Log.Info (str);
    }

    public static void Error (string str)
    {
        Log.Error (str);
    }

    public static void ReleaseDebug (string str)
    {
        Log.ReleaseDebug (str);
    }

    public static void ReleaseDebug (string tag, string message)
    {
        Log.ReleaseDebug ($"#{tag}# {message}");
    }

    // release error 线上报错日志输出
    public static void ReleaseError (string str)
    {
        Log.ReleaseError (str);
    }

    public static void ReleaseError (string tag, string message)
    {
        Log.ReleaseError (tag, message);
    }

    public static bool Write => Log.Write;
}