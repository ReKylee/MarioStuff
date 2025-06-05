namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Interface for animators that can be controlled by the animation flow system
    /// </summary>
    public interface IAnimator
    {
        /// <summary>
        ///     Plays an animation by name
        /// </summary>
        /// <param name="animationName">The name of the animation to play</param>
        /// <returns>True if the animation was found and played, false otherwise</returns>
        bool PlayAnimation(string animationName);

        /// <summary>
        ///     Gets the name of the currently playing animation
        /// </summary>
        /// <returns>The name of the current animation or empty if none is playing</returns>
        string GetCurrentAnimationName();

        /// <summary>
        ///     Checks if the current animation has finished playing
        /// </summary>
        /// <returns>True if the animation has finished, false otherwise</returns>
        bool IsAnimationFinished();

        /// <summary>
        ///     Gets the normalized time of the current animation (0-1)
        /// </summary>
        /// <returns>The normalized time of the current animation</returns>
        float GetAnimationNormalizedTime();

        /// <summary>
        ///     Checks if the specified animation exists
        /// </summary>
        /// <param name="animationName">The name of the animation to check</param>
        /// <returns>True if the animation exists, false otherwise</returns>
        bool HasAnimation(string animationName);
    }
}
