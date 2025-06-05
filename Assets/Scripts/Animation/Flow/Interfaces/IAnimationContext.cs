namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Interface for animation context that provides parameter values
    /// </summary>
    public interface IAnimationContext
    {
        /// <summary>
        ///     Gets the animator instance
        /// </summary>
        IAnimator Animator { get; }


        /// <summary>
        ///     Check if a parameter exists in this context
        /// </summary>
        bool HasParameter(string parameterName);

        /// <summary>
        ///     Get a parameter value from this context
        /// </summary>
        T GetParameter<T>(string parameterName);

        /// <summary>
        ///     Set a parameter value in this context
        /// </summary>
        void SetParameter<T>(string parameterName, T value);
    }
}
