using System.Collections.Generic;
using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Composites
{
    /// <summary>
    ///     Selector node that executes children until one succeeds
    ///     Returns success if any child succeeds, otherwise returns failure
    /// </summary>
    [CreateAssetMenu(fileName = "New Selector", menuName = "Animation/Flow/Nodes/Composites/Selector")]
    public class SelectorNode : CompositeNode
    {
        /// <summary>
        ///     Executes children until one succeeds
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var status = child.Execute(context);

                // If the child succeeds or is running, return that status
                if (status == NodeStatus.Success || status == NodeStatus.Running)
                {
                    return status;
                }
            }

            // If all children failed, return failure
            return NodeStatus.Failure;
        }
    }
}
