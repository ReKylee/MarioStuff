using UnityEngine;

namespace Kirby.Abilities.Modules
{
    /// <summary>
    ///     Jump ability for Kirby - handles jumping mechanics including variable height jumps
    /// </summary>
    public class JumpAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        [Header("Jump Ability Settings")] [SerializeField]
        private float maxJumpHoldTime = 0.3f;

        // Runtime state
        private bool _hasJumped; // True if Kirby has performed a jump since last being grounded
        private float _jumpStartTime; // Time when the current jump started
        private float _lastGroundedTime; // Last time Kirby was grounded, for coyote time
        private float _lastJumpPressedTime; // Last time jump input was pressed, for buffering

        public bool
            IsJumping { get; private set; } // True if Kirby is currently in the upward/controlled phase of a jump

        public Vector2 ProcessMovement(Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            Vector2 modifiedVelocity = currentVelocity;

            // Only apply jump-specific physics if a jump is in progress and controller/stats are valid
            if (!IsJumping || !Controller || Controller.Stats == null)
            {
                return modifiedVelocity;
            }

            // Variable jump height: If jump button released OR max jump hold time exceeded, while still ascending
            if ((!inputContext.JumpHeld || Time.time - _jumpStartTime > maxJumpHoldTime) && modifiedVelocity.y > 0)
            {
                // Apply increased gravity to cut the jump short.
                // Physics2D.gravity.y is typically negative.
                // (releaseMultiplier - 1) should be > 0 for a multiplier > 1.
                // Adding a negative value reduces upward velocity or increases downward velocity.
                float releaseMultiplier = Controller.Stats.jumpReleaseGravityMultiplier;
                modifiedVelocity.y += Physics2D.gravity.y * (releaseMultiplier - 1) * Time.fixedDeltaTime;
            }

            return modifiedVelocity;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            _hasJumped = false;
            IsJumping = false;
            _lastGroundedTime =
                Controller != null && Controller.IsGrounded ? Time.time : -100f; // Initialize based on current state

            _lastJumpPressedTime = 0f; // Reset jump pressed time
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EndJump(); // Ensure jump state is fully reset
        }

        public override void ProcessAbility(InputContext inputContext)
        {
            base.ProcessAbility(inputContext);
            if (!Controller || Controller.Stats == null) return;

            // Handle jump pressed logic from inputContext
            if (inputContext.JumpPressed)
            {
                _lastJumpPressedTime = Time.time; // Record time for buffering

                bool isWithinCoyoteTime =
                    Time.time - _lastGroundedTime <= Controller.Stats.coyoteTime;

                // Perform jump if:
                // 1. Grounded OR within coyote time window
                // 2. Hasn't already performed a jump in the current airtime (_hasJumped is false)
                if ((Controller.IsGrounded || isWithinCoyoteTime) && !_hasJumped)
                {
                    PerformJump();
                }
            }

            // Check for buffered jumps (if a jump wasn't initiated by the above block)
            if (!_hasJumped && ShouldPerformBufferedJump(inputContext))
            {
                PerformJump();
            }

            // Update IsJumping state (controls variable jump height logic in ProcessMovement)
            if (IsJumping)
            {
                // Conditions to end the "IsJumping" state (controlled ascent phase):
                // 1. Kirby starts falling (velocity.y < ~0).
                // 2. Kirby lands (isGrounded is true, with a small delay to prevent instant state flip on jump start).
                if (Controller.Rigidbody.linearVelocity.y < -0.01f ||
                    Controller.IsGrounded && Time.time - _jumpStartTime > 0.05f)
                {
                    IsJumping = false;
                }
            }

            // Update grounded state and reset _hasJumped if landed
            if (Controller.IsGrounded)
            {
                _lastGroundedTime = Time.time;
                if (!IsJumping) // If not in the controlled ascent phase (e.g. landed or just walking)
                {
                    _hasJumped = false; // Reset to allow a new jump
                }
            }
        }

        private bool ShouldPerformBufferedJump(InputContext inputContext)
        {
            if (!Controller || Controller.Stats == null) return false;

            // Check if jump was pressed recently
            bool isWithinJumpBuffer =
                Time.time - _lastJumpPressedTime <= Controller.Stats.jumpBufferTime;

            // Conditions for a buffered jump:
            // 1. Jump to input was registered within the buffer time window (_lastJumpPressedTime > 0 ensures it was pressed).
            // 2. Kirby is now grounded.
            // 3. Kirby hasn't already jumped in this air/ground cycle (_hasJumped is false).
            // 4. Kirby is not currently in the controlled ascent phase of a jump (IsJumping is false).
            return _lastJumpPressedTime > 0f && isWithinJumpBuffer && Controller.IsGrounded && !_hasJumped &&
                   !IsJumping;
        }

        public void PerformJump()
        {
            if (!Controller || Controller.Stats == null) return;

            Controller.Rigidbody.linearVelocity =
                new Vector2(Controller.Rigidbody.linearVelocity.x, Controller.Stats.jumpVelocity);

            _hasJumped = true; // Mark that a jump has been performed in this airtime
            IsJumping = true; // Enter the controlled ascent phase of the jump
            _jumpStartTime = Time.time; // Record start time for variable jump height
            _lastJumpPressedTime = 0f; // Consume the jump press / buffer
        }

        public void EndJump() // Called on deactivate or when jump naturally ends
        {
            IsJumping = false;
            // _hasJumped remains true until Kirby is grounded again.
        }
    }
}
