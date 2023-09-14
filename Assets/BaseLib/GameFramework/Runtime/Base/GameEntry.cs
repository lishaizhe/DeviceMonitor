//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//
// GameFrameworkComponent其实意义不大，目前的实现是放到了一个LinkedList，
// 但是每次访问都涉及到遍历，实属消耗无谓的浪费。而且还是由底层去提供这种服务，那就更不可取。
// 第一是游戏内的组件定义就比较模糊，任何方便的全局类都可以定性为组件
// 第二是如果有大量的组件Register进去，那么再Get的时候效率就会成为潜在问题。
//     所以这里使用一个static变量去再缓存一遍数据，感觉非常不和谐。。。。。
// 可取的方式应该是定义FrameworkEntry，然后用户自己再去定义一个XXGameEntry : FrameworkEntry
// 通过继承，方便访问Framework提供的Component即可。
//
// 任何想给游戏做Framework的想法都是不成熟的，毕竟游戏是服务娱乐行业，定制化特性太正常了。
// 所以应该定性为BaseLib(基础类库)或者Kit(套件)。只给用户提供服务，不给用户提供Box(界限)。
//------------------------------------------------------------

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
                    m_event = GetComponent<EventComponent>();
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


        // private static TimerComponent Timer_;
        // public static TimerComponent Timer
        // {
        //     get
        //     {
        //         if (Timer_ == null)
        //         {
        //             Timer_ = GetComponent<TimerComponent>();
        //         }
        //         return Timer_;
        //
        //     }
        // }
        //
        // static private GlobalDataProxy globalDataProxy;
        // public static GlobalDataProxy GlobalData
        // {
        //     get
        //     {
        //
        //         return GlobalDataProxy.Instance;
        //     }
        // }

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
             
        // 判断是否正常游戏逻辑
        public static bool IsGame()
        {
            return Event != null ? true : false;
        }
    }
}
