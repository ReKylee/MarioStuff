namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Interface for all conditions in the animation system
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        bool Evaluate(IAnimationContext context);
    }
}
