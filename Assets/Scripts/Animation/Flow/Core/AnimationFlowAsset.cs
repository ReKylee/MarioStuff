using System.Collections.Generic;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Scriptable object that defines an animation flow configuration
    ///     Acts as a data container for behavior tree nodes that control animation transitions
    /// </summary>
    [CreateAssetMenu(fileName = "New Animation Flow", menuName = "Animation/Flow/Animation Flow Asset")]
    public class AnimationFlowAsset : ScriptableObject
    {
        [SerializeField] private string _description;
        [SerializeField] private FlowNode _rootNode;
        [SerializeField] private List<FlowNode> _nodes = new();

        /// <summary>
        ///     Description of this animation flow
        /// </summary>
        public string Description => _description;

        /// <summary>
        ///     Root node of the behavior tree
        /// </summary>
        public FlowNode RootNode => _rootNode;

        /// <summary>
        ///     All nodes in this behavior tree
        /// </summary>
        public List<FlowNode> Nodes => _nodes;

        /// <summary>
        ///     Sets the root node of this behavior tree
        /// </summary>
        public void SetRootNode(FlowNode rootNode)
        {
            _rootNode = rootNode;

            // Make sure the root node is in the list
            if (!_nodes.Contains(rootNode))
            {
                _nodes.Add(rootNode);
            }
        }

        /// <summary>
        ///     Adds a node to this behavior tree
        /// </summary>
        public void AddNode(FlowNode node)
        {
            // Avoid duplicates
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
            }
        }

        /// <summary>
        ///     Removes a node from this behavior tree
        /// </summary>
        public void RemoveNode(FlowNode node)
        {
            _nodes.Remove(node);

            // If we removed the root node, clear it
            if (_rootNode == node)
            {
                _rootNode = null;
            }
        }

        /// <summary>
        ///     Clears all nodes from this behavior tree
        /// </summary>
        public void ClearNodes()
        {
            _nodes.Clear();
            _rootNode = null;
        }
    }
}
