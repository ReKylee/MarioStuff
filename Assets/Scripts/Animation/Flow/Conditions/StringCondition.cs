using System;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Conditions
{


    /// <summary>
    ///     Condition that compares a string parameter against a value
    /// </summary>
    public class StringCondition : BaseCondition
    {

        /// <summary>
        ///     Create a new string comparison condition
        /// </summary>
        /// <param name="parameterName">Name of the string parameter to check</param>
        /// <param name="comparisonType">Type of comparison to perform</param>
        /// <param name="compareValue">Value to compare against</param>
        public StringCondition(string parameterName, ComparisonType comparisonType, string compareValue)
        {
            ParameterName = parameterName;
            ComparisonType = comparisonType;
            CompareValue = compareValue ?? string.Empty;
        }

        /// <summary>
        ///     Parameter name being checked
        /// </summary>
        public string ParameterName { get; }

        public override ConditionDataType DataType => ConditionDataType.String;

        /// <summary>
        ///     The comparison type for this condition
        /// </summary>
        public override ComparisonType ComparisonType { get; }

        /// <summary>
        ///     The value being compared against
        /// </summary>
        public string CompareValue { get; }


        /// <summary>
        ///     Evaluate this condition against the given context
        /// </summary>
        public override bool Evaluate(IAnimationContext context)
        {
            if (context == null || !context.HasParameter(ParameterName))
                return false;

            string paramValue = context.GetParameter<string>(ParameterName) ?? string.Empty;

            return ComparisonType switch
            {
                ComparisonType.Equals => paramValue.Equals(CompareValue),
                ComparisonType.EqualsIgnoreCase =>
                    paramValue.Equals(CompareValue, StringComparison.OrdinalIgnoreCase),
                ComparisonType.Contains => paramValue.Contains(CompareValue),
                ComparisonType.StartsWith => paramValue.StartsWith(CompareValue),
                ComparisonType.EndsWith => paramValue.EndsWith(CompareValue),
                _ => false
            };
        }

        /// <summary>
        ///     Get a human-readable description of this condition
        /// </summary>
        public override string GetDescription()
        {
            string comparisonText = ComparisonType switch
            {
                ComparisonType.Equals => "equals",
                ComparisonType.EqualsIgnoreCase => "equals (ignore case)",
                ComparisonType.Contains => "contains",
                ComparisonType.StartsWith => "starts with",
                ComparisonType.EndsWith => "ends with",
                _ => "?"
            };

            return $"{ParameterName} {comparisonText} \"{CompareValue}\"";
        }
    }
}
