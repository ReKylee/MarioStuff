using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions.SpecialConditions
{
    /// <summary>
    ///     Condition that checks if the current animation has completed
    /// </summary>
    [Serializable]
    public class AnimationCompleteCondition : FlowCondition
    {
        public AnimationCompleteCondition()
            : base("Animation Complete")
        {
        }

        public AnimationCompleteCondition(bool isNegated = false)
            : base("Animation Complete", isNegated)
        {
        }

        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public override ConditionType ConditionType => ConditionType.AnimationComplete;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (context?.Animator == null)
                return false;

            return context.Animator.IsAnimationComplete;
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone() => new AnimationCompleteCondition(isNegated);
    }
}
