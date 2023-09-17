//

using GameFramework.Event;
using GameFramework.UI;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 关闭界面完成事件。
    /// </summary>
    public sealed class CloseUIFormCompleteEventArgs : GameEventArgs
    {
        /// <summary>
        /// 关闭界面完成事件编号。
        /// </summary>
        public static readonly int EventId = typeof(CloseUIFormCompleteEventArgs).GetHashCode();

        /// <summary>
        /// 获取关闭界面完成事件编号。
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
        /// 获取界面所属的界面组。
        /// </summary>
        public IUIGroup UIGroup
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
        /// 清理关闭界面完成事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = default(int);
            UIFormAssetName = default(string);
            UIGroup = default(IUIGroup);
            UserData = default(object);
        }

    }
}
