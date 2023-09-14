using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameFramework;
using UnityEngine;

namespace GameKit.Base
{
        public enum TimerType
        {
            FIXED_DURATION,
            FIXED_REALTIME_DURATION,
            EVERY_FRAME,
        }

        public class TimerTask
        {
            public TimerType timerType;
            public float startTime;
            public float interval;
            public int repeatTimes;
            public Action executable;

            private int executeTimes;
            private float deltaTime;
            private bool started;
            //是否取消定时器
            private bool isCancelled = false;

            public bool IsCancelled => isCancelled;
            
            // after the auto destroy owner is destroyed, the timer will expire
            // this way you don't run into any annoying bugs with timers running and accessing objects
            // after they have been destroyed
            private MonoBehaviour _autoDestroyOwner;
            private bool _hasAutoDestroyOwner;

            public TimerTask(TimerType timerType,float startTime,float interval,int repeatTimes,Action executable)
            {
                this.timerType = timerType;
                this.startTime = startTime;
                this.interval = interval;
                this.repeatTimes = repeatTimes;
                this.executable = executable;
                
                this._autoDestroyOwner = null;
                this._hasAutoDestroyOwner = false;
            }
     
            private bool IsOwnerDestroyed
            {
                get { return this._hasAutoDestroyOwner && this._autoDestroyOwner == null; }
            }
            //绑定MonoBehaviour 对象， 如果对象销毁了，定时器会被自动移除
            public TimerTask SetOwner(MonoBehaviour autoDestroyOwner)
            {
                this._autoDestroyOwner = autoDestroyOwner;
                this._hasAutoDestroyOwner = autoDestroyOwner != null;

                return this;
            }
            
            /// <summary>
            /// Get whether or not the timer has finished running for any reason.
            /// </summary>
            public bool IsDone
            {
                get { return this.isCancelled || this.IsOwnerDestroyed; }
            }

