//

using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 打开界面成功事件。
    /// </summary>
    public sealed class OpenUIFormSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 打开界面成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(OpenUIFormSuccessEventArgs).GetHashCode();

        /// <summary>
        /// 获取打开界面成功事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取打开成功的界面。
        /// </summary>
        public UIForm UIForm
        {
            get;
            set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            set;
        }

        /// <summary>
        /// 清理打开界面成功事件。
        /// </summary>
        public override void Clear()
        {
            UIForm = default(UIForm);
            UserData = default(object);
        }

    }
}
