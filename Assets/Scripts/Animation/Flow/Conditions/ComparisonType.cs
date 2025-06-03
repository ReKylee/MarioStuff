using System;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Types of comparisons for conditions
    /// </summary>
    [Serializable]
    public enum ComparisonType
    {
        /// <summary>
        ///     Equality comparison (==)
        /// </summary>
        Equals,

        /// <summary>
        ///     Inequality comparison (!=)
        /// </summary>
        NotEquals,

        /// <summary>
        ///     Greater than comparison (>)
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     Less than comparison (<)
        /// </summary>
        LessThan,

        /// <summary>
        ///     Greater than or equal comparison (>=)
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        ///     Less than or equal comparison (<=)
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        ///     Boolean is true
        /// </summary>
        IsTrue,

        /// <summary>
        ///     Boolean is false
        /// </summary>
        IsFalse,

        /// <summary>
        ///     String contains substring
        /// </summary>
        Contains,

        /// <summary>
        ///     String starts with substring
        /// </summary>
        StartsWith,

        /// <summary>
        ///     String ends with substring
        /// </summary>
        EndsWith,

        /// <summary>
        ///     String equality ignoring case
        /// </summary>
        EqualsIgnoreCase,

        /// <summary>
        ///     Time has elapsed
        /// </summary>
        Elapsed,

        /// <summary>
        ///     Animation has completed
        /// </summary>
        Completed
    }
}
