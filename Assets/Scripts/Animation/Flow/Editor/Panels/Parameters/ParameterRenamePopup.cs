using System;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Editor.Panels.Parameters
{
    public class ParameterRenamePopup : PopupWindowContent
    {
        private const float Height = 70f;
        private const float Width = 250f;
        private readonly Func<string, bool> _onRename;
        private string _parameterName;

        public ParameterRenamePopup(string currentName, Func<string, bool> onRename)
        {
            _parameterName = currentName;
            _onRename = onRename;
        }

        public override Vector2 GetWindowSize() => new(Width, Height);

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Rename Parameter", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Parameter name field
            GUI.SetNextControlName("ParameterNameField");
            _parameterName = EditorGUILayout.TextField("Name", _parameterName);

            EditorGUILayout.Space(5);

            // Button row
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                editorWindow.Close();
            }

            if (GUILayout.Button("OK", GUILayout.Width(80)))
            {
                if (_onRename(_parameterName))
                {
                    editorWindow.Close();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Auto-focus the name field when opening
            if (Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("ParameterNameField");
            }
        }
    }
}
