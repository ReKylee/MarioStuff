using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Condition that checks if the current animation has completed
    /// </summary>
    public class AnimationCompleteCondition : BaseCondition
    {
        /// <summary>
        ///     The type of this condition
        /// </summary>
        public override ConditionDataType DataType => ConditionDataType.Animation;

        public override ComparisonType ComparisonType => ComparisonType.Completed;

        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null ||
                !context.HasParameter(
                    "AnimationComplete")) // It's good practice to check for the parameter's existence.
                return false;

            return context.GetParameter<bool>("AnimationComplete");
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription() => "Animation Completed";
    }
}
