using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Animation.Flow.Editor
{
    [CustomEditor(typeof(AnimationFlowAsset))]
    public class AnimationFlowAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AnimationFlowAsset flowAsset = (AnimationFlowAsset)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Open in Animation Flow Editor", GUILayout.Height(30)))
            {
                OpenInFlowEditor(flowAsset);
            }
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is AnimationFlowAsset flowAsset)
            {
                OpenInFlowEditor(flowAsset);
                return true;
            }

            return false;
        }

        private static void OpenInFlowEditor(AnimationFlowAsset flowAsset)
        {
            // Open as a tab in the center area (typically where Scene view is)
            AnimationFlowEditorWindow window =
                EditorWindow.GetWindow<AnimationFlowEditorWindow>(
                    "Animation Flow Editor",
                    false,
                    typeof(SceneView));

            window.LoadAsset(flowAsset);
            window.Focus();
        }
    }
}
