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
        [SerializeField] protected string parameterName;

        protected ParameterCondition()
        {
        }

        protected ParameterCondition(string parameterName, string name = null, bool isNegated = false)
            : base(name ?? $"Parameter '{parameterName}'", isNegated)
        {
            this.parameterName = parameterName;
        }


        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public override ConditionType ConditionType => ConditionType.ParameterComparison;

        /// <summary>
        ///     Checks if the parameter exists in the context
        /// </summary>
        protected bool ParameterExists(IAnimationContext context) => context != null &&
                                                                     !string.IsNullOrEmpty(parameterName) &&
                                                                     context.HasParameter(parameterName);
    }
}
