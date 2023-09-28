
using GameFramework;
using GameKit.Base;
using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public static class GameEntry
    {
        private const string UnityGameFrameworkVersion = "3.1.3";
        private static readonly LinkedList<GameFrameworkComponent> s_GameFrameworkComponents = new LinkedList<GameFrameworkComponent>();

        /// <summary>
        /// 游戏框架所在的场景编号。
        /// </summary>
        internal const int GameFrameworkSceneId = 0;

        /// <summary>
        /// 获取 Unity 游戏框架版本号。
        /// </summary>
        public static string Version
        {
            get
            {
                return UnityGameFrameworkVersion;
            }
        }
        /// <summary>
        /// ToAll GetComponent频繁调用，有大量GC的产生，这块缓存一下把
        /// </summary>
        private static BaseComponent gameBase = null;

        public static BaseComponent GameBase
        {
            get
            {
                if(gameBase==null)
                {
                    gameBase = GetComponent<BaseComponent>();

                }
                return gameBase;
            }
        }

        private static EventComponent m_event = null;
        public static EventComponent Event
        {
            get
            {
                if(m_event==null)
                {
                    m_event = new EventComponent(EventPoolMode.AllowNoHandler | EventPoolMode.AllowMultiHandler);
                }
                return m_event;
            }
        }

        private static UIComponent ui = null;
        public static UIComponent UI
        {
            get
            {
                if(ui==null)
                {
                    ui = GetComponent<UIComponent>();
                }
                return ui;
            }
        }

        public static WebRequestManager WebRequest
        {
            get
            {
                return WebRequestManager.Instance;
            }
        }

        static private SettingProxy settingProxy;
        public static SettingProxy Setting
        {
            get
            {
                if (settingProxy == null)
                    settingProxy = SettingProxy.Instance; //ApplicationFacade.Instance.RetrieveProxy(SettingProxy.NAME) as SettingProxy;
                return settingProxy;
            }
        }

        static private GameObject m_cacheRoot;

        public static GameObject CacheRoot
        {
            get
            {
                if (m_cacheRoot == null)
                    m_cacheRoot = new GameObject("CacheRoot");
                return m_cacheRoot;
            }
        }

        static private ResourceManager _resourceManager;
        public static ResourceManager Resource
        {
            get
            {
                if (_resourceManager == null)
                    _resourceManager = new ResourceManager();
                return _resourceManager;
            }
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架组件类型。</typeparam>
        /// <returns>要获取的游戏框架组件。</returns>
        public static T GetComponent<T>() where T : GameFrameworkComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <param name="type">要获取的游戏框架组件类型。</param>
        /// <returns>要获取的游戏框架组件。</returns>
        public static GameFrameworkComponent GetComponent(Type type)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <param name="typeName">要获取的游戏框架组件类型名称。</param>
        /// <returns>要获取的游戏框架组件。</returns>
        public static GameFrameworkComponent GetComponent(string typeName)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                Type type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        /// <param name="shutdownType">关闭游戏框架类型。</param>
        public static void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("Shutdown Game Framework ({0})...", shutdownType.ToString());

            // 如果是Quit的话，那么就直接直接退出，不释放Component，
            // 否则有可能在当前帧里会有很多访问Component但是Component为null的情况
            if (shutdownType == ShutdownType.Quit)
            {
                Application.Quit();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                return;
            }

            BaseComponent baseComponent = GetComponent<BaseComponent>();
            if (baseComponent != null)
            {
                baseComponent.Shutdown();
                baseComponent = null;
            }

            s_GameFrameworkComponents.Clear();


            if (shutdownType == ShutdownType.None)
            {
                return;
            }

            if (shutdownType == ShutdownType.Restart)
            {
                SceneManager.LoadScene(GameFrameworkSceneId);
                return;
            }


        }

        /// <summary>
        /// 注册游戏框架组件。
        /// </summary>
        /// <param name="gameFrameworkComponent">要注册的游戏框架组件。</param>
        internal static void RegisterComponent(GameFrameworkComponent gameFrameworkComponent)
        {
            if (gameFrameworkComponent == null)
            {
                Log.Error("Game Framework component is invalid.");
                return;
            }

            Type type = gameFrameworkComponent.GetType();
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    Log.Error("Game Framework component type '{0}' is already exist.", type.FullName);
                    return;
                }

                current = current.Next;
            }

            s_GameFrameworkComponents.AddLast(gameFrameworkComponent);
        }

        public static void Update(float elapseSeconds)
        {
            Resource.Update();
        }

    }
}
