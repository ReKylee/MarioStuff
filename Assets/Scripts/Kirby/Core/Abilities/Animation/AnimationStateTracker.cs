namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Tracks and manages Kirby's animation state flags and timers
    /// </summary>
    public class AnimationStateTracker
    {

        // State flags
        private bool _isFull;

        // Animation state
        public AnimState CurrentState { get; private set; } = AnimState.Idle;
        public AnimState PreviousState { get; private set; } = AnimState.Idle;

        public bool IsFull
        {
            get => _isFull;
            set
            {
                if (value && !_isFull)
                {
                    // When becoming full, reset bounce-related flags
                    ShouldBounce = false;
                    PreloadingBounceAnimation = false;
                }

                _isFull = value;
            }
        }

        public bool IsFlying { get; set; }
        public bool IsFloating { get; set; }
        public bool IsInhaling { get; set; }
        public bool CanDoubleJump { get; set; }
        public bool ShouldBounce { get; set; }
        public bool HasBouncedThisLanding { get; set; }
        public bool WaitingForAnimComplete { get; set; }
        public bool IsJumpApex { get; set; }
        public bool HasStartedFallingThisJump { get; set; }
        public bool PreloadingBounceAnimation { get; set; }
        public bool HasBounceForceBeenApplied { get; set; }

        // Timers
        public float FallTimer { get; set; }
        public float LastGroundedY { get; set; }
        public float StateTimer { get; set; }
        public float InhaleTimer { get; set; }
        public float ApexTimer { get; set; }

        // Input tracking
        public float LastNonZeroHorizontalInput { get; set; }
        public bool WasAttackHeld { get; set; }

        // Animation tracking
        public string LastPlayedAnimation { get; set; }
        public bool HasPlayedJumpFullAnimation { get; set; }

        // Heights and distances
        public float JumpStartHeight { get; set; }
        public float MaxJumpHeight { get; set; }
        public float CurrentJumpHeight { get; set; }
        public bool WasGrounded { get; set; } = true;

        /// <summary>
        ///     Changes the current animation state and updates state tracking
        /// </summary>
        public void ChangeState(AnimState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            StateTimer = 0f;

            // Reset bounce force application when changing states
            if (newState == AnimState.BounceOffFloor)
            {
                HasBounceForceBeenApplied = false;
            }
        }

        /// <summary>
        ///     Updates all state timers based on delta time
        /// </summary>
        public void UpdateTimers(float deltaTime)
        {
            StateTimer += deltaTime;

            if (IsInhaling)
            {
                InhaleTimer += deltaTime;
            }

            if (IsJumpApex)
            {
                ApexTimer += deltaTime;
                if (ApexTimer >= 0.1f) // Short time window to detect apex
                {
                    IsJumpApex = false;
                }
            }
        }

        /// <summary>
        ///     Called when Kirby lands on the ground
        /// </summary>
        public void OnLanded()
        {
            FallTimer = 0f;
            IsFlying = false;
            IsFloating = false;

            CanDoubleJump = false;
            HasStartedFallingThisJump = false;
            PreloadingBounceAnimation = false;
            MaxJumpHeight = 0f;
            CurrentJumpHeight = 0f;

            if (!ShouldBounce)
            {
                HasBouncedThisLanding = false;
            }
        }

        /// <summary>
        ///     Called when Kirby leaves the ground
        /// </summary>
        public void OnLeftGround(float positionY)
        {
            LastGroundedY = positionY;
            JumpStartHeight = positionY;
        }

        /// <summary>
        ///     Updates jump height tracking based on current position
        /// </summary>
        public void UpdateJumpHeightTracking(float currentY)
        {
            CurrentJumpHeight = currentY - JumpStartHeight;

            // Update max height if we're still going up
            if (CurrentJumpHeight > MaxJumpHeight)
            {
                MaxJumpHeight = CurrentJumpHeight;
            }
        }
    }
}
