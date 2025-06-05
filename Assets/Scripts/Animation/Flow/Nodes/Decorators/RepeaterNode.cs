using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Decorators
{
    /// <summary>
    ///     Repeats the execution of its child node a specified number of times
    ///     or indefinitely if repeatCount is -1
    /// </summary>
    [CreateAssetMenu(fileName = "New Repeater", menuName = "Animation/Flow/Nodes/Decorators/Repeater")]
    public class RepeaterNode : DecoratorNode
    {
        [SerializeField] private int _repeatCount = -1; // -1 means repeat indefinitely

        private int _currentRepeat = 0;

        /// <summary>
        ///     Number of times to repeat the child node
        ///     -1 means repeat indefinitely
        /// </summary>
        public int RepeatCount
        {
            get => _repeatCount;
            set => _repeatCount = value;
        }

        /// <summary>
        ///     Executes the child node and repeats according to the repeat count
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            if (_child == null)
            {
                return NodeStatus.Failure;
            }

            // If we have a repeat count and we've reached it, return success
            if (_repeatCount >= 0 && _currentRepeat >= _repeatCount)
            {
                _currentRepeat = 0; // Reset for next time
                return NodeStatus.Success;
            }

            // Execute the child
            var status = _child.Execute(context);

            // If the child is still running, we're still running
            if (status == NodeStatus.Running)
            {
                return NodeStatus.Running;
            }

            // Child has completed one execution (success or failure)
            // For repeater, we treat both success and failure as completion
            _currentRepeat++;

            // If we have a repeat count and we've reached it, return success
            if (_repeatCount >= 0 && _currentRepeat >= _repeatCount)
            {
                _currentRepeat = 0; // Reset for next time
                return NodeStatus.Success;
            }

            // Otherwise, we're still running
            return NodeStatus.Running;
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _currentRepeat = 0;
        }
    }
}
