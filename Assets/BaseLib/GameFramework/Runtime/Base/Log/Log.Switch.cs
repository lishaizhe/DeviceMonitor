/***
 * Created by Darcy
 * Date: Monday, 06 December 2021
 * Time: 15:56:49
 * Description: 从 Log.cs 类迁移过来的 原作者 @xibao
 ***/

namespace GameFramework
{
    public static partial class Log
    {
        
        #region 不同模块日志开关

        private static bool m_isMessage = false;
        private static bool m_isFlatBuffer = false;
        private static bool m_isLoad = false;
        private static bool m_isDevice = false;
        private static bool m_isLua = false;
        private static bool m_isFowViewer = false;
        
        private static bool m_isMovie = false;
        private static bool m_isTask = false;
        private static bool m_isGuide = false;
        private static bool m_isChat = false;
        private static bool m_isMainScene = false;
        private static bool m_isWorldScene = false;
        private static bool m_isRobot = false;
        private static bool m_isReleaseLog = true;

        public static void UpdateLogSwitchState()
        {
        }

        public static bool IsMessage()
        {
            if (Log.Write == false)
                return false;

            return m_isMessage;
        }

        public static bool IsFlatBuffer()
        {
            if (Log.Write == false)
                return false;

            return m_isFlatBuffer;
        }
        
        public static bool IsLoad()
        {
            if (Log.Write == false)
                return false;

            return m_isLoad;
        }
        
        public static bool IsDevice()
        {
            if (Log.Write == false)
                return false;

            return m_isDevice;
        }

        public static bool IsLua()
        {
            if (Log.Write == false)
                return false;

            return m_isLua;
        }
        
        public static bool IsFowViewer()
        {
            if (Log.Write == false)
                return false;

            return m_isFowViewer;
        }
        
        public static bool IsMovie()
        {
            if (Log.Write == false)
                return false;

            return m_isMovie;
        }
        
        public static bool IsTask()
        {
            if (Log.Write == false)
                return false;

            return m_isTask;
        }
        
        public static bool IsGuide()
        {
            if (Log.Write == false)
                return false;

            return m_isGuide;
        }
        
        public static bool IsChat()
        {
            if (Log.Write == false)
                return false;

            return m_isChat;
        }
        
        public static bool IsMainScene()
        {
            if (Log.Write == false)
                return false;

            return m_isMainScene;
        }
        
        public static bool IsWorldScene()
        {
            if (Log.Write == false)
                return false;

            return m_isWorldScene;
        }

        public static bool IsRobot()
        {
            if (Log.Write == false)
                return false;

            return m_isRobot;
        }
        
        public static bool IsReleaseLog()
        {
            // 不受 Log.Write 控制
            // if (Log.Write == false)
                // return false;

            return m_isReleaseLog;
        }

        #endregion
    }
}