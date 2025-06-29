using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Implements animation state machine logic for Kirby
    /// </summary>
    public class KirbyAnimationStateMachine : IAnimationStateMachine
    {
        private readonly KirbyAnimator _animator;
        private readonly LayerMask _groundLayers;
        private readonly KirbyController _kirbyController;
        private readonly AnimationSettings _settings;
        private readonly AnimationStateTracker _stateTracker;

        public KirbyAnimationStateMachine(
            AnimationStateTracker stateTracker,
            KirbyController kirbyController,
            KirbyAnimator animator,
            AnimationSettings settings,
            LayerMask groundLayers)
        {
            _stateTracker = stateTracker;
            _kirbyController = kirbyController;
            _animator = animator;
            _settings = settings;
            _groundLayers = groundLayers;
        }

        /// <summary>
        ///     Gets the current animation state
        /// </summary>
        public AnimState GetCurrentState() => _stateTracker.CurrentState;

        /// <summary>
        ///     Updates the current animation state based on input and character state
        /// </summary>
        public void UpdateState(InputContext input, bool isGrounded, Vector2 velocity)
        {
            // Don't update if waiting for animation to complete
            if (_stateTracker.WaitingForAnimComplete)
            {
                HandleWaitingForAnimationComplete(input, isGrounded);
                return;
            }

            // Get movement input
            float horizontalInput = Mathf.Abs(input.RunInput) > _settings.moveInputThreshold
                ? input.RunInput
                : input.WalkInput;

            float absHorizontal = Mathf.Abs(horizontalInput);
            bool isMoving = absHorizontal > _settings.moveInputThreshold;
            bool isRunning = Mathf.Abs(input.RunInput) > _settings.runInputThreshold;

            // Update state based on current conditions
            switch (_stateTracker.CurrentState)
            {
                case AnimState.Idle:
                    UpdateIdleState(input, isGrounded, isMoving, isRunning);
                    break;

                case AnimState.Walk:
                    UpdateWalkState(input, isGrounded, isMoving, isRunning);
                    break;

                case AnimState.Run:
                    UpdateRunState(input, isGrounded, isMoving);
                    break;

                case AnimState.Crouch:
                    UpdateCrouchState(input);
                    break;

                case AnimState.JumpStart:
                    // Reset jump animation tracking when starting a new jump
                    _stateTracker.HasPlayedJumpFullAnimation = false;
                    UpdateJumpStartState(input, velocity.y);
                    break;

                case AnimState.Jump:
                    UpdateJumpState(input, velocity.y);
                    break;

                case AnimState.Fall:
                    UpdateFallState(input, isGrounded);
                    break;

                case AnimState.BounceOffFloor:
                    UpdateBounceState(input);
                    break;

                case AnimState.JumpToFly:
                    UpdateJumpToFlyState(input);
                    break;

                case AnimState.Fly:
                    UpdateFlyState(input, isGrounded);
                    break;

                case AnimState.Float:
                    UpdateFloatState(input, isGrounded);
                    break;

                case AnimState.Inhale:
                    UpdateInhaleState(input);
                    break;

                case AnimState.Spit:
                case AnimState.Swallow:
                    // These states are handled in OnAnimationComplete
                    break;
            }
        }

        /// <summary>
        ///     Handles animation complete events
        /// </summary>
        public void OnAnimationComplete()
        {
            _stateTracker.WaitingForAnimComplete = false;

            // Handle state transitions after animation completes
            switch (_stateTracker.CurrentState)
            {
                case AnimState.JumpStart:
                    _stateTracker.CanDoubleJump = true;
                    _stateTracker.ChangeState(AnimState.Jump);
                    break;

                case AnimState.Jump:
                    // Transition to Fall when Jump animation completes
                    _stateTracker.ChangeState(AnimState.Fall);
                    break;

                case AnimState.BounceOffFloor:
                    _stateTracker.HasBouncedThisLanding = true;
                    _stateTracker.ShouldBounce = false;
                    _stateTracker.CanDoubleJump = true;
                    _stateTracker.PreloadingBounceAnimation = false;

                    // After bouncing, check if player wants to fly (only if not full)
                    if (_kirbyController.CurrentInput.JumpPressed && !_stateTracker.IsFull)
                    {
                        _stateTracker.ChangeState(AnimState.JumpToFly);
                    }
                    else if (_kirbyController.IsGrounded)
                    {
                        // If we're on the ground, go to idle state
                        _stateTracker.ChangeState(AnimState.Idle);
                    }
                    else
                    {
                        // If we're in the air, go to fall state
                        _stateTracker.ChangeState(AnimState.Fall);
                    }

                    break;

                case AnimState.JumpToFly:
                    Debug.Log("JumpToFly animation complete - transitioning to Fly state");
                    _stateTracker.IsFlying = true;
                    _stateTracker.IsFloating = false;
                    _stateTracker.IsFull = true; // Set to full when transitioning to Fly
                    _stateTracker.ChangeState(AnimState.Fly);
                    break;

                case AnimState.Inhale:
                    if (_kirbyController.CurrentInput.AttackHeld)
                    {
                        // If still holding attack, keep the Inhale state going
                        _stateTracker.IsInhaling = true;
                        _animator.PlayStateAnimation(_stateTracker.CurrentState); // Replay the animation
                    }
                    else
                    {
                        // If not holding attack, become full and return to appropriate state
                        _stateTracker.IsInhaling = false;
                        _stateTracker.IsFull = true;
                        _stateTracker.WaitingForAnimComplete = false; // Ensure we don't wait for another animation

                        // Also immediately stop bounce animation if it's playing
                        if (_stateTracker.CurrentState == AnimState.BounceOffFloor)
                        {
                            _stateTracker.HasBouncedThisLanding = true;
                            _stateTracker.ShouldBounce = false;
                            _stateTracker.PreloadingBounceAnimation = false;
                        }

                        if (_kirbyController.IsGrounded)
                        {
                            _stateTracker.ChangeState(AnimState.Idle);
                        }
                        else
                        {
                            _stateTracker.ChangeState(AnimState.Fall);
                        }
                    }

                    break;

                case AnimState.Spit:
                    _stateTracker.IsFull = false;
                    _stateTracker.IsFlying = false;
                    _stateTracker.IsFloating = false;

                    // Transition to appropriate state based on ground status
                    if (_kirbyController.IsGrounded)
                    {
                        _stateTracker.ChangeState(AnimState.Idle);
                    }
                    else
                    {
                        _stateTracker.ChangeState(AnimState.Fall);
                    }

                    break;

                case AnimState.Swallow:
                    _stateTracker.IsFull = false;
                    _stateTracker.ChangeState(AnimState.Idle);
                    break;

                case AnimState.Skid:
                    _stateTracker.ChangeState(AnimState.Idle);
                    break;
            }
        }

        private void HandleWaitingForAnimationComplete(InputContext input, bool isGrounded)
        {
            // Handle BounceOffFloor state specifically
            if (_stateTracker.CurrentState == AnimState.BounceOffFloor)
            {
                // Allow jump to fly transition immediately (only if not full)
                if (input.JumpPressed && !_stateTracker.IsFull)
                {
                    _stateTracker.WaitingForAnimComplete = false;
                    _stateTracker.HasBouncedThisLanding = true;
                    _stateTracker.ShouldBounce = false;
                    _stateTracker.ChangeState(AnimState.JumpToFly);
                    return;
                }

                // If grounded, allow transition to idle
                if (isGrounded && _stateTracker.StateTimer > 0.3f)
                {
                    _stateTracker.WaitingForAnimComplete = false;
                    _stateTracker.HasBouncedThisLanding = true;
                    _stateTracker.ShouldBounce = false;
                    _stateTracker.ChangeState(AnimState.Idle);
                    return;
                }

                // Force exit bounce after a timeout
                if (_stateTracker.StateTimer > 0.5f)
                {
                    _stateTracker.WaitingForAnimComplete = false;
                    _stateTracker.HasBouncedThisLanding = true;
                    _stateTracker.ShouldBounce = false;
                    _stateTracker.ChangeState(isGrounded ? AnimState.Idle : AnimState.Fall);
                    return;
                }
            }

            // Allow transition to JumpToFly from ANY jump-related animation if jump is pressed and not full
            if (input.JumpPressed && IsJumpRelatedState(_stateTracker.CurrentState) && !_stateTracker.IsFull)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.ChangeState(AnimState.JumpToFly);
                return;
            }

            // Only allow ground landing to interrupt certain animations
            if (isGrounded && CanInterruptWithGroundLanding(_stateTracker.CurrentState))
            {
                _stateTracker.WaitingForAnimComplete = false;

                if (_stateTracker.ShouldBounce && !_stateTracker.HasBouncedThisLanding)
                {
                    _stateTracker.ChangeState(AnimState.BounceOffFloor);
                }
                else
                {
                    _stateTracker.ChangeState(AnimState.Idle);
                }
            }
            // Allow spitting to interrupt flying
            else if (_stateTracker.CurrentState == AnimState.Fly && input.AttackPressed)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.ChangeState(AnimState.Spit);
            }
            // Allow flapping to interrupt floating
            else if (_stateTracker.CurrentState == AnimState.Float && input.JumpPressed)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.ChangeState(AnimState.Fly);
            }
        }

        private bool IsJumpRelatedState(AnimState state) =>
            state is AnimState.JumpStart or AnimState.Jump or AnimState.Fall or AnimState.BounceOffFloor;

        private bool CanInterruptWithGroundLanding(AnimState state) =>
            state is AnimState.Jump or AnimState.Fall or AnimState.JumpToFly or AnimState.BounceOffFloor;

        #region State Update Methods

        private void UpdateIdleState(InputContext input, bool isGrounded, bool isMoving, bool isRunning)
        {
            if (input.CrouchPressed && isGrounded)
            {
                _stateTracker.ChangeState(AnimState.Crouch);
            }
            else if (isMoving && isGrounded)
            {
                _stateTracker.ChangeState(isRunning && !_stateTracker.IsFull ? AnimState.Run : AnimState.Walk);
            }
            else if (input.JumpPressed && isGrounded)
            {
                _stateTracker.ChangeState(AnimState.JumpStart);
            }
            else if (!isGrounded)
            {
                _stateTracker.ChangeState(AnimState.Fall);
            }
            else if (input.AttackPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Inhale);
                _stateTracker.IsInhaling = true;
                _stateTracker.InhaleTimer = 0f;
            }
            else if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Spit);
            }
        }

        private void UpdateWalkState(InputContext input, bool isGrounded, bool isMoving, bool isRunning)
        {
            if (!isMoving)
            {
                _stateTracker.ChangeState(AnimState.Idle);
            }
            else if (isRunning && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Run);
            }
            else if (input.JumpPressed && isGrounded)
            {
                _stateTracker.ChangeState(AnimState.JumpStart);
            }
            else if (!isGrounded)
            {
                _stateTracker.ChangeState(AnimState.Fall);
            }
            else if (input.AttackPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Inhale);
                _stateTracker.IsInhaling = true;
                _stateTracker.InhaleTimer = 0f;
            }
            else if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Spit);
            }
            else if (input.CrouchPressed)
            {
                _stateTracker.ChangeState(AnimState.Crouch);
            }
        }

        private void UpdateRunState(InputContext input, bool isGrounded, bool isMoving)
        {
            if (!isMoving)
            {
                _stateTracker.ChangeState(AnimState.Idle);
            }
            else if (input.JumpPressed && isGrounded)
            {
                _stateTracker.ChangeState(AnimState.JumpStart);
            }
            else if (!isGrounded)
            {
                _stateTracker.ChangeState(AnimState.Fall);
            }
            else if (input.AttackPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Inhale);
                _stateTracker.IsInhaling = true;
                _stateTracker.InhaleTimer = 0f;
            }
            else if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Spit);
            }
            else if (input.CrouchPressed)
            {
                _stateTracker.ChangeState(AnimState.Crouch);
            }
        }

        private void UpdateCrouchState(InputContext input)
        {
            if (!input.CrouchPressed)
            {
                _stateTracker.ChangeState(AnimState.Idle);
            }
            else if (_stateTracker.IsFull && input.AttackPressed)
            {
                _stateTracker.ChangeState(AnimState.Swallow);
            }
        }

        private void UpdateJumpStartState(InputContext input, float velocity)
        {
            // Force immediate transition to JumpToFly when jump is pressed (only if not full)
            if (input.JumpPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.JumpToFly);
                return;
            }

            // Transition to Jump state when velocity starts to decrease or jump button is released
            if (velocity <= 0.1f || input.JumpReleased)
            {
                _stateTracker.CanDoubleJump = true;
                _stateTracker.ChangeState(AnimState.Jump);
            }
        }

        private void UpdateJumpState(InputContext input, float velocity)
        {
            // Allow fly transition at ANY point during jump if jump is pressed (only if not full)
            if (input.JumpPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.JumpToFly);
                return;
            }

            // Immediately transition to Fall state when velocity becomes negative
            if (velocity < 0)
            {
                _stateTracker.ChangeState(AnimState.Fall);
                return;
            }

            // Inhale during jump
            if (input.AttackPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Inhale);
                _stateTracker.IsInhaling = true;
                _stateTracker.InhaleTimer = 0f;
                return;
            }

            // Spit if full
            if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Spit);
            }
        }

        private void UpdateFallState(InputContext input, bool isGrounded)
        {
            // Allow fly transition at ANY point during fall if jump is pressed (only if not full)
            if (input.JumpPressed && !_stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.JumpToFly);
                return;
            }

            // Handle landing
            if (isGrounded)
            {
                // Don't bounce if flying/floating or if full
                if (_stateTracker.ShouldBounce && !_stateTracker.HasBouncedThisLanding &&
                    !_stateTracker.IsFlying && !_stateTracker.IsFloating && !_stateTracker.IsFull)
                {
                    _stateTracker.ChangeState(AnimState.BounceOffFloor);
                }
                else
                {
                    _stateTracker.ChangeState(AnimState.Idle);
                }

                return;
            }

            // Inhale during fall
            if (input.AttackPressed && !_stateTracker.IsFull && !_stateTracker.IsInhaling)
            {
                _stateTracker.ChangeState(AnimState.Inhale);
                _stateTracker.IsInhaling = true;
                _stateTracker.InhaleTimer = 0f;
                return;
            }

            // Spit if full
            if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.ChangeState(AnimState.Spit);
            }
        }

        private void UpdateBounceState(InputContext input)
        {
            _stateTracker.HasPlayedJumpFullAnimation = false;
            // Allow transition to fly anytime during bounce (only if not full)
            if (input.JumpPressed && !_stateTracker.IsFull)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.ChangeState(AnimState.JumpToFly);
                return;
            }

            // Force exit from bounce state after a reasonable time
            if (_stateTracker.StateTimer > 0.5f)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.HasBouncedThisLanding = true;
                _stateTracker.ShouldBounce = false;

                if (_kirbyController.IsGrounded)
                {
                    _stateTracker.ChangeState(AnimState.Idle);
                }
                else
                {
                    _stateTracker.ChangeState(AnimState.Fall);
                }
            }
        }

        /// <summary>
        ///     Handles the transition from jump to fly state
        ///     This method manages the JumpToFly animation state, which is the visual
        ///     transition between jumping/falling and starting to fly
        /// </summary>
        private void UpdateJumpToFlyState(InputContext input)
        {
            // Set the flying flag immediately to ensure the physics module is aware
            _stateTracker.IsFlying = true;

            // Allow immediate transition to spit if attack is pressed during transition
            if (input.AttackPressed && _stateTracker.IsFull)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.ChangeState(AnimState.Spit);
                return;
            }

            // If we hit the ground during the transition animation, go to idle
            if (_kirbyController.IsGrounded)
            {
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.IsFlying = false;
                _stateTracker.ChangeState(AnimState.Idle);
                return;
            }

            // If the animation has been playing for more than 0.4 seconds, force transition to Fly state
            // This prevents getting stuck if the animation complete event doesn't fire
            if (_stateTracker.StateTimer > 0.4f)
            {
                Debug.Log("JumpToFly timeout - forcing transition to Fly state");
                _stateTracker.WaitingForAnimComplete = false;
                _stateTracker.IsFull = true; // Set to full when transitioning to Fly
                _stateTracker.ChangeState(AnimState.Fly);
                return;
            }

            // Wait for animation to complete otherwise
            // The state machine will transition to Fly in OnAnimationComplete
            _stateTracker.WaitingForAnimComplete = true;
        }

        private void UpdateFlyState(InputContext input, bool isGrounded)
        {
            // Set isFull when flying - Kirby should be full when flying
            if (!_stateTracker.IsFull)
            {
                _stateTracker.IsFull = true;
            }

            if (isGrounded)
            {
                _stateTracker.IsFlying = false;
                _stateTracker.IsFloating = false;
                _stateTracker.ChangeState(AnimState.Idle);
                return;
            }

            if (!input.JumpHeld && _stateTracker.StateTimer > 0.2f)
            {
                _stateTracker.IsFlying = true;
                _stateTracker.IsFloating = true;
                _stateTracker.ChangeState(AnimState.Float);
                return;
            }

            if (input.AttackPressed)
            {
                // Transition directly to Spit instead of using Exhale
                _stateTracker.ChangeState(AnimState.Spit);
            }
        }

        private void UpdateFloatState(InputContext input, bool isGrounded)
        {
            // Set isFull when floating - Kirby should be full when floating
            if (!_stateTracker.IsFull)
            {
                _stateTracker.IsFull = true;
            }

            if (isGrounded)
            {
                _stateTracker.IsFlying = false;
                _stateTracker.IsFloating = false;
                _stateTracker.ChangeState(AnimState.Idle);
                return;
            }

            if (input.JumpPressed)
            {
                _stateTracker.IsFlying = true;
                _stateTracker.IsFloating = false;
                _stateTracker.ChangeState(AnimState.Fly);
                return;
            }

            if (input.AttackPressed)
            {
                // Transition directly to Spit instead of using Exhale
                _stateTracker.ChangeState(AnimState.Spit);
            }
        }

        private void UpdateInhaleState(InputContext input)
        {
            // Continue inhaling as long as attack is held
            if (input.AttackHeld)
            {
                // Just continue the Inhale state
                return;
            }

            // When releasing attack button during inhale
            _stateTracker.IsInhaling = false;
            _stateTracker.IsFull = true; // Become full when finishing inhale
            _stateTracker.WaitingForAnimComplete = false; // Force animation state to change immediately

            // Return to appropriate state based on ground status
            if (_kirbyController.IsGrounded)
            {
                _stateTracker.ChangeState(AnimState.Idle);
            }
            else
            {
                _stateTracker.ChangeState(AnimState.Fall);
            }
        }

        #endregion

    }
}
