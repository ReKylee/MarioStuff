using System.Collections.Generic;
using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Composites
{
    /// <summary>
    ///     Parallel node that executes all children simultaneously
    ///     Success policy determines when the node returns success
    ///     Failure policy determines when the node returns failure
    /// </summary>
    [CreateAssetMenu(fileName = "New Parallel", menuName = "Animation/Flow/Nodes/Composites/Parallel")]
    public class ParallelNode : CompositeNode
    {
        public enum Policy
        {
            RequireOne, // Requires at least one child to succeed/fail
            RequireAll  // Requires all children to succeed/fail
        }

        [SerializeField] private Policy _successPolicy = Policy.RequireAll;
        [SerializeField] private Policy _failurePolicy = Policy.RequireOne;

        /// <summary>
        ///     Policy for determining when the node succeeds
        /// </summary>
        public Policy SuccessPolicy
        {
            get => _successPolicy;
            set => _successPolicy = value;
        }

        /// <summary>
        ///     Policy for determining when the node fails
        /// </summary>
        public Policy FailurePolicy
        {
            get => _failurePolicy;
            set => _failurePolicy = value;
        }

        /// <summary>
        ///     Executes all children simultaneously
        /// </summary>
        public override NodeStatus Execute(AnimationContext context)
        {
            int successCount = 0;
            int failureCount = 0;

            // Execute all children
            foreach (var child in Children)
            {
                var status = child.Execute(context);

                if (status == NodeStatus.Success)
                {
                    successCount++;

                    // If success policy is RequireOne and at least one child succeeded
                    if (_successPolicy == Policy.RequireOne)
                    {
                        return NodeStatus.Success;
                    }
                }
                else if (status == NodeStatus.Failure)
                {
                    failureCount++;

                    // If failure policy is RequireOne and at least one child failed
                    if (_failurePolicy == Policy.RequireOne)
                    {
                        return NodeStatus.Failure;
                    }
                }
            }

            // Check success policy
            if (_successPolicy == Policy.RequireAll && successCount == Children.Count)
            {
                return NodeStatus.Success;
            }

            // Check failure policy
            if (_failurePolicy == Policy.RequireAll && failureCount == Children.Count)
            {
                return NodeStatus.Failure;
            }

            // Otherwise, the node is still running
            return NodeStatus.Running;
        }
    }
}
