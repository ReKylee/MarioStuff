using System;
using System.Collections.Generic;
using GabrielBigardi.SpriteAnimator;

namespace Animation.Flow.Adapters
{
    /// <summary>
    ///     Adapter for the GabrielBigardi.SpriteAnimator to work with our animation flow system
    /// </summary>
    public class SpriteAnimatorAdapter : IAnimator
    {
        private readonly SpriteAnimator _animator;

        // Changed to support multiple actions per frame event
        private readonly Dictionary<string, Dictionary<int, List<Action>>> _frameEvents = new();

        public SpriteAnimatorAdapter(SpriteAnimator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
        }

        public void Play(string animationName)
        {
            _animator.Play(animationName);
        }

        public void SetLooping(bool shouldLoop)
        {
            if (_animator.CurrentAnimation != null)
            {
                _animator.CurrentAnimation.SpriteAnimationType =
                    shouldLoop ? SpriteAnimationType.Looping : SpriteAnimationType.PlayOnce;
            }
        }

        public void Pause()
        {
            _animator.Pause();
        }

        public void Resume()
        {
            _animator.Resume();
        }

        public void SetCurrentFrame(int frameIndex)
        {
            _animator.SetCurrentFrame(frameIndex);
        }

        public void RegisterAnimationCompleteCallback(Action callback)
        {
            _animator.OnAnimationComplete += callback;
        }

        public void UnregisterAnimationCompleteCallback(Action callback)
        {
            _animator.OnAnimationComplete -= callback;
        }

        public bool IsAnimationComplete => _animator.AnimationCompleted;

        public string CurrentAnimationName => _animator.CurrentAnimation?.Name;

        public void SetFrameEvent(string animationName, int frameIndex, Action callback)
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

        public void ClearFrameEvents(string animationName)
        {
            if (_frameEvents.ContainsKey(animationName))
            {
                _frameEvents.Remove(animationName);
                _animator.SetAnimationFrameEvents(animationName, null);
            }
        }

        private void ApplyFrameEvents(string animationName)
        {
            if (_frameEvents.TryGetValue(animationName, out var events))
            {
                // Create a copy of the events dictionary to pass to the animator
                // SpriteAnimator expects Dictionary<int, List<Action>>
                var spriteAnimatorEvents = new Dictionary<int, List<Action>>();
                foreach (var kvp in events)
                {
                    spriteAnimatorEvents[kvp.Key] = new List<Action>(kvp.Value); // Ensure a new list is created
                }

                _animator.SetAnimationFrameEvents(animationName, spriteAnimatorEvents);
            }
        }
    }
}
