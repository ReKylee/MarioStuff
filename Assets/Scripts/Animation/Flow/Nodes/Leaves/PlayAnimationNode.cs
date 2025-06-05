using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Leaves
{
    /// <summary>
    ///     Plays an animation and returns success when it completes
    ///     Can be set to wait for animation completion or return immediately
    /// </summary>
    [CreateAssetMenu(fileName = "New Play Animation", menuName = "Animation/Flow/Nodes/Leaves/Play Animation")]
    public class PlayAnimationNode : FlowNode
    {
        [SerializeField] private string _animationName;
        [SerializeField] private bool _waitForCompletion = true;

        /// <summary>
        ///     Name of the animation to play
        /// </summary>
        public string AnimationName
        {
            get => _animationName;
            set => _animationName = value;
        }

        /// <summary>
        ///     Whether to wait for the animation to complete before returning success
        /// </summary>
        public bool WaitForCompletion
        {
            get => _waitForCompletion;
            set => _waitForCompletion = value;
        }

        /// <summary>
        ///     Plays the animation and returns the appropriate status
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            if (string.IsNullOrEmpty(_animationName) || context.Animator == null)
            {
                return NodeStatus.Failure;
            }

            // Try to play the animation
            bool played = context.Animator.PlayAnimation(_animationName);
            if (!played)
            {
                return NodeStatus.Failure;
            }

            // If we're not waiting for completion, return success immediately
            if (!_waitForCompletion)
            {
                return NodeStatus.Success;
            }

            // If waiting, check if the animation has finished
            if (context.Animator.IsAnimationFinished())
            {
                return NodeStatus.Success;
            }

            // Still playing
            return NodeStatus.Running;
        }
    }
}
