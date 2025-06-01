using System.Collections.Generic;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Kirby.Core.Components
{
    /// <summary>
    ///     Animation controller that uses a simple priority system
    /// </summary>
    public class AnimationPriorityController
    {
        private readonly SpriteAnimator _animator;
        private readonly HashSet<string> _availableAnimations = new();
        private float _highestPriority = float.MinValue;
        private string _highestPriorityAnimation;

        public AnimationPriorityController(SpriteAnimator animator)
        {
            _animator = animator;
            RefreshAvailableAnimations();
        }

        /// <summary>
        ///     Request an animation to be played with a certain priority
        /// </summary>
        public bool PlayAnimation(string animationName, float priority)
        {
            // Check if animation exists
            if (!AnimationExists(animationName))
            {
                return false;
            }

            // Check if this animation has higher priority than the current highest
            if (priority > _highestPriority)
            {
                _highestPriority = priority;
                _highestPriorityAnimation = animationName;
            }

            return true;
        }

        /// <summary>
        ///     Apply the highest priority animation this frame and reset for next frame
        ///     Call this at the end of FixedUpdate
        /// </summary>
        public void ApplyAnimationForThisFrame()
        {
            // If we have a highest priority animation and it's different from what's playing
            if (_highestPriorityAnimation != null)
            {
                _animator.PlayIfNotPlaying(_highestPriorityAnimation);
            }

            // Reset for next frame
            _highestPriority = float.MinValue;
            _highestPriorityAnimation = null;
        }

        /// <summary>
        ///     Check if an animation exists
        /// </summary>
        public bool AnimationExists(string animationName) => _availableAnimations.Contains(animationName);

        /// <summary>
        ///     Flip the sprite based on direction
        /// </summary>
        public void SetDirection(int direction)
        {
            if (direction == 0 || _animator == null)
                return;

            if (Mathf.RoundToInt(_animator.transform.localScale.x) != direction)
            {
                Vector3 scale = _animator.transform.localScale;
                scale.x = direction;
                _animator.transform.localScale = scale;
            }
        }

        /// <summary>
        ///     Refresh the list of available animations
        /// </summary>
        public void RefreshAvailableAnimations()
        {
            if (_animator == null)
                return;

            _availableAnimations.Clear();

            foreach (SpriteAnimation anim in _animator.SpriteAnimationObject.SpriteAnimations)
            {
                _availableAnimations.Add(anim.Name);
            }
        }
    }
}
