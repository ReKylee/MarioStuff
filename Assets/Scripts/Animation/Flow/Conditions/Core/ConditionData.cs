using System;
using UnityEngine;

namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Serializable data for a condition
    /// </summary>
    [Serializable]
    public class ConditionData
    {
        [Tooltip("Unique identifier for this condition")]
        public string UniqueId = Guid.NewGuid().ToString();

        [Tooltip("Type of data for this condition")]
        public ConditionDataType DataType;

        [Tooltip("Type of comparison for this condition")]
        public ComparisonType ComparisonType;

        [Tooltip("Name of the parameter to check")]
        public string ParameterName = string.Empty;

        [Tooltip("String value for the condition")]
        public string StringValue = string.Empty;

        [Tooltip("Float value for the condition")]
        public float FloatValue;

        [Tooltip("Integer value for the condition")]
        public int IntValue;

        [Tooltip("Boolean value for the condition")]
        public bool BoolValue;

        [Tooltip("Index of this condition in its parent group")]
        public int GroupIndex;

        [Tooltip("Depth level of this condition in nested groups")]
        public int NestingLevel;

        [Tooltip("Parent group ID for this condition")]
        public string ParentGroupId = string.Empty;

        /// <summary>
        ///     Create an empty condition data object
        /// </summary>
        public ConditionData()
        {
        }

        /// <summary>
        ///     Create a condition data object with specified type and parameter
        /// </summary>
        public ConditionData(ConditionDataType dataType, ComparisonType comparisonType, string parameterName = "")
        {
            DataType = dataType;
            ComparisonType = comparisonType;
            ParameterName = parameterName;

        }


        /// <summary>
        ///     Get the value object based on the data type
        /// </summary>
        public object GetValue()
        {
            return DataType switch
            {
                ConditionDataType.Boolean => BoolValue,
                ConditionDataType.Float => FloatValue,
                ConditionDataType.Integer => IntValue,
                ConditionDataType.String => StringValue,
                ConditionDataType.Time => FloatValue,
                _ => null
            };
        }

        /// <summary>
        ///     Set a value based on the data type
        /// </summary>
        public void SetValue(object value)
        {
            switch (DataType)
            {
                case ConditionDataType.Boolean:
                    BoolValue = Convert.ToBoolean(value);
                    break;

                case ConditionDataType.Float:
                case ConditionDataType.Time:
                    FloatValue = Convert.ToSingle(value);
                    break;

                case ConditionDataType.Integer:
                    IntValue = Convert.ToInt32(value);
                    break;

                case ConditionDataType.String:
                    StringValue = value?.ToString() ?? string.Empty;
                    break;
            }
        }

        /// <summary>
        ///     Create a deep copy of this condition data
        /// </summary>
        public ConditionData Clone() =>
            new()
            {
                DataType = DataType,
                ComparisonType = ComparisonType,
                ParameterName = ParameterName,
                StringValue = StringValue,
                FloatValue = FloatValue,
                IntValue = IntValue,
                BoolValue = BoolValue,
                GroupIndex = GroupIndex,
                NestingLevel = NestingLevel,
                ParentGroupId = ParentGroupId
            };
    }
}
