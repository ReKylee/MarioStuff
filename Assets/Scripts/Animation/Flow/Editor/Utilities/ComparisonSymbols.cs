using System.Collections.Generic;
using Animation.Flow.Conditions;

namespace Animation.Flow.Editor.Utilities
{
    /// <summary>
    ///     Utility class to convert comparison types to symbols for the UI
    /// </summary>
    public static class ComparisonSymbols
    {
        private static readonly Dictionary<ComparisonType, string> Symbols = new()
        {
            { ComparisonType.Equals, "=" },
            { ComparisonType.NotEquals, "≠" },
            { ComparisonType.GreaterThan, ">" },
            { ComparisonType.LessThan, "<" },
            { ComparisonType.GreaterThanOrEqual, "≥" },
            { ComparisonType.LessThanOrEqual, "≤" },
            { ComparisonType.Contains, "∈" },
            { ComparisonType.StartsWith, "≻" },
            { ComparisonType.EndsWith, "≺" }
        };

        /// <summary>
        ///     Convert a comparison type to its symbolic representation
        /// </summary>
        /// <param name="comparisonType">The comparison type to convert</param>
        /// <returns>A symbol representing the comparison, or the enum string if no mapping exists</returns>
        public static string GetSymbol(ComparisonType comparisonType) =>
            Symbols.GetValueOrDefault(comparisonType, comparisonType.ToString());

        /// <summary>
        ///     Get the full text description for a comparison type (for tooltips and menus)
        /// </summary>
        /// <param name="comparisonType">The comparison type</param>
        /// <returns>Human readable description of the comparison type</returns>
        public static string GetDescription(ComparisonType comparisonType)
        {
            string symbol = GetSymbol(comparisonType);
            return $"{comparisonType} ({symbol})";
        }
    }
}
