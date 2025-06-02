namespace Kirby.Abilities.Animation
{
    /// <summary>
    ///     Flying animation state for Kirby
    /// </summary>
    public class FlyAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Fly";
            Priority = 15f; // Higher priority than jump/fall
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Check if Kirby is flying (not grounded, and jump is held after leaving ground)
            !Controller.IsGrounded &&
            input.JumpHeld &&
            Controller.Rigidbody.linearVelocity.y > 0.1f &&
            !Controller.IsGrounded && input.JumpPressed;
    }

    /// <summary>
    ///     Float animation state for Kirby (when descending slowly)
    /// </summary>
    public class FloatAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Float";
            Priority = 14f; // Lower than Fly but higher than normal fall
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when falling and in a "floating" state (slowed descent)
            !Controller.IsGrounded &&
            Controller.Rigidbody.linearVelocity.y < 0 &&
            !Controller.IsGrounded &&
            Controller.Rigidbody.linearVelocity.y < 0 &&
            input is { JumpHeld: false, JumpReleased: true };
    }
}
