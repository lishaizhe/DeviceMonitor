//

using GameFramework;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    
    public enum EventPoolMode
    {
        /// <summary>
        /// 默认事件池模式，即必须存在有且只有一个事件处理函数。
        /// </summary>
        Default = 0,

        /// <summary>
        /// 允许不存在事件处理函数。
        /// </summary>
        AllowNoHandler = 1,

        /// <summary>
        /// 允许存在多个事件处理函数。
        /// </summary>
        AllowMultiHandler = 2,

        /// <summary>
        /// 允许存在重复的事件处理函数。
        /// </summary>
        AllowDuplicateHandler = 4,
    }
    
    /// <summary>
    /// 事件组件。
    /// </summary>
    public sealed class EventComponent
    {
        public class ObjectPool<T>
        {
            private readonly Queue<T> _objects; 
            private readonly Func<T> _objectGenerator;

            public ObjectPool(Func<T> objectGenerator)
            {
                _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
                _objects = new Queue<T>();
            }

            public T Get()
            {
                if (_objects.Count > 0)
                {
                    return _objects.Dequeue();
                }
                return _objectGenerator();
            }

            public void Return(T item)
            {
                _objects.Enqueue(item);
            }

            public int Count()
            {
                return _objects.Count;
            }
        }
        
        private ObjectPool<List<Action<object>>> m_Pool = new ObjectPool<List<Action<object>>>(() => new List<Action<object>>(1));

        
        private readonly Dictionary<int, List<Action<object>>> m_EventHandlers;
        private readonly EventPoolMode m_EventPoolMode;
        
        /// <summary>
        /// 初始化事件池的新实例。
        /// </summary>
        /// <param name="mode">事件池模式。</param>
        public EventComponent(EventPoolMode mode)
        {
            m_EventHandlers = new Dictionary<int, List<Action<object>>>();
            m_EventPoolMode = mode;
        }

        public void OnUpdate(float elapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理事件池。
        /// </summary>
        public void Shutdown()
        {
            m_EventHandlers.Clear();
        }

        /// <summary>
        /// 检查订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        private bool Check(EventId id, Action<object> handler)
        {
#if UNITY_EDITOR
            if (handler == null)
            {
                return false;
            }

            List<Action<object>> handlers = null;
            if (!m_EventHandlers.TryGetValue((int)id, out handlers))
            {
                return false;
            }

            if (handlers == null || handlers.Count == 0)
            {
                return false;
            }

            foreach (Action<object> i in handlers)
            {
                if (i == handler)
                {
                    return true;
                }
            }
#endif
            return false;
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventID">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(EventId eventID, Action<object> handler)
        {
            if (handler == null)
            {
                return;
            }

            int id = (int)eventID;

            List<Action<object>> handlers = null;
            if (!m_EventHandlers.TryGetValue(id, out handlers) || handlers == null)
            {
                handlers = m_Pool.Get();
                handlers.Clear();
                handlers.Add(handler);
                
                m_EventHandlers[id] = handlers;
            }
            else if ((m_EventPoolMode & EventPoolMode.AllowMultiHandler) == 0)
            {
                return;
            }
            else if ((m_EventPoolMode & EventPoolMode.AllowDuplicateHandler) == 0 && Check(eventID, handler))
            {
                return;
            }
            else
            {
                //eventHandler += handler;
                //m_EventHandlers[id] = eventHandler;
                
                handlers.Add(handler);
            }
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventId">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(EventId eventId, Action<object> handler)
        {
            if (handler == null)
            {
                return;
            }

            int id = (int) eventId;
            
            // if (m_EventHandlers.ContainsKey(id))
            // {
            //     m_EventHandlers[id] -= handler;
            // }
            
            
            //这里要检查下value是否为null
            if (m_EventHandlers.TryGetValue(id, out var eventHandler) && eventHandler != null)
            {
                // 从后往前删除
                for (int i= eventHandler.Count - 1; i>=0; --i)
                {
                    if (eventHandler[i] == handler)
                    {
                        eventHandler.RemoveAt(i);
                    }
                }

                // 如果数量为0，则直接删除
                if (eventHandler.Count == 0)
                {
                    if (m_Pool.Count() < 128)
                    {
                        m_Pool.Return(eventHandler);
                    }

                    m_EventHandlers[id] = null;
                }
            }
        }

        /// <summary>
        /// 抛出事件
        /// </summary>
        public void Fire(EventId eventId, object userData = null)
        {
            HandleEvent(eventId, userData);
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        private void HandleEvent(EventId eventId, object userData)
        {
            int id = (int)eventId;
            //
            // 通知 CS 端处理
            //
            if (m_EventHandlers.TryGetValue(id, out var handlers) && handlers != null)
            {
                //Note: 这里不要用global list, 因为在handlerEvent过程中 极有可能会fire其他事件，然后再次调用HandleEvent 相当于递归调用 此时global list就会被清除 引起逻辑错误
                var current = m_Pool.Get();
                current.Clear();
                
                for (var i = 0; i < handlers.Count; i++)
                {
                    current.Add(handlers[i]);
                }

                int c = current.Count;
                for (int i = 0; i < c; ++i)
                {
                    try
                    {
                        current[i](userData);
                    }
                    catch(Exception exception)
                    {
                    }
                }
                
                m_Pool.Return(current);
            }
        }
        // private IEventManager m_EventManager = null;
        //
        // /// <summary>
        // /// 获取事件数量。
        // /// </summary>
        // public int Count
        // {
        //     get
        //     {
        //         return m_EventManager.Count;
        //     }
        // }
        //
        // /// <summary>
        // /// 游戏框架组件初始化。
        // /// </summary>
        // protected override void Awake()
        // {
        //     base.Awake();
        //
        //     m_EventManager = GameFrameworkEntry.GetModule<IEventManager>();
        //     if (m_EventManager == null)
        //     {
        //         Log.Fatal("Event manager is invalid.");
        //         return;
        //     }
        // }
        //
        // private void Start()
        // {
        //
        // }
        //
        // /// <summary>
        // /// 检查订阅事件处理回调函数。
        // /// </summary>
        // /// <param name="id">事件类型编号。</param>
        // /// <param name="handler">要检查的事件处理回调函数。</param>
        // /// <returns>是否存在事件处理回调函数。</returns>
        // public bool Check(int id, EventHandler<GameEventArgs> handler)
        // {
        //     return m_EventManager.Check(id, handler);
        // }
        //
        // /// <summary>
        // /// 订阅事件处理回调函数。
        // /// </summary>
        // /// <param name="id">事件类型编号。</param>
        // /// <param name="handler">要订阅的事件处理回调函数。</param>
        // public void Subscribe(EventId id, EventHandler<GameEventArgs> handler)
        // {
        //     m_EventManager.Subscribe(id, handler);
        // }
        //
        //
        // /// <summary>
        // /// 取消订阅事件处理回调函数。
        // /// </summary>
        // /// <param name="id">事件类型编号。</param>
        // /// <param name="handler">要取消订阅的事件处理回调函数。</param>
        // public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        // {
        //     m_EventManager.Unsubscribe(id, handler);
        // }
        //
        // /// <summary>
        // /// 设置默认事件处理函数。
        // /// </summary>
        // /// <param name="handler">要设置的默认事件处理函数。</param>
        // public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
        // {
        //     m_EventManager.SetDefaultHandler(handler);
        // }
        //
        // /// <summary>
        // /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        // /// </summary>
        // /// <param name="sender">事件发送者。</param>
        // /// <param name="e">事件内容。</param>
        // public void FireAsync(object sender, GameEventArgs e)
        // {
        //     m_EventManager.FireAsync(sender, e);
        // }
        //
        // /// <summary>
        // /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        // /// </summary>
        // /// <param name="sender">事件发送者。</param>
        // /// <param name="e">事件内容。</param>
        // public void Fire(object sender, GameEventArgs e)
        // {
        //     m_EventManager.Fire(sender, e);
        // }
        //
        // public void Fire(object sender, EventId eventId, object userData = null, int intPara = 0, string strPara = null)
        // {
        //     var e = ReferencePool.Acquire<CommonEventArgs>();
        //     e.setEventId(eventId);
        //     e.UserData = userData;
        //     e.IntPara1 = intPara;
        //     e.StrPara1 = strPara;
        //     
        //     m_EventManager.Fire(sender, e);
        // }
        //
        // public void Fire(EventId eventId, object userData = null, int intPara = 0, string strPara = null)
        // {
        //     var e = ReferencePool.Acquire<CommonEventArgs>();
        //     e.setEventId(eventId);
        //     e.UserData = userData;
        //     e.IntPara1 = intPara;
        //     e.StrPara1 = strPara;
        //     
        //     m_EventManager.Fire(null, e);
        // }
        
    }
}
