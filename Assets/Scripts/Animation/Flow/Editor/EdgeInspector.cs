using System.Collections.Generic;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Manages the inspection and editing of edges in the Animation Flow graph
    ///     Acts as a bridge between the graph view and the transition editor panel
    /// </summary>
    public class EdgeInspector
    {
        // Static instance of the currently edited edge (follows singleton pattern for editor tools)
        private static EdgeInspector _instance;

        // Cache of conditions for the current edge

        // Current edge being inspected

        // Edge ID for the current edge

        // Reference to the editor panel
        private TransitionEditorPanel _editorPanel;

        // Singleton access
        public static EdgeInspector Instance => _instance ??= new EdgeInspector();

        // Public properties
        public AnimationFlowEdge CurrentEdge { get; private set; }

        public List<ConditionData> Conditions { get; private set; }

        public string CurrentEdgeId { get; private set; }

        // This method sets up the inspector for a specific edge
        public static void InspectEdge(AnimationFlowEdge edge)
        {
            Instance.BeginInspectEdge(edge);
        }

        // Begin inspecting an edge
        private void BeginInspectEdge(AnimationFlowEdge edge)
        {
            CurrentEdge = edge;

            if (CurrentEdge == null)
                return;

            // Get the edge ID and conditions
            CurrentEdgeId = EdgeConditionManager.Instance.GetEdgeId(CurrentEdge);
            if (!string.IsNullOrEmpty(CurrentEdgeId))
            {
                Conditions = EdgeConditionManager.Instance.GetConditions(CurrentEdgeId);
            }
            else
            {
                Conditions = new List<ConditionData>();
            }

            // Use the panel directly if it exists in the AnimationFlowGraphView
            if (_editorPanel != null)
            {
                _editorPanel.Toggle(edge);
            }
        }

        // Save conditions for the current edge
        public void SaveConditions(List<ConditionData> conditions)
        {
            if (CurrentEdge == null || string.IsNullOrEmpty(CurrentEdgeId))
                return;

            Conditions = conditions;
            EdgeConditionManager.Instance.SetConditions(CurrentEdgeId, Conditions);
        }

        // Set reference to the editor panel
        public void SetEditorPanel(TransitionEditorPanel panel)
        {
            _editorPanel = panel;
        }

        // Get source and target node names for the current edge
        public bool GetEdgeNodeNames(out string sourceName, out string targetName)
        {
            sourceName = "Unknown";
            targetName = "Unknown";

            if (CurrentEdge == null)
                return false;

            AnimationStateNode sourceNode = CurrentEdge.output?.node as AnimationStateNode;
            AnimationStateNode targetNode = CurrentEdge.input?.node as AnimationStateNode;

            if (sourceNode != null && targetNode != null)
            {
                sourceName = sourceNode.AnimationName;
                targetName = targetNode.AnimationName;
                return true;
            }

            return false;
        }
    }
}
