using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels.Conditions
{

    /// <summary>
    ///     Utility for selecting comparison types based on data type
    /// </summary>
    public class ComparisonTypeSelector
    {
        private readonly ConditionDataType _dataType;

        public ComparisonTypeSelector(ConditionDataType dataType)
        {
            _dataType = dataType;
        }

        /// <summary>
        ///     Get available comparison types for the current data type
        /// </summary>
        public List<ComparisonType> GetAvailableComparisonTypes()
        {
            return _dataType switch
            {
                ConditionDataType.Boolean => new List<ComparisonType>
                    { ComparisonType.Equals },
                ConditionDataType.Integer => new List<ComparisonType>
                {
                    ComparisonType.Equals,
                    ComparisonType.NotEquals,
                    ComparisonType.GreaterThan,
                    ComparisonType.GreaterThanOrEqual,
                    ComparisonType.LessThan,
                    ComparisonType.LessThanOrEqual
                },
                ConditionDataType.Float => new List<ComparisonType>
                {
                    ComparisonType.Equals,
                    ComparisonType.NotEquals,
                    ComparisonType.GreaterThan,
                    ComparisonType.GreaterThanOrEqual,
                    ComparisonType.LessThan,
                    ComparisonType.LessThanOrEqual
                },
                ConditionDataType.String => new List<ComparisonType>
                {
                    ComparisonType.Equals,
                    ComparisonType.NotEquals,
                    ComparisonType.Contains,
                    ComparisonType.StartsWith,
                    ComparisonType.EndsWith
                },
                ConditionDataType.Time => new List<ComparisonType>
                {
                    ComparisonType.GreaterThan,
                    ComparisonType.GreaterThanOrEqual,
                    ComparisonType.LessThan,
                    ComparisonType.LessThanOrEqual
                },
                ConditionDataType.Animation => new List<ComparisonType>
                {
                    ComparisonType.Completed
                },
                ConditionDataType.Composite => new List<ComparisonType>
                {
                    ComparisonType.IsTrue,
                    ComparisonType.IsFalse
                },
                _ => new List<ComparisonType> { ComparisonType.Equals }
            };
        }

    }
}
