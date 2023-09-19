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
        private SerializedProperty m_GroupRoot = null;
        private SerializedProperty m_UIGroups = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

            EditorGUILayout.PropertyField(m_UICamera);
            EditorGUILayout.PropertyField(m_InstanceRoot);
            EditorGUILayout.PropertyField(m_GroupRoot);
            
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
            m_GroupRoot = serializedObject.FindProperty("m_GroupRoot");
            m_UIGroups = serializedObject.FindProperty("m_UIGroups");
            
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
