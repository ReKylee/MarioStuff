using System;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Types of data that can be used in conditions
    /// </summary>
    [Serializable]
    public enum ConditionDataType
    {
        /// <summary>
        ///     Boolean data type (true/false)
        /// </summary>
        Boolean,

        /// <summary>
        ///     Integer data type
        /// </summary>
        Integer,

        /// <summary>
        ///     Float data type
        /// </summary>
        Float,

        /// <summary>
        ///     String data type
        /// </summary>
        String,

        /// <summary>
        ///     Composite condition group (contains other conditions)
        /// </summary>
        Composite,

        /// <summary>
        ///     Time-based condition
        /// </summary>
        Time,

        /// <summary>
        ///     Animation-based condition
        /// </summary>
        Animation
    }
}
