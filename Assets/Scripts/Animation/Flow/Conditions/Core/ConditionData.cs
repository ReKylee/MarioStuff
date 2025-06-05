using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.ParameterConditions;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Types of parameter values
    /// </summary>
    public enum ParameterValueType
    {
        Bool,
        Int,
        Float,
        String
    }

    /// <summary>
    ///     Serializable data for storing condition information
    /// </summary>
    [Serializable]
    public class ConditionData
    {
        [SerializeField] public string UniqueId;
        [SerializeField] public uint NestingLevel;
        [SerializeField] private ConditionType _type;
        [SerializeField] private bool _isNegated;

        // Parameter condition data
        [SerializeField] private string _parameterName;
        [SerializeField] private ParameterValueType _parameterValueType;

        // Possible values (only one will be used depending on the parameter type)
        [SerializeField] private bool _boolValue;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private string _stringValue;

        // For numeric comparisons
        [SerializeField] private ComparisonType _comparisonType;

        // For composite conditions
        [SerializeField] private List<ConditionData> _childConditions;
        [SerializeField] public string ParentGroupId;

        /// <summary>
        ///     Gets or sets the condition type
        /// </summary>
        public ConditionType Type
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        ///     Gets or sets whether the condition is negated
        /// </summary>
        public bool IsNegated
        {
            get => _isNegated;
            set => _isNegated = value;
        }

        /// <summary>
        ///     Gets or sets the parameter name
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value;
        }

        /// <summary>
        ///     Gets or sets the parameter value type
        /// </summary>
        public ParameterValueType ParameterValueType
        {
            get => _parameterValueType;
            set => _parameterValueType = value;
        }

        /// <summary>
        ///     Gets or sets the boolean value
        /// </summary>
        public bool BoolValue
        {
            get => _boolValue;
            set => _boolValue = value;
        }

        /// <summary>
        ///     Gets or sets the integer value
        /// </summary>
        public int IntValue
        {
            get => _intValue;
            set => _intValue = value;
        }

        /// <summary>
        ///     Gets or sets the float value
        /// </summary>
        public float FloatValue
        {
            get => _floatValue;
            set => _floatValue = value;
        }

        /// <summary>
        ///     Gets or sets the string value
        /// </summary>
        public string StringValue
        {
            get => _stringValue;
            set => _stringValue = value;
        }

        /// <summary>
        ///     Gets or sets the comparison type
        /// </summary>
        public ComparisonType ComparisonType
        {
            get => _comparisonType;
            set => _comparisonType = value;
        }

        /// <summary>
        ///     Gets or sets the child conditions for composite conditions
        /// </summary>
        public List<ConditionData> ChildConditions
        {
            get => _childConditions;
            set => _childConditions = value;
        }

        /// <summary>
        ///     Creates a deep copy of this condition data
        /// </summary>
        public ConditionData Clone()
        {
            ConditionData clone = new ConditionData
            {
                _type = _type,
                _isNegated = _isNegated,
                _parameterName = _parameterName,
                _parameterValueType = _parameterValueType,
                _boolValue = _boolValue,
                _intValue = _intValue,
                _floatValue = _floatValue,
                _stringValue = _stringValue,
                _comparisonType = _comparisonType
            };

            // Clone child conditions if they exist
            if (_childConditions != null && _childConditions.Count > 0)
            {
                clone._childConditions = new List<ConditionData>();
                foreach (var childCondition in _childConditions)
                {
                    clone._childConditions.Add(childCondition.Clone());
                }
            }

            return clone;
        }
    }
}
