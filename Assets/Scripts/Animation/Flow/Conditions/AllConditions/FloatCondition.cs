using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions
{


    /// <summary>
    ///     Condition that compares a float parameter against a value
    /// </summary>
    public class FloatCondition : BaseCondition
    {
        private readonly float _threshold;

        /// <summary>
        ///     Create a new float comparison condition
        /// </summary>
        /// <param name="parameterName">Name of the float parameter to check</param>
        /// <param name="comparisonType">Type of comparison to perform</param>
        /// <param name="compareValue">Value to compare against</param>
        /// <param name="threshold">Threshold for equality comparison (default 0.001f)</param>
        public FloatCondition(string parameterName, ComparisonType comparisonType, float compareValue,
            float threshold = 0.001f)
        {
            ParameterName = parameterName;
            ComparisonType = comparisonType;
            CompareValue = compareValue;
            _threshold = threshold;
        }

        /// <summary>
        ///     Parameter name being checked
        /// </summary>
        public string ParameterName { get; }

        public override ConditionDataType DataType => ConditionDataType.Float;

        /// <summary>
        ///     The comparison type for this condition
        /// </summary>
        public override ComparisonType ComparisonType { get; }

        /// <summary>
        ///     The value being compared against
        /// </summary>
        public float CompareValue { get; }


        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null || !context.HasParameter(ParameterName))
                return false;

            float paramValue = context.GetParameter<float>(ParameterName);

            return ComparisonType switch
            {
                ComparisonType.Equals => Mathf.Abs(paramValue - CompareValue) <= _threshold,
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
