//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

//
// 这个类问题很多，且有GC，本人因为今天加班，所以把这个代码重新处理了一下!
// 简单来讲，Event不能变成多播（multicast)，即EventHandler += handler，否则就会产生GC。
// 由于这个类的接口不好修改，否则代码改动量太大，所以这里我没有使用多播，直接做了一个List池。
// 这个设计完全是为了优化，如果有什么想讨论的，欢迎找我！
//                        -- liusiyang 写于 2021.04.26 凌晨01:33
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 事件池。
    /// </summary>
    /// <typeparam name="T">事件类型。</typeparam>
    internal sealed partial class EventPool<T> where T : BaseEventArgs
    {
        // 这里先做成线程不安全的，我们似乎没有在多线程中使用这个
        public class ObjectPool<T>
        {
//            private readonly ConcurrentBag<T> _objects;    
            private readonly Queue<T> _objects; 
            private readonly Func<T> _objectGenerator;

            public ObjectPool(Func<T> objectGenerator)
            {
                _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
//                _objects = new ConcurrentBag<T>();
                _objects = new Queue<T>();
            }

            public T Get()
            {
//                var o = _objects.TryTake(out T item) ? item : _objectGenerator();

                if (_objects.Count > 0)
                {
                    return _objects.Dequeue();
                }
                
                return _objectGenerator();
//                var o = _objects.TryTake(out T item) ? item : _objectGenerator();
//                return o;
            }

            public void Return(T item)
            {
//                _objects.Add(item);
                _objects.Enqueue(item);
            }

            public int Count()
            {
                return _objects.Count;
            }
        }

        private readonly Dictionary<int, List<EventHandler<T>>> m_EventHandlers;
        private readonly Queue<Event> m_Events;
        private readonly EventPoolMode m_EventPoolMode;
        private EventHandler<T> m_DefaultHandler;

        // 处理一个缓存列表
        private ObjectPool<List<EventHandler<T>>> m_Pool = new ObjectPool<List<EventHandler<T>>>(() => new List<EventHandler<T>>(1));


        /// <summary>
        /// 初始化事件池的新实例。
        /// </summary>
        /// <param name="mode">事件池模式。</param>
        public EventPool(EventPoolMode mode)
        {
            m_EventHandlers = new Dictionary<int, List<EventHandler<T>>>();
            m_Events = new Queue<Event>();
            m_EventPoolMode = mode;
            m_DefaultHandler = null;
        }

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        public int Count
        {
            get
            {
                return m_Events.Count;
            }
        }

        /// <summary>
        /// 事件池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_Events.Count > 0)
            {
                Event e = null;
                lock (m_Events)
                {
                    e = m_Events.Dequeue();
                }

                HandleEvent(e.Sender, e.EventArgs);
            }
        }

        /// <summary>
        /// 关闭并清理事件池。
        /// </summary>
        public void Shutdown()
        {
            Clear();
            m_EventHandlers.Clear();
            m_DefaultHandler = null;
        }

        /// <summary>
        /// 清理事件。
        /// </summary>
        public void Clear()
        {
            lock (m_Events)
            {
                m_Events.Clear();
            }
        }

        /// <summary>
        /// 检查订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        public bool Check(int id, EventHandler<T> handler)
        {
            // 此函数比较耗，因为是一个O(n)操作，这个理论上只需要在编辑模式生效
#if UNITY_EDITOR
// #if false
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

            List<EventHandler<T>> handlers = null;
            if (!m_EventHandlers.TryGetValue(id, out handlers))
            {
                return false;
            }

            if (handlers == null || handlers.Count == 0)
            {
                return false;
            }

            //foreach (EventHandler<T> i in handlers.GetInvocationList())
            //{
            //    if (i == handler)
            //    {
            //        return true;
            //    }
            //}

            foreach (EventHandler<T> i in handlers)
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
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

            List<EventHandler<T>> eventHandler = null;
            if (!m_EventHandlers.TryGetValue(id, out eventHandler) || eventHandler == null)
            {
                //m_EventHandlers[id] = handler;
                var li = m_Pool.Get();
                li.Clear();
                li.Add(handler);
                m_EventHandlers[id] = li;
            }
            else if ((m_EventPoolMode & EventPoolMode.AllowMultiHandler) == 0)
            {
                Log.Error("Event '{0} {1}' not allow multi handler.", id.ToString(), (EventId)id);
                return;
            }
            else if ((m_EventPoolMode & EventPoolMode.AllowDuplicateHandler) == 0 && Check(id, handler))
            {
                Log.Error("Event '{0} {1}' not allow duplicate handler.", id.ToString(), (EventId)id);
                return;
            }
            else
            {
                //eventHandler += handler;
                eventHandler.Add(handler);
                //m_EventHandlers[id] = eventHandler;
            }
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

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

//                    m_EventHandlers.Remove(id);
                    m_EventHandlers[id] = null;
                }
            }
        }

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler(EventHandler<T> handler)
        {
            m_DefaultHandler = handler;
        }

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void FireAsync(object sender, T e)
        {
            Event eventNode = new Event(sender, e);
            lock (m_Events)
            {
                m_Events.Enqueue(eventNode);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void Fire(object sender, T e)
        {
            HandleEvent(sender, e);
        }

        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void HandleEvent(object sender, T e)
        {
            if (e == null)
            {
                //Debug.LogError("HandleEvent Error!!! e is null!");
                return;
            }
            
            int eventId = e.Id;
            if (m_EventHandlers.TryGetValue(eventId, out var handlers) && handlers != null)
            {
                //Note: 这里改为一个局部的list 因为在handlerEvent过程中 极有可能会fire其他事件，然后再次调用HandleEvent 相当于递归调用 此时全局list就会被清除 
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
                        current[i](sender, e);
                    }
                    catch(Exception exception)
                    {
                        Log.Error("EventHandler process exception!!! exception:{0}", exception.ToString());
                    }
                }
                
                m_Pool.Return(current);
            }

            if (handlers == null && m_DefaultHandler != null)
            {
                //handlers = m_DefaultHandler;
                //handlers(sender, e);
                m_DefaultHandler(sender, e);
            }

            //为了保证C#的通知lua层也能接收到，所以这里调用下通知lua层的监听
            if(e is CommonEventArgs args)
            {
                LuaManager.Instance.FireToLuaEvent((int)e.Id, args.UserData);
            }
            
            ReferencePool.Release(e.GetType(), e);
            if (handlers == null && (m_EventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                throw new GameFrameworkException(string.Format("Event '{0}' not allow no handler.", eventId.ToString()));
            }
        }
    }
}
