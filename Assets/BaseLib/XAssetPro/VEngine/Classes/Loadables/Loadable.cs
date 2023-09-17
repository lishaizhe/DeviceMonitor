using System;
using System.Collections.Generic;

namespace VEngine
{
    /// <summary>
    ///     可加载类，自带缓存，并提供了基于引用计数的内存管理机制，目前实现的一级子类主要有 Asset, Bundle。
    ///     此类对象在 Release 的时候，会检查对象是否有使用，如果没有使用就会添加到 Unused 的列表，然后在 Updater 中集中卸载。
    ///     集中卸载时，如果发现对象又被使用了，则只会从 Unused 列表删除对象不会发生真正的卸载操作，否则就会触发真正的卸载了。
    /// </summary>
    public class Loadable
    {
        /// <summary>
        ///     加载中的列表，会在 Updater 中集中更新状态
        /// </summary>
        protected internal static readonly List<Loadable> Loading = new List<Loadable>();


        /// <summary>
        ///     引用计数
        /// </summary>
        protected readonly Reference reference = new Reference();

        /// <summary>
        ///     加载对象的状态，对象加载完成后，最好先检查状态判断是否正常加载。
        /// </summary>
        public LoadableStatus status { get; protected set; } = LoadableStatus.Wait;

        /// <summary>
        ///     加载路径
        /// </summary>
        public string pathOrURL { get; set; }

        /// <summary>
        ///     是否是同步加载
        /// </summary>
        protected bool mustCompleteOnNextFrame { get; set; }

        /// <summary>
        ///     如果加载出错可以通过这个属性获取错误信息
        /// </summary>
        public string error { get; internal set; }

        public bool isError
        {
            get { return !string.IsNullOrEmpty(error); }
        }


        /// <summary>
        ///     是否完成了加载
        /// </summary>
        public bool isDone =>
            status == LoadableStatus.SuccessToLoad ||
            status == LoadableStatus.Unloaded ||
            status == LoadableStatus.FailedToLoad;

        protected internal bool keepAliveOnLoad { get; set; }

        /// <summary>
        ///     加载进度
        /// </summary>
        public float progress { get; protected set; }


        protected void Finish(string errorCode = null)
        {
            error = errorCode;
            status = string.IsNullOrEmpty(errorCode) ? LoadableStatus.SuccessToLoad : LoadableStatus.FailedToLoad;
            progress = 1;
        }


        public static void UpdateLoadables()
        {
            for (var index = 0; index < Loading.Count; index++)
            {
                var item = Loading[index];
                if (Updater.busy)
                {
                    break;
                }

                item.Update();
                if (!item.isDone)
                {
                    continue;
                }

                Loading.RemoveAt(index);
                index--;
                item.Complete();
            }

            Asset.UpdateAssets();
            // Scene.UpdateScenes();
            // SceneObject.UpdateObjects();
            Bundle.UpdateBundles();
            // RawFile.UpdateFiles();
            ManifestFile.UpdateFiles();
        }

        internal static void Add(Loadable loadable)
        {
            Loading.Add(loadable);
        }

        internal void Update()
        {
            OnUpdate();
        }

        internal void Complete()
        {
            if (status == LoadableStatus.FailedToLoad)
            {
                Logger.E("Unable to load {0} {1} with error: {2}", GetType().Name, pathOrURL, error);
                Release();
            }

            OnComplete();
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnUnload()
        {
        }

        protected virtual void OnComplete()
        {
        }

        public virtual void LoadImmediate()
        {
            throw new InvalidOperationException();
        }

        protected internal void Load()
        {
            reference.Retain();
            Add(this);
            
            Logger.I("Load {0} {1} status:{2} id:{3} {4}.", GetType().Name, reference.count, status, GetHashCode(), pathOrURL);

            if (status != LoadableStatus.Wait)
            {
                return;
            }

            status = LoadableStatus.Loading;
            progress = 0;
            OnLoad();
            Logger.I("Real Load {0} {1} status:{2} id:{3} {4}.", GetType().Name, reference.count, status, GetHashCode(), pathOrURL);
        }

        protected internal bool Unload()
        {
            if (status == LoadableStatus.Unloaded)
            {
                return false;
            }

            Logger.I("Unload {0} status:{1} id:{2} {3}.", GetType().Name, status, GetHashCode(), pathOrURL);
            OnUnload();
            status = LoadableStatus.Unloaded;
            return true;
        }

        public int referenceCount => reference.count;

        public void Release()
        {
            if (reference.count <= 0)
            {
                Logger.W("Release {0} {1} status:{2} id:{3} {4}. reference.count <= 0", GetType().Name, reference.count, status, GetHashCode(), pathOrURL);
                return;
            }

            if (keepAliveOnLoad)
            {
                return;
            }

            reference.Release();
            Logger.I("Release {0} {1} status:{2} id:{3} {4}.", GetType().Name, reference.count, status, GetHashCode(), pathOrURL);

            if (!reference.unused)
            {
                return;
            }

            OnUnused();
        }

        protected virtual void OnUnused()
        {
        }
    }
}