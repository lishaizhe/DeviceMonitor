//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2018 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using GameFramework.Debugger;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private abstract class ScrollableDebuggerWindowBase : IDebuggerWindow
        {
            private const float TitleWidth = 360f;
            private Vector2 m_ScrollPosition = Vector2.zero;
            private GUIStyle m_fontStyle = new GUIStyle();

            public virtual void Initialize(params object[] args)
            {
                m_fontStyle.fontSize = 18;
                m_fontStyle.normal.textColor = Color.white;
                m_fontStyle.fixedHeight = 22;
            }

            public virtual void Shutdown()
            {

            }

            public virtual void OnEnter()
            {

            }

            public virtual void OnLeave()
            {

            }

            public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {

            }

            public void OnDraw()
            {
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    OnDrawScrollableWindow();
                }
                GUILayout.EndScrollView();
            }

            protected abstract void OnDrawScrollableWindow();

            protected void DrawItem(string title, string content)
            {
                GUILayout.BeginHorizontal();
                {
                    m_fontStyle.fixedWidth = TitleWidth;
                    GUILayout.Label(title, m_fontStyle);

                    m_fontStyle.fixedWidth = 0;
                    GUILayout.Label(content, m_fontStyle);
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
