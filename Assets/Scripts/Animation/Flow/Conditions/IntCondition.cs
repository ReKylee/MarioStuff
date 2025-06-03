using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{


    /// <summary>
    ///     Condition that compares an integer parameter against a value
    /// </summary>
    public class IntCondition : BaseCondition
    {
        /// <summary>
        ///     Create a new integer comparison condition
        /// </summary>
        /// <param name="parameterName">Name of the integer parameter to check</param>
        /// <param name="comparisonType">Type of comparison to perform</param>
        /// <param name="compareValue">Value to compare against</param>
        public IntCondition(string parameterName, ComparisonType comparisonType, int compareValue)
        {
            ParameterName = parameterName;
            ComparisonType = comparisonType;
            CompareValue = compareValue;
        }

        /// <summary>
        ///     Parameter name being checked
        /// </summary>
        public string ParameterName { get; }

        public override ConditionDataType DataType => ConditionDataType.Integer;

        /// <summary>
        ///     The comparison type for this condition
        /// </summary>
        public override ComparisonType ComparisonType { get; }

        /// <summary>
        ///     The value being compared against
        /// </summary>
        public int CompareValue { get; }


        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null || !context.HasParameter(ParameterName))
                return false;

            int paramValue = context.GetParameter<int>(ParameterName);

            return ComparisonType switch
            {
                ComparisonType.Equals => paramValue == CompareValue,
                ComparisonType.GreaterThan => paramValue > CompareValue,
                ComparisonType.LessThan => paramValue < CompareValue,
                ComparisonType.GreaterThanOrEqual => paramValue >= CompareValue,
                ComparisonType.LessThanOrEqual => paramValue <= CompareValue,
                _ => false
            };
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription()
        {
            string comparisonSymbol = ComparisonType switch
            {
                ComparisonType.Equals => "=",
                ComparisonType.GreaterThan => ">",
                ComparisonType.LessThan => "<",
                ComparisonType.GreaterThanOrEqual => ">=",
                ComparisonType.LessThanOrEqual => "<=",
                _ => "?"
            };

            return $"{ParameterName} {comparisonSymbol} {CompareValue}";
        }
    }
}
