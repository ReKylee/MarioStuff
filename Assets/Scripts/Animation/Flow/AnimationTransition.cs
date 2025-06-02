using System;
using System.Collections.Generic;

namespace Animation.Flow
{
    /// <summary>
    /// Defines a transition between animation states
    /// </summary>
    public class AnimationTransition
    {
        /// <summary>
        /// ID of the destination state for this transition
        /// </summary>
        public string TargetStateId { get; private set; }
        
        /// <summary>
        /// Conditions that must be satisfied for this transition to be valid
        /// </summary>
        private readonly List<ITransitionCondition> _conditions = new List<ITransitionCondition>();
        
        /// <summary>
        /// Create a new transition to the specified target state
        /// </summary>
        public AnimationTransition(string targetStateId)
        {
            TargetStateId = targetStateId;
        }
        
        /// <summary>
        /// Add a condition to this transition
        /// </summary>
        public AnimationTransition AddCondition(ITransitionCondition condition)
        {
            _conditions.Add(condition);
            return this; // For method chaining
        }
        
        /// <summary>
        /// Check if all conditions for this transition are satisfied
        /// </summary>
        public bool CanTransition(IAnimationContext context)
        {
            // If no conditions, transition is always valid
            if (_conditions.Count == 0)
                return true;
                
            // All conditions must be satisfied
            foreach (var condition in _conditions)
            {
                if (!condition.IsSatisfied(context))
                    return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Interface for transition conditions
    /// </summary>
    public interface ITransitionCondition
    {
        /// <summary>
        /// Check if this condition is satisfied
        /// </summary>
        bool IsSatisfied(IAnimationContext context);
    }
}
