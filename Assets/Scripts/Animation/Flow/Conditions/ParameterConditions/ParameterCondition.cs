using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.ParameterConditions
{
    /// <summary>
    ///     Base class for parameter-based conditions
    /// </summary>
    [Serializable]
    public abstract class ParameterCondition : FlowCondition
    {
        [SerializeField] protected string _parameterName;

        protected ParameterCondition() { }

        protected ParameterCondition(string parameterName, string name = null, bool isNegated = false)
            : base(name ?? $"Parameter '{parameterName}'" , isNegated)
        {
            _parameterName = parameterName;
        }

        /// <summary>
        ///     Gets the parameter name this condition checks
        /// </summary>
        public string ParameterName => _parameterName;

        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public override ConditionType ConditionType => ConditionType.ParameterComparison;

        /// <summary>
        ///     Checks if the parameter exists in the context
        /// </summary>
        protected bool ParameterExists(IAnimationContext context)
        {
            return context != null && !string.IsNullOrEmpty(_parameterName) && context.HasParameter(_parameterName);
        }
    }
}
