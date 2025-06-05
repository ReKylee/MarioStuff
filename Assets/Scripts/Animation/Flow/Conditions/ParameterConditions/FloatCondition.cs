using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.ParameterConditions
{
    /// <summary>
    ///     Condition that evaluates a float parameter
    /// </summary>
    [Serializable]
    public class FloatCondition : ParameterCondition
    {
        [SerializeField] private float _compareValue;
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.Equal;
        [SerializeField] private float _epsilon = 0.0001f; // For float equality comparison

        public FloatCondition()
        {
        }

        public FloatCondition(string parameterName, float compareValue,
            ComparisonType comparisonType = ComparisonType.Equal,
            float epsilon = 0.0001f, bool isNegated = false)
            : base(parameterName, $"Parameter '{parameterName}' {GetComparisonSymbol(comparisonType)} {compareValue}",
                isNegated)
        {
            _compareValue = compareValue;
            _comparisonType = comparisonType;
            _epsilon = epsilon;
        }

        /// <summary>
        ///     Gets the value to compare against
        /// </summary>
        public float CompareValue => _compareValue;

        /// <summary>
        ///     Gets the comparison type
        /// </summary>
        public ComparisonType ComparisonType => _comparisonType;

        /// <summary>
        ///     Gets the epsilon value for float equality comparisons
        /// </summary>
        public float Epsilon => _epsilon;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (!ParameterExists(context))
                return false;

            float value = context.GetParameter<float>(parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => Math.Abs(value - _compareValue) < _epsilon,
                ComparisonType.NotEqual => Math.Abs(value - _compareValue) >= _epsilon,
                ComparisonType.Greater => value > _compareValue,
                ComparisonType.GreaterOrEqual => value >= _compareValue,
                ComparisonType.Less => value < _compareValue,
                ComparisonType.LessOrEqual => value <= _compareValue,
                _ => false
            };
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone() =>
            new FloatCondition(parameterName, _compareValue, _comparisonType, _epsilon, isNegated);

        /// <summary>
        ///     Gets a string representation of the comparison type
        /// </summary>
        private static string GetComparisonSymbol(ComparisonType type)
        {
            return type switch
            {
                ComparisonType.Equal => "==",
                ComparisonType.NotEqual => "!=",
                ComparisonType.Greater => ">",
                ComparisonType.GreaterOrEqual => ">=",
                ComparisonType.Less => "<",
                ComparisonType.LessOrEqual => "<=",
                _ => "?"
            };
        }
    }
}
