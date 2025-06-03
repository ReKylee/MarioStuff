using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Interfaces;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Animation.Flow.Adapters
{
    /// <summary>
    ///     Adapter for the GabrielBigardi.SpriteAnimator to work with our animation flow system.
    /// </summary>
    public class SpriteAnimatorAdapter : IAnimator
    {
        // The sprite animator component this adapter wraps
        private readonly SpriteAnimator _animator;

        // Dictionary to store frame events by animation name
        private readonly Dictionary<string, Dictionary<int, List<Action>>> _frameEvents = new();

        // Cache for animation names to improve performance
        private List<string> _cachedAnimationNames;

        /// <summary>
        ///     Create a new adapter for the given SpriteAnimator component
        /// </summary>
        public SpriteAnimatorAdapter(SpriteAnimator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator),
                "SpriteAnimatorAdapter requires a valid SpriteAnimator component");
        }

        #region Helper Methods

        /// <summary>
        ///     Apply frame events to the sprite animator
        /// </summary>
        private void ApplyFrameEvents(string animationName)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName))
                return;

            try
            {
                if (_frameEvents.TryGetValue(animationName, out var events))
                {
                    // Create a copy of the events dictionary to pass to the animator
                    var spriteAnimatorEvents = new Dictionary<int, List<Action>>();
                    foreach (var kvp in events)
                    {
                        spriteAnimatorEvents[kvp.Key] = new List<Action>(kvp.Value);
                    }

                    _animator.SetAnimationFrameEvents(animationName, spriteAnimatorEvents);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to apply frame events for animation '{animationName}': {ex.Message}");
            }
        }

        #endregion

        #region IAnimator Implementation

        /// <summary>
        ///     Get the name of the currently playing animation
        /// </summary>
        public string CurrentAnimationName => _animator?.CurrentAnimation?.Name;

        /// <summary>
        ///     Check if the current animation has completed playing
        /// </summary>
        public bool IsAnimationComplete => _animator != null && _animator.AnimationCompleted;

        /// <summary>
        ///     Play the specified animation
        /// </summary>
        public void Play(string animationName)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName))
                return;

            try
            {
                _animator.Play(animationName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to play animation '{animationName}': {ex.Message}");
            }
        }

        /// <summary>
        ///     Set whether the current animation should loop
        /// </summary>
        public void SetLooping(bool shouldLoop)
        {
            if (_animator?.CurrentAnimation == null)
                return;

            try
            {
                _animator.CurrentAnimation.SpriteAnimationType =
                    shouldLoop ? SpriteAnimationType.Looping : SpriteAnimationType.PlayOnce;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set looping state: {ex.Message}");
            }
        }

        /// <summary>
        ///     Pause the current animation
        /// </summary>
        public void Pause()
        {
            if (_animator == null)
                return;

            try
            {
                _animator.Pause();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to pause animation: {ex.Message}");
            }
        }

        /// <summary>
        ///     Resume a paused animation
        /// </summary>
        public void Resume()
        {
            if (_animator == null)
                return;

            try
            {
                _animator.Resume();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to resume animation: {ex.Message}");
            }
        }

        /// <summary>
        ///     Set the current frame of the animation
        /// </summary>
        public void SetCurrentFrame(int frameIndex)
        {
            if (_animator == null)
                return;

            try
            {
                _animator.SetCurrentFrame(frameIndex);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set frame {frameIndex}: {ex.Message}");
            }
        }

        /// <summary>
        ///     Register a callback to be invoked when the current animation completes
        /// </summary>
        public void RegisterAnimationCompleteCallback(Action callback)
        {
            if (_animator == null || callback == null)
                return;

            try
            {
                _animator.OnAnimationComplete += callback;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to register animation complete callback: {ex.Message}");
            }
        }

        /// <summary>
        ///     Unregister a previously registered animation complete callback
        /// </summary>
        public void UnregisterAnimationCompleteCallback(Action callback)
        {
            if (_animator == null || callback == null)
                return;

            try
            {
                _animator.OnAnimationComplete -= callback;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unregister animation complete callback: {ex.Message}");
            }
        }

        /// <summary>
        ///     Get a list of all available animation names
        /// </summary>
        public List<string> GetAvailableAnimations()
        {
            // Return cached list if available
            if (_cachedAnimationNames != null)
                return _cachedAnimationNames;

            var animations = new List<string>();

            try
            {
                if (_animator?.SpriteAnimationObject?.SpriteAnimations != null)
                {
                    animations.AddRange(
                        _animator.SpriteAnimationObject.SpriteAnimations
                            .Where(anim => anim != null)
                            .Select(anim => anim.Name));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error getting animation list: {ex.Message}");
            }

            // Cache the result and return
            _cachedAnimationNames = animations;
            return animations;
        }

        /// <summary>
        ///     Set a frame event for a specific animation and frame
        /// </summary>
        public void SetFrameEvent(string animationName, int frameIndex, Action callback)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName) || callback == null)
                return;

            try
            {
                // Ensure we have a dictionary for this animation
                if (!_frameEvents.TryGetValue(animationName, out var animEvents))
                {
                    animEvents = new Dictionary<int, List<Action>>();
                    _frameEvents[animationName] = animEvents;
                }

                // Ensure we have a list for this frame index
                if (!animEvents.TryGetValue(frameIndex, out var frameActions))
                {
                    frameActions = new List<Action>();
                    animEvents[frameIndex] = frameActions;
                }

                // Add the callback if it's not already there
                if (!frameActions.Contains(callback))
                {
                    frameActions.Add(callback);
                }

                // Apply frame events to the animator
                ApplyFrameEvents(animationName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set frame event for animation '{animationName}': {ex.Message}");
            }
        }

        /// <summary>
        ///     Clear all frame events for an animation
        /// </summary>
        public void ClearFrameEvents(string animationName)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName))
                return;

            try
            {
                if (_frameEvents.ContainsKey(animationName))
                {
                    _frameEvents.Remove(animationName);
                    _animator.SetAnimationFrameEvents(animationName, null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to clear frame events for animation '{animationName}': {ex.Message}");
            }
        }

        /// <summary>
        ///     Get the total frame count of an animation
        /// </summary>
        public int GetFrameCount(string animationName)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName))
                return 0;

            try
            {
                return _animator.SpriteAnimationObject?.SpriteAnimations
                    .FirstOrDefault(anim => anim != null && anim.Name == animationName)?.Frames.Count ?? 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get frame count for animation '{animationName}': {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        ///     Refresh any cached animation data
        /// </summary>
        public void RefreshAnimationCache()
        {
            _cachedAnimationNames = null;
        }

        /// <summary>
        ///     Check if an animation with the given name exists
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            if (_animator == null || string.IsNullOrEmpty(animationName))
                return false;

            try
            {
                var animations = GetAvailableAnimations();
                return animations.Contains(animationName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to check if animation '{animationName}' exists: {ex.Message}");
                return false;
            }
        }

        #endregion

    }
}
