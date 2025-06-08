using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Handles physics interactions for Kirby's animations
    /// </summary>
    public class KirbyPhysicsController
    {
        private readonly KirbyController _kirbyController;
        private readonly AnimationSettings _settings;
        private readonly AnimationStateTracker _stateTracker;

        public KirbyPhysicsController(
            KirbyController kirbyController,
            AnimationStateTracker stateTracker,
            AnimationSettings settings)
        {
            _kirbyController = kirbyController;
            _stateTracker = stateTracker;
            _settings = settings;
        }

        /// <summary>
        ///     Tracks Kirby's ground state changes
        /// </summary>
        public void TrackGroundState(Transform transform)
        {
            // Track ground state changes
            if (_kirbyController.IsGrounded && !_stateTracker.WasGrounded)
            {
                OnLanded();
            }
            else if (!_kirbyController.IsGrounded && _stateTracker.WasGrounded)
            {
                OnLeftGround(transform.position.y);
            }

            _stateTracker.WasGrounded = _kirbyController.IsGrounded;

            // Track jump height when not grounded
            if (!_kirbyController.IsGrounded)
            {
                _stateTracker.UpdateJumpHeightTracking(transform.position.y);
            }
        }

        /// <summary>
        ///     Tracks Kirby's vertical state (apex, falling)
        /// </summary>
        public void TrackVerticalState(Transform transform)
        {
            if (!_kirbyController.IsGrounded)
            {
                float verticalVelocity = _kirbyController.Velocity.y;

                // Jump apex detection
                if (Mathf.Approximately(verticalVelocity, 0) &&
                    !_stateTracker.IsJumpApex &&
                    !_stateTracker.HasStartedFallingThisJump)
                {
                    _stateTracker.IsJumpApex = true;
                    _stateTracker.ApexTimer = 0f;

                    // If in JumpStart and we've reached apex, transition to Jump
                    if (_stateTracker.CurrentState == AnimState.JumpStart)
                    {
                        _stateTracker.CanDoubleJump = true;
                        _stateTracker.ChangeState(AnimState.Jump);
                    }
                }
                // Track falling
                else if (verticalVelocity < -0.1f)
                {
                    if (!_stateTracker.HasStartedFallingThisJump)
                    {
                        _stateTracker.HasStartedFallingThisJump = true;

                        // If still in JumpStart and starting to fall, force transition to Jump
                        if (_stateTracker.CurrentState == AnimState.JumpStart)
                        {
                            _stateTracker.CanDoubleJump = true;
                            _stateTracker.ChangeState(AnimState.Jump);
                        }
                    }

                    _stateTracker.FallTimer += Time.deltaTime;

                    float fallHeight = _stateTracker.LastGroundedY - transform.position.y;
                    if ((_stateTracker.FallTimer > _settings.fallTimeBeforeBounce ||
                         fallHeight > _settings.bounceHeightThreshold) &&
                        !_stateTracker.PreloadingBounceAnimation &&
                        !_stateTracker.IsFull) // Only allow bounce if not full
                    {
                        _stateTracker.ShouldBounce = true;

                        // Check if we're close enough to the ground to start bounce animation
                        RaycastHit2D hit = Physics2D.Raycast(
                            transform.position,
                            Vector2.down,
                            _settings.preFloorBounceDistance,
                            _kirbyController.GroundLayers);

                        if (hit.collider != null && _stateTracker.CurrentState != AnimState.BounceOffFloor)
                        {
                            _stateTracker.PreloadingBounceAnimation = true;
                            _stateTracker.ChangeState(AnimState.BounceOffFloor);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Applies physics forces for certain animation states
        /// </summary>
        public void ApplyAnimationPhysics()
        {
            // Reset bounce force flag when not in bounce state
            if (_stateTracker.CurrentState != AnimState.BounceOffFloor)
            {
                _stateTracker.HasBounceForceBeenApplied = false;
            }
            else if (_kirbyController.Rigidbody)
            {
                ApplyBounceForce();
            }
        }

        /// <summary>
        ///     Apply bounce force when in bounce state
        /// </summary>
        private void ApplyBounceForce()
        {
            // Only apply bounce force once per bounce animation
            if (!_stateTracker.HasBounceForceBeenApplied && _stateTracker.StateTimer < 0.1f)
            {
                Debug.Log("Applying bounce force");
                Vector2 bounceVelocity = new(_kirbyController.Velocity.x, _settings.bounceForce);
                _kirbyController.Rigidbody.linearVelocity = bounceVelocity;
                _stateTracker.HasBounceForceBeenApplied = true;
            }
        }

        /// <summary>
        ///     Handle what happens when Kirby lands on the ground
        /// </summary>
        private void OnLanded()
        {
            _stateTracker.OnLanded();
        }

        /// <summary>
        ///     Handle what happens when Kirby leaves the ground
        /// </summary>
        private void OnLeftGround(float positionY)
        {
            _stateTracker.OnLeftGround(positionY);
        }
    }
}
