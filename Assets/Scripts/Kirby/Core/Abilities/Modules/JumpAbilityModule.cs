using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Jump ability for Kirby - handles jumping mechanics including variable height jumps
    /// </summary>
    public class JumpAbilityModule : AbilityModuleBase, IMovementAbilityModule // Implement IMovementAbility
    {
        [Header("Jump Ability Settings")] [SerializeField]
        private bool allowDoubleJump = true;

        [SerializeField]
        private float doubleJumpForce = 80f; // This could be a StatType if desired for global modification

        [SerializeField] private float maxJumpHoldTime = 0.3f;

        // Default values for stats that this ability will set if not overridden by CopyAbilityData
        [Header("Default Stat Values (used if not modified by CopyAbilityData)")] [SerializeField]
        private float defaultCoyoteTime = 0.1f;

        [SerializeField] private float defaultJumpBufferTime = 0.15f;
        [SerializeField] private float defaultJumpReleaseGravityMultiplier = 2.5f;
        private bool _hasDoubleJumped;

        // Runtime state
        private bool _hasJumped;
        private bool _isJumpHeld;
        private float _jumpStartTime;
        private float _lastGroundedTime;
        private float _lastJumpPressedTime;

        public bool IsJumping { get; private set; }


        public void FinalizeStats(KirbyStats stats)
        {
            // This ability defines specific values for these jump-related stats.
            // It sets them directly. CopyAbilityData can also modify these if they are StatTypes.
            // If a StatModifier for these exists in CopyAbilityData, it would have already been applied.
            // This step can be seen as JumpAbility ensuring its core parameters are set,
            // potentially overriding what CopyAbilityData set if these weren't StatTypes,
            // or simply using the values from stats if they were already modified by CopyAbilityData.

            // For simplicity, let's assume these are base operational parameters for this jump ability.
            // If CopyAbilityData modified CoyoteTime, that value will be used.
            // If not, this ability doesn't force its defaultCoyoteTime onto the stats.
            // The stats object (which has been processed by CopyAbilityData) is what we use.
            // If we wanted this ability to *always* override, we would do:
            // stats.SetStat(StatType.CoyoteTime, defaultCoyoteTime); 
            // But for now, we assume CopyAbilityData has precedence for StatTypes.
            // The serialized fields defaultCoyoteTime etc. are more for reference or if this ability
            // was used without a CopyAbilityData that modifies these specific stats.
        }

        public Vector2 ProcessMovement(Vector2 currentVelocity, Vector2 targetVelocity, bool isGrounded)
        {
            // This method is now part of IMovementAbility.
            // It's responsible for modifying velocity during an ongoing jump (e.g., variable jump height).
            // The actual jump impulse is handled by PerformJump/PerformDoubleJump.

            if (isGrounded) // Handled by OnActivate or a ProcessAbility check for landing
            {
                // Reset some jump states on grounding, though much of this is in OnActivate or ProcessAbility
                _hasJumped = false;
                _hasDoubleJumped = false;
                // IsJumping is set to false in ProcessAbility when landing or y velocity < 0
            }

            // Update timing info for coyote time
            if (isGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            Vector2 modifiedVelocity = currentVelocity;

            if (!IsJumping) // Only apply these physics if a jump is in progress
                return modifiedVelocity;

            // If jump button released early while still ascending
            if (!_isJumpHeld && modifiedVelocity.y > 0)
            {
                // Apply increased gravity to cut the jump short
                // Ensure Controller and Stats are not null
                if (Controller && Controller.Stats != null)
                {
                    // Use the stat from KirbyStats, which might have been modified by CopyAbilityData
                    float releaseMultiplier = Controller.Stats.GetStat(StatType.JumpReleaseGravityMultiplier);
                    float extraGravity =
                        Physics2D.gravity.y * (releaseMultiplier - 1) *
                        Time.fixedDeltaTime; // Subtract 1 because base gravity is already applied

                    modifiedVelocity.y += extraGravity;
                }
            }

            // If exceeded max jump hold time, stop allowing upward influence by holding
            if (Time.time - _jumpStartTime > maxJumpHoldTime)
            {
                _isJumpHeld = false; // This primarily affects the variable jump height logic
            }

            return modifiedVelocity;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            _hasJumped = false;
            _hasDoubleJumped = false;
            IsJumping = false;
            _isJumpHeld = false;
            _lastGroundedTime = Controller.IsGrounded ? Time.time : -100f; // Initialize lastGroundedTime
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EndJump(); // Ensure jump state is fully reset
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (!Controller) return;

            // Check for buffered jumps
            if (ShouldPerformBufferedJump())
            {
                PerformJump();
            }

            // Update IsJumping state based on Y velocity and grounded status
            if (IsJumping)
            {

                // If we were jumping and now we are falling, or if we landed
                if (Controller.Rigidbody.linearVelocity.y < -0.01f ||
                    Controller.IsGrounded &&
                    Time.time - _jumpStartTime > 0.05f) // Small delay to prevent instant grounding
                {
                    if (Controller.IsGrounded)
                    {
                        _hasJumped = false; // Can jump again from ground
                        _hasDoubleJumped = false;
                        IsJumping = false; // No longer in the upward/controlled phase of a jump
                    }
                    else if (Controller.Rigidbody.linearVelocity.y < -0.01f) // If just falling, but not yet grounded
                    {
                        IsJumping = false; // No longer in the controlled ascent of a jump
                    }
                }
            }

            // Update last grounded time if we become grounded
            if (Controller.IsGrounded)
            {
                _lastGroundedTime = Time.time;
                // If we land, reset _hasJumped to allow for a new jump.
                // This is important if IsJumping was set to false due to falling, but before actually landing.
                if (!_hasJumped && !IsJumping)
                {
                    // Check !IsJumping to ensure we are not in the middle of a jump action
                    _hasDoubleJumped = false;
                }
            }
        }

        public void OnJumpPressed()
        {
            if (Controller == null || Controller.Stats == null) return;

            _lastJumpPressedTime = Time.time;
            _isJumpHeld = true;

            bool isWithinCoyoteTime = Time.time - _lastGroundedTime <= Controller.Stats.GetStat(StatType.CoyoteTime);

            if ((Controller.IsGrounded || isWithinCoyoteTime) && !_hasJumped)
            {
                PerformJump();
            }
            else if (allowDoubleJump && _hasJumped && !_hasDoubleJumped)
            {
                PerformDoubleJump();
            }
        }

        public void OnJumpReleased()
        {
            _isJumpHeld = false;
        }

        private bool ShouldPerformBufferedJump()
        {
            if (!Controller || Controller.Stats == null) return false;
            bool isWithinJumpBuffer =
                Time.time - _lastJumpPressedTime <= Controller.Stats.GetStat(StatType.JumpBufferTime);

            return isWithinJumpBuffer && Controller.IsGrounded && !_hasJumped && !IsJumping;
        }

        public void PerformJump()
        {
            if (!Controller || Controller.Stats == null) return;


            float jumpVelocity = Controller.Stats.GetStat(StatType.JumpVelocity);
            Controller.Rigidbody.linearVelocity = new Vector2(Controller.Rigidbody.linearVelocity.x, jumpVelocity);

            _hasJumped = true;
            IsJumping = true;
            _isJumpHeld = true; // Start holding jump
            _jumpStartTime = Time.time;
            _lastJumpPressedTime = 0f; // Consume buffer
        }

        private void PerformDoubleJump()
        {
            if (!Controller || Controller.Stats == null) return;


            Controller.Rigidbody.linearVelocity =
                new Vector2(Controller.Rigidbody.linearVelocity.x,
                    doubleJumpForce); // Uses JumpAbility's own doubleJumpForce

            _hasDoubleJumped = true;
            IsJumping = true; // Still considered jumping
            _isJumpHeld = true; // Can also hold double jump for variable height if desired, or set to false
            _jumpStartTime = Time.time; // Reset jump start time for variable height on double jump
        }

        public void EndJump()
        {
            IsJumping = false;
            _isJumpHeld = false;
        }
    }
}
