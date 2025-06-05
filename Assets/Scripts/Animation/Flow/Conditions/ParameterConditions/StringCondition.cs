using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.ParameterConditions
{

    /// <summary>
    ///     Condition that evaluates a string parameter
    /// </summary>
    [Serializable]
    public class StringCondition : ParameterCondition
    {
        [SerializeField] private string _compareValue;
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.Equal;
        [SerializeField] private bool _ignoreCase = true;

        public StringCondition()
        {
        }

        public StringCondition(string parameterName, string compareValue,
            ComparisonType comparisonType = ComparisonType.Equal,
            bool ignoreCase = true, bool isNegated = false)
            : base(parameterName, $"Parameter '{parameterName}' {GetComparisonText(comparisonType)} '{compareValue}'",
                isNegated)
        {
            _compareValue = compareValue;
            _comparisonType = comparisonType;
            _ignoreCase = ignoreCase;
        }

        /// <summary>
        ///     Gets the value to compare against
        /// </summary>
        public string CompareValue => _compareValue;

        /// <summary>
        ///     Gets the comparison type
        /// </summary>
        public ComparisonType ComparisonType => _comparisonType;

        /// <summary>
        ///     Gets whether case should be ignored in the comparison
        /// </summary>
        public bool IgnoreCase => _ignoreCase;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (!ParameterExists(context))
                return false;

            string value = context.GetParameter<string>(parameterName);
            if (value == null) value = string.Empty;
            if (_compareValue == null) _compareValue = string.Empty;

            StringComparison comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return _comparisonType switch
            {
                ComparisonType.Equal => string.Equals(value, _compareValue, comparison),
                ComparisonType.NotEqual => !string.Equals(value, _compareValue, comparison),
                ComparisonType.Contains => value.IndexOf(_compareValue, comparison) >= 0,
                ComparisonType.StartsWith => value.StartsWith(_compareValue, comparison),
                ComparisonType.EndsWith => value.EndsWith(_compareValue, comparison),
                _ => false
            };
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone() =>
            new StringCondition(parameterName, _compareValue, _comparisonType, _ignoreCase, isNegated);

        /// <summary>
        ///     Gets a string representation of the comparison type
        /// </summary>
        private static string GetComparisonText(ComparisonType type)
        {
            return type switch
            {
                ComparisonType.Equal => "equals",
                ComparisonType.NotEqual => "not equals",
                ComparisonType.Contains => "contains",
                ComparisonType.StartsWith => "starts with",
                ComparisonType.EndsWith => "ends with",
                _ => "?"
            };
        }
    }
}
