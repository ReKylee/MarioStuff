using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.SpecialConditions
{
    /// <summary>
    ///     Condition that evaluates time spent in the current state
    /// </summary>
    [Serializable]
    public class TimeCondition : FlowCondition
    {

        private const string StateTimeParameter = "StateTime";
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.GreaterOrEqual;

        public TimeCondition()
            : base("Time Condition")
        {
        }

        public TimeCondition(float duration, ComparisonType comparisonType = ComparisonType.GreaterOrEqual,
            bool isNegated = false)
            : base($"State time {GetComparisonSymbol(comparisonType)} {duration}s", isNegated)
        {
            _duration = duration;
            _comparisonType = comparisonType;
        }

        /// <summary>
        ///     Gets the duration to compare against
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        ///     Gets the comparison type
        /// </summary>
        public ComparisonType ComparisonType => _comparisonType;

        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public override ConditionType ConditionType => ConditionType.TimeBased;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (context == null || !context.HasParameter(StateTimeParameter))
                return false;

            float stateTime = context.GetParameter<float>(StateTimeParameter);

            return _comparisonType switch
            {
                ComparisonType.Equal => Math.Abs(stateTime - _duration) < 0.0001f,
                ComparisonType.NotEqual => Math.Abs(stateTime - _duration) >= 0.0001f,
                ComparisonType.Greater => stateTime > _duration,
                ComparisonType.GreaterOrEqual => stateTime >= _duration,
                ComparisonType.Less => stateTime < _duration,
                ComparisonType.LessOrEqual => stateTime <= _duration,
                _ => false
            };
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone() => new TimeCondition(_duration, _comparisonType, isNegated);

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
