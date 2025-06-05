using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Types of composite conditions
    /// </summary>
    public enum CompositeType
    {
        And,
        Or
    }

    /// <summary>
    ///     Condition that combines multiple conditions with AND or OR logic
    /// </summary>
    [Serializable]
    public class CompositeCondition : FlowCondition
    {
        [SerializeField] private CompositeType _compositeType = CompositeType.And;
        [SerializeField] private List<FlowCondition> _conditions = new();

        public CompositeCondition() : base("Composite Condition") { }

        public CompositeCondition(CompositeType compositeType, bool isNegated = false)
            : base(compositeType == CompositeType.And ? "All conditions" : "Any condition", isNegated)
        {
            _compositeType = compositeType;
        }

        public CompositeCondition(CompositeType compositeType, IEnumerable<FlowCondition> conditions, bool isNegated = false)
            : this(compositeType, isNegated)
        {
            if (conditions != null)
            {
                _conditions.AddRange(conditions);
            }
        }

        /// <summary>
        ///     Gets the composite type (AND/OR)
        /// </summary>
        public CompositeType CompositeType => _compositeType;

        /// <summary>
        ///     Gets the conditions in this composite
        /// </summary>
        public IReadOnlyList<FlowCondition> Conditions => _conditions;

        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public override ConditionType ConditionType => ConditionType.Composite;

        /// <summary>
        ///     Adds a condition to this composite
        /// </summary>
        public void AddCondition(ICondition condition)
        {
            if (condition == null) return;

            if (condition is FlowCondition flowCondition)
            {
                _conditions.Add(flowCondition);
            }
            else
            {
                Debug.LogWarning("Cannot add non-FlowCondition to CompositeCondition");
            }
        }

        /// <summary>
        ///     Removes a condition from this composite
        /// </summary>
        public bool RemoveCondition(FlowCondition condition)
        {
            return _conditions.Remove(condition);
        }

        /// <summary>
        ///     Clears all conditions from this composite
        /// </summary>
        public void ClearConditions()
        {
            _conditions.Clear();
        }

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        protected override bool EvaluateInternal(IAnimationContext context)
        {
            if (_conditions.Count == 0)
                return true; // Empty composite is always true

            bool result = _compositeType == CompositeType.And;

            foreach (var condition in _conditions)
            {
                bool conditionResult = condition.Evaluate(context);

                if (_compositeType == CompositeType.And)
                {
                    // AND logic - one false means all false
                    if (!conditionResult)
                        return false;
                }
                else // OR logic
                {
                    // OR logic - one true means all true
                    if (conditionResult)
                        return true;
                }
            }

            // If we get here, then for AND all were true, for OR all were false
            return result;
        }

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public override FlowCondition Clone()
        {
            var clone = new CompositeCondition(_compositeType, _isNegated);

            foreach (var condition in _conditions)
            {
                clone.AddCondition(condition.Clone());
            }

            return clone;
        }
    }
}
