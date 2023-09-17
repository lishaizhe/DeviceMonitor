//

using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 打开界面失败事件。
    /// </summary>
    public sealed class OpenUIFormFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 打开界面失败事件编号。
        /// </summary>
        public static readonly int EventId = typeof(OpenUIFormFailureEventArgs).GetHashCode();

        /// <summary>
        /// 获取打开界面失败事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            set;
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get;
            set;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName
        {
            get;
            set;
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
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
        /// 清理打开界面失败事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = default(int);
            UIFormAssetName = default(string);
            UIGroupName = default(string);
            PauseCoveredUIForm = default(bool);
            ErrorMessage = default(string);
            UserData = default(object);
        }

    }
}
