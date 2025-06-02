namespace Kirby.Abilities.Animation
{
    /// <summary>
    ///     Jump animation state for Kirby - holds first frame while button is held
    /// </summary>
    public class JumpStartAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "JumpStart";
            Priority = 10f;
            ShouldLoop = false; // This state will just show the first frame
            CanBeInterrupted = false; // Only the JumpReleaseState can interrupt this
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when: in the air, moving upward, and jump button is held
            !Controller.IsGrounded &&
            Controller.Rigidbody.linearVelocity.y > 0.1f &&
            input.JumpHeld;
    }

    /// <summary>
    ///     Jump release animation state - plays full jump animation when button is released
    /// </summary>
    public class JumpReleaseAnimationState : KirbyAnimationState
    {
        private bool _hasCompletedAnimation;

        protected override void OnInitialize()
        {
            AnimationName = "Jump";
            Priority = 9f;
            ShouldLoop = false;
        }

        public override void Enter()
        {
            base.Enter();
            _hasCompletedAnimation = false;
        }

        public override void Update(InputContext input)
        {
            // Track when animation completes
            if (HasAnimationFinished())
            {
                _hasCompletedAnimation = true;
            }
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when: in the air, moving upward, jump button released, and animation not completed
            !Controller.IsGrounded &&
            Controller.Rigidbody.linearVelocity.y > 0.1f &&
            !input.JumpHeld &&
            !_hasCompletedAnimation;
    }

    /// <summary>
    ///     Fall animation state for Kirby
    /// </summary>
    public class FallAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Fall";
            Priority = 8f;
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when in the air and moving downward
            !Controller.IsGrounded && Controller.Rigidbody.linearVelocity.y < 0;
    }
}
