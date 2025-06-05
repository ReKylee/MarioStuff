using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using UnityEditor.Experimental.GraphView;

namespace Animation.Flow.Editor.Managers
{
    // Class to store conditions for edges in the editor
    // This allows us to associate conditions with standard Edge objects
    public class EdgeConditionManager
    {
        // Singleton instance
        private static EdgeConditionManager _instance;

        // Dictionary mapping edge IDs to their conditions
        private readonly Dictionary<string, List<FlowCondition>> _edgeConditions = new();
        public static EdgeConditionManager Instance => _instance ??= new EdgeConditionManager();

        // Get a unique identifier for an edge based on its connected nodes
        public static string GetEdgeId(Edge edge)
        {
            if (edge?.output?.node is not AnimationStateNode sourceNode ||
                edge.input?.node is not AnimationStateNode targetNode)
                return null;

            return $"{sourceNode.ID}_{targetNode.ID}";
        }

        // Get conditions for an edge
        public List<FlowCondition> GetConditions(string edgeId)
        {
            if (string.IsNullOrEmpty(edgeId))
                return new List<FlowCondition>();

            if (!_edgeConditions.TryGetValue(edgeId, out var conditions))
            {
                conditions = new List<FlowCondition>();
                _edgeConditions[edgeId] = conditions;
            }

            return conditions;
        }

        // Set conditions for an edge
        public void SetConditions(string edgeId, List<FlowCondition> conditions)
        {
            if (string.IsNullOrEmpty(edgeId))
                return;

            // Create a deep copy of the conditions to avoid reference issues
            if (conditions != null)
            {
                var clonedConditions = new List<FlowCondition>();
                foreach (FlowCondition condition in conditions)
                {
                    if (condition != null)
                    {
                        clonedConditions.Add(condition.Clone());
                    }
                }

                _edgeConditions[edgeId] = clonedConditions;
            }
            else
            {
                _edgeConditions[edgeId] = new List<FlowCondition>();
            }
        }

        // Remove all conditions associated with an edge
        public void RemoveConditions(string edgeId)
        {
            if (!string.IsNullOrEmpty(edgeId) && _edgeConditions.ContainsKey(edgeId))
            {
                _edgeConditions.Remove(edgeId);
            }
        }

        // Clear all conditions (used when loading a new graph)
        public void Clear()
        {
            _edgeConditions.Clear();
        }
    }
}
