using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Core interface for all animation transition conditions
    /// </summary>
    public interface ICondition
    {
        public ConditionDataType DataType { get; }
        public ComparisonType ComparisonType { get; }

        /// <summary>
        ///     Evaluate the condition against the current animation context
        /// </summary>
        /// <param name="context">Current animation context with parameters</param>
        /// <returns>True if condition is satisfied, false otherwise</returns>
        bool Evaluate(IAnimationContext context);

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        string GetDescription();
    }
}
