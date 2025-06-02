using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    // Class to handle edge selection in the graph view
    public class EdgeSelectionHandler
    {
        private readonly GraphView _graphView;

        public EdgeSelectionHandler(GraphView graphView)
        {
            _graphView = graphView;

            // Register for selection changes
            _graphView.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                // Check if the selection contains exactly one edge
                if (_graphView.selection.Count == 1 && _graphView.selection[0] is Edge edge)
                {
                    // Open the transition editor for this edge
                    StandardTransitionEditorWindow.ShowWindow(edge);

                    // Mark the event as handled
                    evt.StopPropagation();
                }
            }
        }

        public void Dispose()
        {
            // Unregister callback when we're done
            _graphView.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
    }
}
