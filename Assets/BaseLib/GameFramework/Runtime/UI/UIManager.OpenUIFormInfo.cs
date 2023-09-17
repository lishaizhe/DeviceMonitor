//

using System;

namespace GameFramework.UI
{
    internal partial class UIManager
    {
        private sealed class OpenUIFormInfo
        {
            private readonly int m_SerialId;
            private readonly string m_UIKey;
            private readonly UIGroup m_UIGroup;
            private readonly bool m_PauseCoveredUIForm;
            private readonly object m_UserData;
            private readonly Action<IUIForm> m_onLoadFromSuccessAction;
            private readonly object[] m_backArgs;

            public OpenUIFormInfo(int serialId, string uiKey, UIGroup uiGroup, bool pauseCoveredUIForm, object userData, Action<IUIForm> onLoadFromSuccessAction, params object[] args )
            {
                m_SerialId = serialId;
                m_UIKey = uiKey;
                m_UIGroup = uiGroup;
                m_PauseCoveredUIForm = pauseCoveredUIForm;
                m_UserData = userData;
                m_onLoadFromSuccessAction = onLoadFromSuccessAction;
                m_backArgs = args;
            }

            public int SerialId
            {
                get
                {
                    return m_SerialId;
                }
            }

            public string UIKey
            {
                get
                {
                    return m_UIKey;
                }
            }

            public UIGroup UIGroup
            {
                get
                {
                    return m_UIGroup;
                }
            }

            public bool PauseCoveredUIForm
            {
                get
                {
                    return m_PauseCoveredUIForm;
                }
            }

            public object UserData
            {
                get
                {
                    return m_UserData;
                }
            }

            public object[] BackArgs
            {
                get
                {
                    return m_backArgs;
                }
            }


            public Action<IUIForm> OnLoadFromSuccessAction
            {
                get
                {
                    return m_onLoadFromSuccessAction;
                }
            }

        }
    }
}
