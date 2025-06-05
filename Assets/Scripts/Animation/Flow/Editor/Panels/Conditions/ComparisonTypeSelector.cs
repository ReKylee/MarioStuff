using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels.Conditions
{

    /// <summary>
    ///     Utility for selecting comparison types based on data type
    /// </summary>
    public class ComparisonTypeSelector
    {
        private readonly ParameterValueType _dataType;

        private static readonly Dictionary<ComparisonType, string> Symbols = new()
        {
            { ComparisonType.Equal, "=" },
            { ComparisonType.NotEqual, "≠" },
            { ComparisonType.Greater, ">" },
            { ComparisonType.Less, "<" },
            { ComparisonType.GreaterOrEqual, "≥" },
            { ComparisonType.LessOrEqual, "≤" },
            { ComparisonType.Contains, "∈" },
            { ComparisonType.StartsWith, "≻" },
            { ComparisonType.EndsWith, "≺" }
        };

        public ComparisonTypeSelector(ParameterValueType dataType)
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
                ParameterValueType.Bool => new List<ComparisonType>
                    { ComparisonType.Equal },
                ParameterValueType.Int => new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Greater,
                    ComparisonType.GreaterOrEqual,
                    ComparisonType.Less,
                    ComparisonType.LessOrEqual
                },
                ParameterValueType.Float => new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Greater,
                    ComparisonType.GreaterOrEqual,
                    ComparisonType.Less,
                    ComparisonType.LessOrEqual
                },
                ParameterValueType.String => new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Contains,
                    ComparisonType.StartsWith,
                    ComparisonType.EndsWith
                },
                // Support for special types can be added with custom handling
                _ => new List<ComparisonType> { ComparisonType.Equal }
            };
        }

    }
}
