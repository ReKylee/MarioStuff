using GabrielBigardi.SpriteAnimator;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Handles animation playback for Kirby
    /// </summary>
    public class KirbyAnimator
    {
        private readonly KirbyAnimationController _animationController;
        private readonly SpriteAnimator _animator;
        private readonly KirbyController _kirbyController;
        private readonly SpriteRenderer _spriteRenderer;
        private readonly AnimationStateTracker _stateTracker;

        public KirbyAnimator(
            SpriteAnimator animator,
            KirbyController kirbyController,
            AnimationStateTracker stateTracker,
            SpriteRenderer spriteRenderer,
            KirbyAnimationController animationController)
        {
            _animator = animator;
            _kirbyController = kirbyController;
            _stateTracker = stateTracker;
            _spriteRenderer = spriteRenderer;
            _animationController = animationController;

            if (_animator)
            {
                _animator.OnAnimationComplete += OnAnimationComplete;
            }
        }

        /// <summary>
        ///     Plays the animation for the given state
        /// </summary>
        public void PlayStateAnimation(AnimState state)
        {
            string animName = GetAnimationName(state);

            if (string.IsNullOrEmpty(animName))
            {
                Debug.LogWarning($"No animation for state: {state}");
                return;
            }

            bool shouldPlayAnimation = false;

            // For one-shot animations, always play them when the state changes
            if (IsOneShotAnimation(state) && state != _stateTracker.CurrentState)
            {
                shouldPlayAnimation = true;
            }
            // For other animations, only play if the animation name changed
            else if (_stateTracker.LastPlayedAnimation != animName)
            {
                shouldPlayAnimation = true;
            }

            // Play the animation if needed
            if (shouldPlayAnimation)
            {
                // Special handling for Jump_Full animation
                if (state == AnimState.JumpStart && animName == "Jump_Full")
                {
                    _animator.Play(animName);
                    _stateTracker.HasPlayedJumpFullAnimation = true;
                    Debug.Log("Playing Jump_Full animation");
                }
                // For Jump state with full, ensure we're using the appropriate animation
                else if (state == AnimState.Jump && _stateTracker.IsFull)
                {
                    // If we've already played Jump_Full, use the air version
                    string jumpAnimName = _stateTracker.HasPlayedJumpFullAnimation ? "Jump_Full_Air" : "Jump_Full";

                    // If we're playing Jump_Full for the first time, mark it
                    if (jumpAnimName == "Jump_Full")
                    {
                        _stateTracker.HasPlayedJumpFullAnimation = true;
                    }

                    _animator.Play(jumpAnimName);
                    Debug.Log($"Playing {jumpAnimName} animation");
                }
                // Play animation - use PlayIfNotPlaying for looping animations
                else if (IsOneShotAnimation(state))
                {
                    // For JumpToFly specifically, ensure it always plays from the beginning
                    if (state == AnimState.JumpToFly)
                    {
                        _animator.Play(animName);
                        Debug.Log($"Playing JumpToFly animation: {animName}");
                    }
                    // For BounceOffFloor, ensure it resets properly
                    else if (state == AnimState.BounceOffFloor)
                    {
                        _animator.Play(animName);
                        Debug.Log($"Playing BounceOffFloor animation: {animName}");
                    }
                    else
                    {
                        _animator.Play(animName);
                    }
                }
                else
                {
                    _animator.PlayIfNotPlaying(animName);
                }

                _animator.OnAnimationComplete += OnAnimationComplete;
                _stateTracker.LastPlayedAnimation = animName;
            }

            // Handle one-shot animations
            if (IsOneShotAnimation(state))
            {
                _stateTracker.WaitingForAnimComplete = true;

                // For BounceOffFloor, also set a maximum time to wait
                if (state == AnimState.BounceOffFloor)
                {
                    _stateTracker.StateTimer = 0f; // Reset state timer to track time in this state
                }
            }
        }

        /// <summary>
        ///     Updates the direction of the sprite based on input
        /// </summary>
        public void UpdateSpriteDirection(InputContext input, float moveInputThreshold)
        {
            float horizontalInput = Mathf.Abs(input.RunInput) > moveInputThreshold ? input.RunInput : input.WalkInput;

            if (Mathf.Abs(horizontalInput) > moveInputThreshold)
            {
                _stateTracker.LastNonZeroHorizontalInput = horizontalInput;
                _spriteRenderer.flipX = horizontalInput < 0;
            }
        }

        /// <summary>
        ///     Gets the facing direction (1 for right, -1 for left)
        /// </summary>
        public int GetFacingDirection() => _spriteRenderer.flipX ? -1 : 1;

        /// <summary>
        ///     Handles animation complete event
        /// </summary>
        private void OnAnimationComplete()
        {
            // Notify the animation controller that the current animation has completed
            // This is crucial for state transitions like JumpToFly -> Fly
            _animationController?.OnAnimationComplete();
        }

        /// <summary>
        ///     Cleans up event handlers
        /// </summary>
        public void Cleanup()
        {
            if (_animator)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
            }
        }

        /// <summary>
        ///     Gets the animation name for the given state
        /// </summary>
        private string GetAnimationName(AnimState state)
        {
            switch (state)
            {
                case AnimState.Idle:
                    return GetIdleAnimation();
                case AnimState.Walk:
                    return _stateTracker.IsFull ? "Run_Full" : "Walk";
                case AnimState.Run:
                    return _stateTracker.IsFull ? "Run_Full" : "Run";
                case AnimState.Crouch:
                    return GetCrouchAnimation();
                case AnimState.JumpStart:
                    return _stateTracker.IsFull ? "Jump_Full" : "JumpStart";
                case AnimState.Jump:
                    // Don't play Jump_Full for regular Jump state - use a different animation
                    return _stateTracker.IsFull ? "Jump_Full_Air" : "Jump";
                case AnimState.Fall:
                    return _stateTracker.IsFull ? "Fall_Full" : "Fall";
                case AnimState.BounceOffFloor:
                    return "BounceOffFloor";
                case AnimState.JumpToFly:
                    return "JumpToFly";
                case AnimState.Fly:
                    return "Fly";
                case AnimState.Float:
                    return "Float";
                case AnimState.Inhale:
                    return "Inhale";
                case AnimState.Spit:
                    return "Spit";
                case AnimState.Swallow:
                    return "Swallow";
                case AnimState.Skid:
                    return "Skid";
                default:
                    return "Idle";
            }
        }

        private string GetIdleAnimation()
        {
            string baseName = _stateTracker.IsFull ? "Idle_Full" : "Idle";

            switch (_kirbyController.GroundType)
            {
                case KirbyGroundCheck.SlopeType.Slope:
                    return baseName + (GetFacingDirection() > 0 ? "_SlopeR" : "_SlopeL");
                case KirbyGroundCheck.SlopeType.DeepSlope:
                    return baseName + (GetFacingDirection() > 0 ? "_DSlopeR" : "_DSlopeL");
                default:
                    return baseName;
            }
        }

        private string GetCrouchAnimation()
        {
            return _kirbyController.GroundType switch
            {
                KirbyGroundCheck.SlopeType.Slope => GetFacingDirection() > 0
                    ? "Idle_Squashed_SlopeR"
                    : "Idle_Squashed_SlopeL",
                KirbyGroundCheck.SlopeType.DeepSlope => GetFacingDirection() > 0
                    ? "Idle_Squashed_DSlopeR"
                    : "Idle_Squashed_DSlopeL",
                _ => "Idle_Crouched"
            };
        }

        private bool IsOneShotAnimation(AnimState state)
        {
            switch (state)
            {
                case AnimState.JumpStart:
                case AnimState.Jump:
                case AnimState.BounceOffFloor:
                case AnimState.JumpToFly:
                case AnimState.Inhale:
                case AnimState.Spit:
                case AnimState.Swallow:
                case AnimState.Skid:
                    return true;
                default:
                    return false;
            }
        }
    }
}
