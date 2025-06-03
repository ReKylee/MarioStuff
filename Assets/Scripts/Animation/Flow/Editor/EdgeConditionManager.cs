using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor.Experimental.GraphView;

namespace Animation.Flow.Editor
{
    // Class to store conditions for edges in the editor
    // This allows us to associate conditions with standard Edge objects
    public class EdgeConditionManager
    {
        // Singleton instance
        private static EdgeConditionManager _instance;

        // Dictionary mapping edge IDs to their conditions
        private readonly Dictionary<string, List<ConditionData>> _edgeConditions = new();
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
        public List<ConditionData> GetConditions(string edgeId)
        {
            if (string.IsNullOrEmpty(edgeId))
                return new List<ConditionData>();

            if (!_edgeConditions.TryGetValue(edgeId, out var conditions))
            {
                conditions = new List<ConditionData>();
                _edgeConditions[edgeId] = conditions;
            }

            return conditions;
        }

        // Set conditions for an edge
        public void SetConditions(string edgeId, List<ConditionData> conditions)
        {
            if (string.IsNullOrEmpty(edgeId))
                return;

            _edgeConditions[edgeId] = conditions ?? new List<ConditionData>();
        }

        // Clear all conditions (used when loading a new graph)
        public void Clear()
        {
            _edgeConditions.Clear();
        }
    }
}
