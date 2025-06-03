namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Core interface for all animation states in the flow system
    /// </summary>
    public interface IAnimationState
    {
        /// <summary>
        ///     Unique identifier for this animation state
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Name of the animation to play
        /// </summary>
        string AnimationName { get; }

        /// <summary>
        ///     Whether this animation should loop
        /// </summary>
        bool ShouldLoop { get; }

        /// <summary>
        ///     Called when entering this animation state
        /// </summary>
        void OnEnter(IAnimationContext context);

        /// <summary>
        ///     Called when exiting this animation state
        /// </summary>
        void OnExit(IAnimationContext context);

        /// <summary>
        ///     Called each frame while this state is active
        /// </summary>
        void OnUpdate(IAnimationContext context, float deltaTime);

        /// <summary>
        ///     Check available transitions and return the next state ID if a transition should occur
        /// </summary>
        /// <returns>ID of the next state, or null if no transition should occur</returns>
        string CheckTransitions(IAnimationContext context);
    }
}
