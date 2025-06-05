namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Enumeration of condition types for categorization
    /// </summary>
    public enum ConditionType
    {
        // Parameter-based conditions
        ParameterComparison,

        // Logical conditions
        Composite,

        // Special conditions
        AnimationComplete,
        TimeBased,

        // Custom conditions
        Custom
    }
}
