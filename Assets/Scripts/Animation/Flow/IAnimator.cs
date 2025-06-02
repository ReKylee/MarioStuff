using System;

namespace Animation.Flow
{
    /// <summary>
    ///     Adapter interface to abstract animation system implementations
    ///     This allows our flow system to work with any animation implementation, not just SpriteAnimator
    /// </summary>
    public interface IAnimator
    {

        /// <summary>
        ///     Get whether the current animation has finished playing
        /// </summary>
        bool IsAnimationComplete { get; }

        /// <summary>
        ///     Get the name of the current animation
        /// </summary>
        string CurrentAnimationName { get; }

        /// <summary>
        ///     Play the specified animation
        /// </summary>
        void Play(string animationName);

        /// <summary>
        ///     Set whether the current animation should loop
        /// </summary>
        void SetLooping(bool shouldLoop);

        /// <summary>
        ///     Pause the current animation
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resume the current animation if paused
        /// </summary>
        void Resume();

        /// <summary>
        ///     Set the current frame index
        /// </summary>
        void SetCurrentFrame(int frameIndex);

        /// <summary>
        ///     Register a callback for when the current animation completes
        /// </summary>
        void RegisterAnimationCompleteCallback(Action callback);

        /// <summary>
        ///     Unregister a callback for animation completion
        /// </summary>
        void UnregisterAnimationCompleteCallback(Action callback);

        /// <summary>
        ///     Set a frame event for a specific animation
        /// </summary>
        void SetFrameEvent(string animationName, int frameIndex, Action callback);

        /// <summary>
        ///     Clear frame events for an animation
        /// </summary>
        void ClearFrameEvents(string animationName);
    }
}
