using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     A NOT condition that inverts the result of another condition
    /// </summary>
    public class NotCondition : BaseCondition
    {

        /// <summary>
        ///     Create a new NOT condition
        /// </summary>
        /// <param name="condition">The condition to negate</param>
        public NotCondition(ICondition condition)
        {
            InnerCondition = condition;
        }

        /// <summary>
        ///     The condition being negated
        /// </summary>
        public ICondition InnerCondition { get; }

        /// <summary>
        ///     The type of this condition
        /// </summary>

        public override ConditionDataType DataType => ConditionDataType.Composite;

        public override ComparisonType ComparisonType => ComparisonType.IsFalse;
        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            // If no inner condition, return true (NOT false = true)
            if (InnerCondition == null)
                return true;

            // Negate the inner condition result
            return !InnerCondition.Evaluate(context);
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription()
        {
            if (InnerCondition == null)
                return "NOT (null)";

            return $"NOT ({InnerCondition.GetDescription()})";
        }
    }
}
