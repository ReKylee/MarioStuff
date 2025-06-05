using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Decorators
{
    /// <summary>
    ///     Inverts the result of its child node
    ///     Success becomes failure and failure becomes success
    /// </summary>
    [CreateAssetMenu(fileName = "New Inverter", menuName = "Animation/Flow/Nodes/Decorators/Inverter")]
    public class InverterNode : DecoratorNode
    {
        /// <summary>
        ///     Executes the child node and inverts its result
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            if (_child == null)
            {
                return NodeStatus.Failure;
            }

            var status = _child.Execute(context);

            switch (status)
            {
                case NodeStatus.Success:
                    return NodeStatus.Failure;
                case NodeStatus.Failure:
                    return NodeStatus.Success;
                default:
                    return status; // Running stays running
            }
        }
    }
}
