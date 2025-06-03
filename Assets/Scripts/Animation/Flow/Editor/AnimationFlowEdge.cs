using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class AnimationFlowEdge : Edge
    {
        public AnimationFlowEdge()
        {
            // Style the edge for better visual appearance
            AddToClassList("flow-edge");

            // Make the edge capable of being selected/dragged
            capabilities |= Capabilities.Selectable | Capabilities.Deletable;

            // Make sure the edge is visible with proper styling
            edgeControl.style.minWidth = 2;

            // Use the built-in edge styling with end cap
            edgeControl.drawToCap = true;

            // Add direct click handling to improve reliability
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public List<ConditionData> Conditions { get; set; } = new();

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Only process left-clicks
            if (evt.button != 0)
                return;

            // Force selection of this edge
            Select(parent, false);

            // Stop propagation to prevent double-handling
            evt.StopPropagation();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            EdgeInspector.UnSelectEdge();

        }
        public override void OnSelected()
        {
            base.OnSelected();

            // Only proceed if this is a valid edge between animation states
            if (output?.node is AnimationStateNode && input?.node is AnimationStateNode)
            {
                // Get or create edge ID
                string edgeId = EdgeConditionManager.GetEdgeId(this);
                if (!string.IsNullOrEmpty(edgeId))
                {
                    // Ensure conditions list exists
                    var conditions = EdgeConditionManager.Instance.GetConditions(edgeId);
                    if (conditions == null)
                    {
                        conditions = new List<ConditionData>();
                        EdgeConditionManager.Instance.SetConditions(edgeId, conditions);
                    }

                    // Update our local conditions reference - this addresses the warning
                    Conditions = conditions;
                }

                // Show the transition editor window
                EdgeInspector.InspectEdge(this);
            }
        }
    }
}
