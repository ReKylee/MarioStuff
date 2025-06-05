using Animation.Flow.Interfaces;
using GabrielBigardi.SpriteAnimator;

namespace Animation.Flow.Adapters
{
    /// <summary>
    ///     Adapter for the SpriteAnimator from GabrielBigardi.SpriteAnimator
    ///     Implements the IAnimator interface for use with the animation flow system
    /// </summary>
    public class SpriteAnimatorAdapter : IAnimator
    {
        private readonly SpriteAnimator _spriteAnimator;

        /// <summary>
        ///     Creates a new adapter for the specified sprite animator
        /// </summary>
        /// <param name="spriteAnimator">The sprite animator to adapt</param>
        public SpriteAnimatorAdapter(SpriteAnimator spriteAnimator)
        {
            _spriteAnimator = spriteAnimator;
        }

        /// <summary>
        ///     Plays an animation by name
        /// </summary>
        public bool PlayAnimation(string animationName)
        {
            if (_spriteAnimator == null || !_spriteAnimator.HasAnimation(animationName))
            {
                return false;
            }

            // Only play if it's different from the current animation
            if (_spriteAnimator.CurrentAnimation.Name != animationName)
            {
                _spriteAnimator.Play(animationName);
            }

            return true;
        }

        /// <summary>
        ///     Gets the name of the currently playing animation
        /// </summary>
        public string GetCurrentAnimationName() => _spriteAnimator?.CurrentAnimation.Name ?? string.Empty;

        /// <summary>
        ///     Checks if the current animation has finished playing
        /// </summary>
        public bool IsAnimationFinished()
        {
            // For SpriteAnimator, we need to check if the animation is on its last frame
            // and not looping, or if the animation is complete
            if (_spriteAnimator == null || _spriteAnimator.CurrentAnimation == null)
            {
                return true;
            }

            return _spriteAnimator.AnimationCompleted &&
                   _spriteAnimator.CurrentAnimation.SpriteAnimationType != SpriteAnimationType.Looping;
        }

        /// <summary>
        ///     Gets the normalized time of the current animation (0-1)
        /// </summary>
        public float GetAnimationNormalizedTime()
        {
            if (!_spriteAnimator || _spriteAnimator.CurrentAnimation == null)
            {
                return 0f;
            }

            // Calculate normalized time based on current frame and total frames
            int totalFrames = _spriteAnimator.CurrentAnimation.Frames.Count;
            if (totalFrames <= 1)
            {
                return 1f;
            }

            return (float)_spriteAnimator.CurrentFrame / (totalFrames - 1);
        }

        /// <summary>
        ///     Checks if the specified animation exists
        /// </summary>
        public bool HasAnimation(string animationName) =>
            _spriteAnimator != null && _spriteAnimator.HasAnimation(animationName);
    }
}
