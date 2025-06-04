using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Base class for all conditions in the animation flow system
    /// </summary>
    public abstract class BaseCondition : ICondition
    {

        /// <summary>
        ///     The data type of this condition
        /// </summary>
        public abstract ConditionDataType DataType { get; }

        /// <summary>
        ///     The comparison type of this condition
        /// </summary>
        public abstract ComparisonType ComparisonType { get; }

        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public abstract bool Evaluate(IAnimationContext context);

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public abstract string GetDescription();
    }
}
