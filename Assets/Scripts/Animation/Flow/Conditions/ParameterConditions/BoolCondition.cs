using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.ParameterConditions
{
    /// <summary>
    ///     Condition that evaluates a boolean parameter
    /// </summary>
    [Serializable]
    public class BoolCondition : ParameterCondition
    {
        [SerializeField] private bool _expectedValue = true;

        public BoolCondition()
        {
        }

        public BoolCondition(string parameterName, bool expectedValue = true, bool isNegated = false)
            : base(parameterName, $"Parameter '{parameterName}' == {expectedValue}", isNegated)
        {
            _expectedValue = expectedValue;
        }

        /// <summary>
        ///     Gets the expected value for this condition to be true
        /// </summary>
        public bool ExpectedValue => _expectedValue;

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (!ParameterExists(context))
                return false;

            bool value = context.GetParameter<bool>(parameterName);
            return value == _expectedValue;
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone() => new BoolCondition(parameterName, _expectedValue, isNegated);
    }
}
