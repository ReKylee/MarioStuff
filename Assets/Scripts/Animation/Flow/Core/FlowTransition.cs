using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Core.Types;
using Animation.Flow.Interfaces;
using Animation.Flow.States;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Serializable data for a state transition
    /// </summary>
    [Serializable]
    public class FlowTransition
    {
        [Tooltip("ID of the source state")] [SerializeField]
        public string fromStateId;

        [Tooltip("ID of the destination state")] [SerializeField]
        public string toStateId;

        [Tooltip("Conditions that must be met for this transition")] [SerializeReference]
        public CompositeCondition rootCondition = new(CompositeType.All);


        /// <summary>
        ///     Create an empty transition data object
        /// </summary>
        public FlowTransition()
        {
        }

        /// <summary>
        ///     Create a transition data object with the specified source and destination states
        /// </summary>
        public FlowTransition(string fromStateId, string toStateId)
        {
            this.fromStateId = fromStateId;
            this.toStateId = toStateId;
        }

        /// <summary>
        ///     Create a fully qualified transition data object
        /// </summary>
        public FlowTransition(string fromStateId, FlowStateType fromStateType,
            string toStateId, FlowStateType toStateType)
        {
            this.fromStateId = fromStateId;
            this.toStateId = toStateId;
        }
        public bool CanTransition(IAnimationContext context) => rootCondition.Evaluate(context);
        /// <summary>
        ///     Add a condition to this transition
        /// </summary>
        public void AddCondition(FlowCondition condition)
        {
            if (condition != null)
            {
                rootCondition.AddCondition(condition);
            }
        }
        public void AddConditions(List<FlowCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return;
            rootCondition.AddConditions(conditions);
        }
        public void RemoveCondition(FlowCondition condition)
        {
            if (condition != null)
            {
                rootCondition.RemoveCondition(condition);
            }
        }

        /// <summary>
        ///     Create a deep copy of this transition data
        /// </summary>
        public FlowTransition Clone()
        {
            FlowTransition clone = new()
            {
                fromStateId = fromStateId,
                toStateId = toStateId
            };

            clone.rootCondition = rootCondition.Clone() as CompositeCondition;
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(fromStateId))
            {
                Debug.LogError("FlowTransition: fromStateId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(toStateId))
            {
                Debug.LogError("FlowTransition: toStateId cannot be null or empty");
            }

            if (rootCondition == null)
            {
                Debug.LogError("FlowTransition: rootCondition cannot be null");
            }
            else
            {
                rootCondition.Validate();
            }
        }
    }
}
