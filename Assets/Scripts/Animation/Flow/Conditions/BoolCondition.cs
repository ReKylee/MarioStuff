using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Condition that checks a boolean parameter
    /// </summary>
    public class BoolCondition : BaseCondition
    {

        /// <summary>
        ///     Create a new boolean condition
        /// </summary>
        /// <param name="parameterName">Name of the boolean parameter to check</param>
        /// <param name="expectedValue">Value the parameter should equal for the condition to be true</param>
        public BoolCondition(string parameterName, bool expectedValue)
        {
            ParameterName = parameterName;
            ExpectedValue = expectedValue;
        }

        /// <summary>
        ///     Parameter name being checked
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        ///     The expected value for this condition to be true
        /// </summary>
        public bool ExpectedValue { get; }


        /// <summary>
        ///     The type of this condition
        /// </summary>
        public override ConditionDataType DataType => ConditionDataType.Boolean;

        public override ComparisonType ComparisonType => ComparisonType.Equals;
        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null || !context.HasParameter(ParameterName))
                return false;

            bool paramValue = context.GetParameter<bool>(ParameterName);
            return paramValue == ExpectedValue;
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription() => $"{ParameterName} is {(ExpectedValue ? "true" : "false")}";
    }
}
