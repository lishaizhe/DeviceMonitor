//

using BaseLib.GameFramework.Runtime.UI;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// 界面组辅助器接口。
    /// </summary>
    public interface IUIGroupHelper
    {
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        void SetDepth(int depth);

        void SetSoringLayer (string layer);
    }
}
