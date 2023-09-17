//

using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_UICamera = null;
        private SerializedProperty m_InstanceRoot = null;
        private SerializedProperty m_SceneBackground = null;
        private SerializedProperty m_ScreenCapture = null;
        private SerializedProperty m_GroupRoot = null;
        private SerializedProperty m_IphoneXCover = null;
        private SerializedProperty m_TestIphoneX = null;
        private SerializedProperty m_UIGroups = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

            EditorGUILayout.PropertyField(m_UICamera);
            EditorGUILayout.PropertyField(m_InstanceRoot);
            EditorGUILayout.PropertyField(m_SceneBackground);
            EditorGUILayout.PropertyField(m_ScreenCapture);
            EditorGUILayout.PropertyField(m_GroupRoot);
            EditorGUILayout.PropertyField(m_IphoneXCover);
            
            //iphoneX适配测试开关
            if (EditorApplication.isPlaying && PrefabUtility.GetPrefabType(t.gameObject) != PrefabType.Prefab)
            {
                var testIphoneX = EditorGUILayout.Toggle("Test IphoneX", t.TestIphoneX);
                if (testIphoneX != t.TestIphoneX)
                {
                    t.TestIphoneX = testIphoneX;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_TestIphoneX);
            }
            
            EditorGUILayout.PropertyField(m_UIGroups, true);
            
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_UICamera = serializedObject.FindProperty("m_UICamera");
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_SceneBackground = serializedObject.FindProperty("m_SceneBackground");
            m_ScreenCapture = serializedObject.FindProperty("m_ScreenCapture");
            m_GroupRoot = serializedObject.FindProperty("m_GroupRoot");
            m_IphoneXCover = serializedObject.FindProperty("m_IphoneXCover");
            m_TestIphoneX = serializedObject.FindProperty("m_TestIphoneX");
            m_UIGroups = serializedObject.FindProperty("m_UIGroups");
            
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