            public bool Update(float delta)
            {
                if (timerType == TimerType.FIXED_DURATION)
                    deltaTime += delta;

                if (!started)
                {
                    if (timerType == TimerType.FIXED_REALTIME_DURATION)
                    {
                        if (Time.realtimeSinceStartup >= startTime)
                        {
                            started = true;
                            deltaTime = startTime + interval;
                            return true;
                        }
                    }
                    else
                    {
                        if (deltaTime >= startTime)
                        {
                            started = true;
                            deltaTime -= startTime;
                            return true;
                        }
                    }
                }
                else
                {
                    if (timerType == TimerType.EVERY_FRAME)
                    {
                        deltaTime = 0;
                        return true;
                    }

                    if (timerType == TimerType.FIXED_REALTIME_DURATION)
                    {
                        if (Time.realtimeSinceStartup >= deltaTime)
                        {
                            deltaTime += interval;
                            return true;
                        }
                    }
                    else
                    {
                        if (deltaTime >= interval)
                        {
                            deltaTime -= interval;
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Execute this instance.
            /// </summary>
            /// <returns>if <see langword="true"/>, remove this task</returns>
            public bool Execute()
            {
                //如果对象销毁了直接返回 并会删除定时器
                if (IsOwnerDestroyed)
                    return true;
                    
                if(repeatTimes > 0)
                    executeTimes++;
                if (executable != null)
                {
                    try
                    {
                        executable();
                    }
                    catch (Exception e)
                    {
                        GameFramework.Log.Error($"TimerTask Exception:{e.Message} \n StackTrace:{e.StackTrace}");
                    }
                    
                    return executeTimes == repeatTimes;
                }

                return true;
            }
            
            /// <summary>
            /// Stop a timer that is in-progress or paused. The timer's on executable callback will not be called.
            /// </summary>
            public void Cancel()
            {
                isCancelled = true;
            }

            public string GetActionName()
            {
                if (executable != null)
                    return executable.GetMethodInfo().ToString();
                return "";
            }


        }
        
        
    public class TimerManager : Singleton<TimerManager>
    {
        private int MAX_TASK_COUNT = 500;// 这个值是预估的
        private readonly List<TimerTask> taskList = new List<TimerTask>();
        private readonly List<TimerTask> toBeRemoved = new List<TimerTask>();

        public void Release()
        {
            lock (taskList)
            {
                toBeRemoved.Clear();
                taskList.Clear();
            }
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);

            lock (taskList)
            {
                for (int i = 0; i < taskList.Count; ++i)
                {
                    TimerTask task = taskList[i];
                    if (task.IsCancelled || (task.Update(delta) && task.Execute()))
                    {
                        toBeRemoved.Add(task);
                    }
                }
                
                for (int i = 0; i < toBeRemoved.Count; ++i)
                {
                    taskList.Remove(toBeRemoved[i]);
                }
            }

            toBeRemoved.Clear();
        }

        /// <summary>
        /// Adds the one shot task.
        /// </summary>
        /// <param name="startTime">Start delay.</param>
        /// <param name="executable">Executable.</param>
        public TimerTask AddOneShotTask(float startTime, Action executable)
        {
            return AddTask(TimerType.FIXED_DURATION, startTime, 0, 1, executable);
        }

        /// <summary>
        /// Adds the realtime one shot task.
        /// </summary>
        /// <param name="startTime">Start delay.</param>
        /// <param name="executable">Executable.</param>
        public TimerTask AddRealtimeOneShotTask(float startTime, Action executable)
        {
            return AddTask(TimerType.FIXED_REALTIME_DURATION, Time.realtimeSinceStartup + startTime, 0, 1, executable);
        }

        /// <summary>
        /// Adds the repeat task.
        /// </summary>
        /// <param name="startTime">Start delay.</param>
        /// <param name="interval">Interval.</param>
        /// <param name="repeatTimes">Repeat times, -1 means always.</param>
        /// <param name="executable">Executable.</param>
        public TimerTask AddRepeatTask(float startTime, float interval, int repeatTimes, Action executable)
        {
            return AddTask(TimerType.FIXED_DURATION, startTime, interval, repeatTimes, executable);
        }

        /// <summary>
        /// Adds the realtime repeat task.
        /// </summary>
        /// <param name="startTime">Start delay.</param>
        /// <param name="interval">Interval.</param>
        /// <param name="repeatTimes">Repeat times, -1 means always.</param>
        /// <param name="executable">Executable.</param>
        public TimerTask AddRealtimeRepeatTask(float startTime, float interval, int repeatTimes, Action executable)
        {
            return AddTask(TimerType.FIXED_REALTIME_DURATION, Time.realtimeSinceStartup + startTime, interval, repeatTimes, executable);
        }

        /// <summary>
        /// Adds the frame execute task.
        /// </summary>
        /// <param name="executable">Executable.</param>
        public TimerTask AddFrameExecuteTask(Action executable)
        {
            return AddTask(TimerType.EVERY_FRAME, 0, 0, -1, executable);
        }

        public void RemoveTimerTask(TimerTask timerTask)
        {
            timerTask?.Cancel();
        }

        public bool HasTimerTask(TimerTask timerTask)
        {
            if (timerTask == null)
                return false;
            lock (taskList)
            {
                return taskList.Contains(timerTask);
            }
        }

        private TimerTask AddTask(TimerType timerType, float startTime, float interval, int repeatTimes,Action executable)
        {
            if (executable == null)
                return null;
            TimerTask timerTask = new TimerTask(timerType,startTime,interval,repeatTimes,executable);
            lock (taskList)
            {
                taskList.Add(timerTask);
            }

            CheckTaskException();
            return timerTask;
        }

        private bool haveCheckedException = false;
        private void CheckTaskException()
        {
            if (haveCheckedException)
                return;
            lock (taskList)
            {
                if (taskList.Count <= MAX_TASK_COUNT) return;
                haveCheckedException = true;
                var actionCountDic = new Dictionary<string, int>(MAX_TASK_COUNT / 2);
                foreach (var timerTask in taskList)
                {
                    var actionName = timerTask.GetActionName();
                    if (actionCountDic.ContainsKey(actionName))
                    {
                        actionCountDic[actionName]++;
                    }
                    else
                    {
                        actionCountDic.Add(timerTask.GetActionName(),1);
                    }
                }

                var result = actionCountDic.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, p => p.Value);
                Log.ReleaseError($"TimerTask count is more than {MAX_TASK_COUNT} ,total count = {taskList.Count}");
                foreach (var actionObj in result)
                {
                    if(actionObj.Value > 5)
                        Log.ReleaseError($"warning:TimerTask method name : {actionObj.Key}  count : {actionObj.Value}");
                }
            }
        }
        
    }
}
