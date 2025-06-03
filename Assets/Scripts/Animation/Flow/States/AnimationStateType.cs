namespace Animation.Flow.States
{
    /// <summary>
    ///     Defines all available animation state types
    /// </summary>
    public enum AnimationStateType
    {
        /// <summary>
        ///     One-time animation that plays and stops
        /// </summary>
        OneTime,

        /// <summary>
        ///     Looping animation that repeats
        /// </summary>
        Looping,

        /// <summary>
        ///     Holds on a specific frame
        /// </summary>
        HoldFrame
    }
}
