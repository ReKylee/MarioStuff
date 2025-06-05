using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Base class for all flow conditions with integrated editor data
    /// </summary>
    [Serializable]
    public abstract class FlowCondition : ICondition
    {
        [SerializeField] protected string name;
        [SerializeField] protected bool isNegated;

        // Editor data from ConditionData
        [SerializeField] public string uniqueId;
        [SerializeField] public uint nestingLevel;
        [SerializeField] public string parentGroupId;

        // Parameter condition data
        [SerializeField] private string parameterName;
        [SerializeField] private bool boolValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private string stringValue;
        [SerializeField] private ComparisonType comparisonType;

        // For composite conditions
        [SerializeField] private List<FlowCondition> _childConditions;
        private Type _parameterValueType;

        protected FlowCondition()
        {
            uniqueId = Guid.NewGuid().ToString();
        }

        protected FlowCondition(string name, bool isNegated = false) : this()
        {
            this.name = name;
            this.isNegated = isNegated;
        }

        // Original properties
        public string Name => name;

        public bool IsNegated
        {
            get => isNegated;
            set => isNegated = value;
        }

        public abstract ConditionType ConditionType { get; }

        // New properties from ConditionData
        public string ParameterName
        {
            get => parameterName;
            set => parameterName = value;
        }

        public Type ParameterValueType
        {
            get => _parameterValueType;
            set => _parameterValueType = value;
        }

        public bool BoolValue
        {
            get => boolValue;
            set => boolValue = value;
        }

        public int IntValue
        {
            get => intValue;
            set => intValue = value;
        }

        public float FloatValue
        {
            get => floatValue;
            set => floatValue = value;
        }

        public string StringValue
        {
            get => stringValue;
            set => stringValue = value;
        }

        public ComparisonType ComparisonType
        {
            get => comparisonType;
            set => comparisonType = value;
        }

        public List<FlowCondition> ChildConditions
        {
            get => _childConditions;
            set => _childConditions = value;
        }

        // Original methods
        public bool Evaluate(IAnimationContext context)
        {
            bool result = EvaluateInternal(context);
            return isNegated ? !result : result;
        }

        protected abstract bool EvaluateInternal(IAnimationContext context);
        public abstract FlowCondition Clone();

        public void Validate()
        {
            // Validate parameter name for parameter-based conditions
            if (ConditionType == ConditionType.ParameterComparison && string.IsNullOrEmpty(parameterName))
            {
                throw new InvalidOperationException("Parameter name cannot be null or empty for parameter conditions.");
            }

            // Validate child conditions for composite conditions
            if (ConditionType == ConditionType.Composite)
            {
                if (_childConditions == null)
                {
                    _childConditions = new List<FlowCondition>();
                }

                foreach (FlowCondition childCondition in _childConditions)
                {
                    childCondition?.Validate();
                }
            }
        }
    }
}
