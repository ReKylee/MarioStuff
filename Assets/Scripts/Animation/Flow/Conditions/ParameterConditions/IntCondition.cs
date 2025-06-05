using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.ParameterConditions
{
    /// <summary>
    ///     Condition that evaluates an integer parameter
    /// </summary>
    [Serializable]
    public class IntCondition : ParameterCondition
    {
        [SerializeField] private int _compareValue;
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.Equal;

        public IntCondition() { }

        public IntCondition(string parameterName, int compareValue, ComparisonType comparisonType = ComparisonType.Equal, 
            bool isNegated = false)
            : base(parameterName, $"Parameter '{parameterName}' {GetComparisonSymbol(comparisonType)} {compareValue}", isNegated)
        {
            _compareValue = compareValue;
            _comparisonType = comparisonType;
        }

        /// <summary>
        ///     Gets the value to compare against
        /// </summary>
        public int CompareValue => _compareValue;

        /// <summary>
        ///     Gets the comparison type
        /// </summary>
        public ComparisonType ComparisonType => _comparisonType;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (!ParameterExists(context))
                return false;

            int value = context.GetParameter<int>(_parameterName);

            return _comparisonType switch
            {
                ComparisonType.Equal => value == _compareValue,
                ComparisonType.NotEqual => value != _compareValue,
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
        public override FlowCondition Clone()
        {
            return new IntCondition(_parameterName, _compareValue, _comparisonType, _isNegated);
        }

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
