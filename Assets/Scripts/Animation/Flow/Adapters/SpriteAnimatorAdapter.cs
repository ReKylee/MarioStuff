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
        private readonly Dictionary<string, Dictionary<int, Action>> _frameEvents = new();

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
            if (!_frameEvents.TryGetValue(animationName, out var animationEvents))
            {
                animationEvents = new Dictionary<int, Action>();
                _frameEvents[animationName] = animationEvents;
            }

            animationEvents[frameIndex] = callback;

            // Convert our frame events dictionary to the format SpriteAnimator expects
            var spriteAnimatorEvents = new Dictionary<int, List<Action>>();
            foreach (var frameEvent in animationEvents)
            {
                spriteAnimatorEvents[frameEvent.Key] = new List<Action> { frameEvent.Value };
            }

            _animator.SetAnimationFrameEvents(animationName, spriteAnimatorEvents);
        }

        public void ClearFrameEvents(string animationName)
        {
            if (_frameEvents.ContainsKey(animationName))
            {
                _frameEvents.Remove(animationName);
            }

            _animator.SetAnimationFrameEvents(animationName, null);
        }
    }
}
