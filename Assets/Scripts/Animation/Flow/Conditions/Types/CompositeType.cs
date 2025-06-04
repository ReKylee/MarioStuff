using System;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Types of condition groups for composite conditions
    /// </summary>
    [Serializable]
    public enum CompositeType
    {
        /// <summary>
        ///     All conditions must be true (AND logic)
        /// </summary>
        And,

        /// <summary>
        ///     At least one condition must be true (OR logic)
        /// </summary>
        Or,

        /// <summary>
        ///     At least N conditions must be true
        /// </summary>
        AtLeast,

        /// <summary>
        ///     Exactly N conditions must be true
        /// </summary>
        Exactly,

        /// <summary>
        ///     At most N conditions must be true
        /// </summary>
        AtMost
    }
}
