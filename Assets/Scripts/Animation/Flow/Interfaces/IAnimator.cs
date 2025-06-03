using System;
using System.Collections.Generic;

namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Core interface for animation control in the Animation Flow system.
    ///     Provides a common abstraction layer for any animation implementation.
    /// </summary>
    public interface IAnimator
    {
        /// <summary>
        ///     Gets the name of the currently playing animation
        /// </summary>
        string CurrentAnimationName { get; }

        /// <summary>
        ///     Gets whether the current animation has completed playing
        /// </summary>
        bool IsAnimationComplete { get; }

        /// <summary>
        ///     Play the specified animation
        /// </summary>
        /// <param name="animationName">Name of the animation to play</param>
        void Play(string animationName);

        /// <summary>
        ///     Set whether the current animation should loop
        /// </summary>
        /// <param name="shouldLoop">Whether the animation should loop</param>
        void SetLooping(bool shouldLoop);

        /// <summary>
        ///     Pause the current animation
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resume a paused animation
        /// </summary>
        void Resume();

        /// <summary>
        ///     Set the current frame of the animation
        /// </summary>
        /// <param name="frameIndex">The frame index to set</param>
        void SetCurrentFrame(int frameIndex);

        /// <summary>
        ///     Register a callback to be invoked when the current animation completes
        /// </summary>
        /// <param name="callback">The callback to invoke</param>
        void RegisterAnimationCompleteCallback(Action callback);

        /// <summary>
        ///     Unregister a previously registered animation complete callback
        /// </summary>
        /// <param name="callback">The callback to unregister</param>
        void UnregisterAnimationCompleteCallback(Action callback);

        /// <summary>
        ///     Get a list of all available animation names
        /// </summary>
        /// <returns>List of animation names that can be played</returns>
        List<string> GetAvailableAnimations();

        /// <summary>
        ///     Set a frame event for a specific animation and frame
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="frameIndex">Frame index to trigger the event</param>
        /// <param name="callback">Callback to invoke when the frame is reached</param>
        void SetFrameEvent(string animationName, int frameIndex, Action callback);

        /// <summary>
        ///     Clear all frame events for an animation
        /// </summary>
        /// <param name="animationName">Name of the animation to clear events for</param>
        void ClearFrameEvents(string animationName);

        /// <summary>
        ///     Get the total frame count of an animation
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <returns>Number of frames in the animation, or 0 if not found</returns>
        int GetFrameCount(string animationName);

        /// <summary>
        ///     Refresh any cached animation data
        /// </summary>
        void RefreshAnimationCache();

        /// <summary>
        ///     Check if an animation with the given name exists
        /// </summary>
        /// <param name="animationName">Animation name to check</param>
        /// <returns>True if the animation exists</returns>
        bool HasAnimation(string animationName);
    }
}
