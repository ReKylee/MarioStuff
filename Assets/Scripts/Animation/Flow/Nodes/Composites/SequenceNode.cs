using System.Collections.Generic;
using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Composites
{
    /// <summary>
    ///     Sequence node that executes children in order until one fails
    ///     Returns success only if all children succeed, otherwise returns failure or running
    /// </summary>
    [CreateAssetMenu(fileName = "New Sequence", menuName = "Animation/Flow/Nodes/Composites/Sequence")]
    public class SequenceNode : CompositeNode
    {
        // Index of the currently running child
        private int _currentChildIndex = 0;

        /// <summary>
        ///     Executes children in order until one fails
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            // If no children, return success
            if (Children.Count == 0)
            {
                return NodeStatus.Success;
            }

            // Execute children in order
            for (int i = _currentChildIndex; i < Children.Count; i++)
            {
                var child = Children[i];
                var status = child.Execute(context);

                // If the child fails or is running, remember this index and return that status
                if (status == NodeStatus.Failure)
                {
                    _currentChildIndex = 0; // Reset for next time
                    return NodeStatus.Failure;
                }

                if (status == NodeStatus.Running)
                {
                    _currentChildIndex = i;
                    return NodeStatus.Running;
                }
            }

            // All children succeeded, reset index and return success
            _currentChildIndex = 0;
            return NodeStatus.Success;
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _currentChildIndex = 0;
        }
    }
}
