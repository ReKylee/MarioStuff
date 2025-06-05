using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;

namespace Animation.Flow.Editor.Panels.Conditions
{
    /// <summary>
    ///     Utility for selecting comparison types based on condition type
    /// </summary>
    public class ComparisonTypeSelector
    {
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

        private readonly FlowCondition _condition;

        public ComparisonTypeSelector(FlowCondition condition)
        {
            _condition = condition;
        }

        /// <summary>
        ///     Get available comparison types for the current condition type
        /// </summary>
        public List<ComparisonType> GetAvailableComparisonTypes()
        {
            // Check the specific condition type using 'is' pattern matching
            if (_condition is BoolCondition)
            {
                return new List<ComparisonType> { ComparisonType.Equal };
            }

            if (_condition is IntCondition)
            {
                return new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Greater,
                    ComparisonType.GreaterOrEqual,
                    ComparisonType.Less,
                    ComparisonType.LessOrEqual
                };
            }

            if (_condition is FloatCondition)
            {
                return new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Greater,
                    ComparisonType.GreaterOrEqual,
                    ComparisonType.Less,
                    ComparisonType.LessOrEqual
                };
            }

            if (_condition is StringCondition)
            {
                return new List<ComparisonType>
                {
                    ComparisonType.Equal,
                    ComparisonType.NotEqual,
                    ComparisonType.Contains,
                    ComparisonType.StartsWith,
                    ComparisonType.EndsWith
                };
            }

            // Default fallback for unknown condition types
            return new List<ComparisonType> { ComparisonType.Equal };
        }

        /// <summary>
        ///     Get the symbol for a comparison type
        /// </summary>
        public static string GetSymbol(ComparisonType comparisonType) =>
            Symbols.TryGetValue(comparisonType, out string symbol) ? symbol : "?";

        /// <summary>
        ///     Get a human-readable description for a comparison type
        /// </summary>
        public static string GetDescription(ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.Equal => "Equal to",
                ComparisonType.NotEqual => "Not equal to",
                ComparisonType.Greater => "Greater than",
                ComparisonType.Less => "Less than",
                ComparisonType.GreaterOrEqual => "Greater than or equal to",
                ComparisonType.LessOrEqual => "Less than or equal to",
                ComparisonType.Contains => "Contains",
                ComparisonType.StartsWith => "Starts with",
                ComparisonType.EndsWith => "Ends with",
                _ => comparisonType.ToString()
            };
        }
    }
}
