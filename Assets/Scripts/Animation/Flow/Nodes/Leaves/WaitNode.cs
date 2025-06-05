using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Leaves
{
    /// <summary>
    ///     Waits for a specified amount of time before returning success
    /// </summary>
    [CreateAssetMenu(fileName = "New Wait", menuName = "Animation/Flow/Nodes/Leaves/Wait")]
    public class WaitNode : FlowNode
    {
        [SerializeField] private float _waitTime = 1f;

        private float _startTime;
        private bool _isWaiting;

        /// <summary>
        ///     Time to wait in seconds
        /// </summary>
        public float WaitTime
        {
            get => _waitTime;
            set => _waitTime = Mathf.Max(0f, value);
        }

        /// <summary>
        ///     Waits for the specified time and returns success
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            // Start waiting if we're not already
            if (!_isWaiting)
            {
                _startTime = Time.time;
                _isWaiting = true;
            }

            // Check if we've waited long enough
            if (Time.time - _startTime >= _waitTime)
            {
                _isWaiting = false; // Reset for next time
                return NodeStatus.Success;
            }

            // Still waiting
            return NodeStatus.Running;
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _isWaiting = false;
        }
    }
}
