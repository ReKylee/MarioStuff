#if UNITY_EDITOR
using Kirby.Abilities;
using UnityEditor;
using UnityEngine;

namespace Kirby.Editor
{
    [CustomEditor(typeof(AbilityModuleBase), true)]
    public class AbilityModuleBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _abilityDefinedModifiersProperty;
        private ReorderableModifierList _modifierList;

        private void OnEnable()
        {
            _abilityDefinedModifiersProperty = serializedObject.FindProperty("abilityDefinedModifiers");
            _modifierList = new ReorderableModifierList(serializedObject, _abilityDefinedModifiersProperty);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // Create a container for the base properties section with helpBox style
            GUIStyle sectionBoxStyle = new(EditorStyles.helpBox)
            {
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(10, 10, 10, 10)
            };

            // Set background color to ensure the helpBox is visible
            Color prevColor = GUI.backgroundColor;
            // Use a more contrasting color that will stand out in both light and dark themes
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.35f, 0.5f, 0.8f) // Blueish for dark theme
                : new Color(0.8f, 0.8f, 0.9f, 0.8f); // Light bluish for light theme

            EditorGUILayout.BeginVertical(sectionBoxStyle);

            // Reset background color
            GUI.backgroundColor = prevColor;

            // Create a nice header for the base properties
            Rect basePropsHeaderRect = EditorGUILayout.GetControlRect(false, 28);

            // Style for header text
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12
            };


            EditorGUI.LabelField(basePropsHeaderRect, "Ability Properties", headerStyle);

            EditorGUILayout.Space(8);

            // Draw all properties excluding the stat modifiers
            DrawPropertiesExcluding(serializedObject, "m_Script", "abilityDefinedModifiers");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Use a rounded container for stat modifiers section
            // Set background color again to ensure visibility with a more contrasting color
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.35f, 0.5f, 0.8f) // Blueish for dark theme
                : new Color(0.8f, 0.8f, 0.9f, 0.8f); // Light bluish for light theme

            EditorGUILayout.BeginVertical(sectionBoxStyle);

            // Reset background color
            GUI.backgroundColor = prevColor;

            // Create a nice header for the stat modifiers
            Rect headerRect = EditorGUILayout.GetControlRect(false, 28);


            EditorGUI.LabelField(headerRect, "Ability Stat Modifiers", headerStyle);

            EditorGUILayout.Space(8);

            // Use our modern reorderable list for stat modifiers
            _modifierList.DoLayoutList();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
