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

        // Reference to the editor panel
        private TransitionEditorPanel _editorPanel;

        // Singleton access
        public static EdgeInspector Instance => _instance ??= new EdgeInspector();

        // Private properties
        private AnimationFlowEdge CurrentEdge { get; set; }

        private List<ConditionData> Conditions { get; set; }

        private string CurrentEdgeId { get; set; }

        // This method sets up the inspector for a specific edge
        public static void InspectEdge(AnimationFlowEdge edge)
        {
            Instance.BeginInspectEdge(edge);
        }

        public static void UnSelectEdge()
        {
            Instance.EndInspectEdge();
        }

        private void EndInspectEdge()
        {
            // Only hide the panel if it's not being interacted with
            if (_editorPanel?.IsBeingInteracted() == false)
            {
                _editorPanel.Hide();
            }
        }

        // Begin inspecting an edge
        private void BeginInspectEdge(AnimationFlowEdge edge)
        {
            CurrentEdge = edge;

            if (CurrentEdge == null)
                return;

            // Get the edge ID and conditions
            CurrentEdgeId = EdgeConditionManager.GetEdgeId(CurrentEdge);

            Conditions = !string.IsNullOrEmpty(CurrentEdgeId)
                ? EdgeConditionManager.Instance.GetConditions(CurrentEdgeId)
                : new List<ConditionData>();

            // Use the panel directly if it exists in the AnimationFlowGraphView
            _editorPanel?.Show(edge);
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

            if (CurrentEdge?.output?.node is not AnimationStateNode sourceNode ||
                CurrentEdge.input?.node is not AnimationStateNode targetNode)
                return false;

            sourceName = sourceNode.AnimationName;
            targetName = targetNode.AnimationName;
            return true;

        }
    }
}
