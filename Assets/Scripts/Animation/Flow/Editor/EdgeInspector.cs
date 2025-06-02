using System.Collections.Generic;

namespace Animation.Flow.Editor
{
    // Not using CustomEditor attribute since AnimationFlowEdge doesn't inherit from UnityEngine.Object
    public class EdgeInspector
    {
        private static AnimationFlowEdge _currentEdge;
        private static List<ConditionData> _conditions;
        private static string _currentEdgeId;

        // This method sets up the inspector for a specific edge
        public static void InspectEdge(AnimationFlowEdge edge)
        {
            _currentEdge = edge;

            if (_currentEdge != null)
            {
                // Get the edge ID and conditions
                _currentEdgeId = EdgeConditionManager.Instance.GetEdgeId(_currentEdge);
                if (!string.IsNullOrEmpty(_currentEdgeId))
                {
                    _conditions = EdgeConditionManager.Instance.GetConditions(_currentEdgeId);
                }
                else
                {
                    _conditions = new List<ConditionData>();
                }

                // Open the transition editor window for this edge
                TransitionEditorWindow.ShowWindow(edge);
            }
        }
    }
}
