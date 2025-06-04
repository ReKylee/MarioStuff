using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Conditions;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Handles comparison type selection based on data type
    /// </summary>
    public class ComparisonTypeSelector
    {

        private readonly ConditionDataType _dataType;

        private readonly Dictionary<ConditionDataType, ComparisonType[]> _validComparisons = new()
        {
            { ConditionDataType.Boolean, new[] { ComparisonType.IsTrue, ComparisonType.IsFalse } },
            {
                ConditionDataType.Float,
                new[]
                {
                    ComparisonType.Equals, ComparisonType.NotEquals, ComparisonType.GreaterThan,
                    ComparisonType.LessThan, ComparisonType.GreaterThanOrEqual, ComparisonType.LessThanOrEqual
                }
            },
            {
                ConditionDataType.Integer,
                new[]
                {
                    ComparisonType.Equals, ComparisonType.NotEquals, ComparisonType.GreaterThan,
                    ComparisonType.LessThan, ComparisonType.GreaterThanOrEqual, ComparisonType.LessThanOrEqual
                }
            },
            {
                ConditionDataType.String,
                new[]
                {
                    ComparisonType.Equals, ComparisonType.NotEquals, ComparisonType.Contains, ComparisonType.StartsWith,
                    ComparisonType.EndsWith
                }
            }
        };

        public ComparisonTypeSelector(ConditionDataType dataType)
        {
            _dataType = dataType;
        }

        public PopupField<ComparisonType> CreateDropdown(ComparisonType current, Action<ComparisonType> onChanged)
        {
            var validTypes = GetValidComparisons();
            var dropdown = new PopupField<ComparisonType>(
                validTypes.ToList(),
                current,
                FormatComparison,
                FormatComparison
            );

            dropdown.AddToClassList("comparison-dropdown");
            dropdown.RegisterValueChangedCallback(evt => onChanged(evt.newValue));

            return dropdown;
        }

        private ComparisonType[] GetValidComparisons()
        {
            return _validComparisons.TryGetValue(_dataType, out var types)
                ? types
                : new[] { ComparisonType.Equals };
        }

        private string FormatComparison(ComparisonType type)
        {
            return type switch
            {
                ComparisonType.Equals => "==",
                ComparisonType.NotEquals => "!=",
                ComparisonType.GreaterThan => ">",
                ComparisonType.LessThan => "<",
                ComparisonType.GreaterThanOrEqual => ">=",
                ComparisonType.LessThanOrEqual => "<=",
                ComparisonType.IsTrue => "is true",
                ComparisonType.IsFalse => "is false",
                ComparisonType.Contains => "contains",
                ComparisonType.StartsWith => "starts with",
                ComparisonType.EndsWith => "ends with",
                _ => type.ToString()
            };
        }
    }
}
