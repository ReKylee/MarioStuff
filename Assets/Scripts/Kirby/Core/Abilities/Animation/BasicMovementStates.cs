using UnityEngine;

namespace Kirby.Abilities.Animation
{
    /// <summary>
    ///     Idle animation state for Kirby
    /// </summary>
    public class IdleAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Idle";
            Priority = 1f;
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when grounded and not moving horizontally
            Controller.IsGrounded &&
            Mathf.Abs(Controller.Rigidbody.linearVelocity.x) < 0.1f;
    }

    /// <summary>
    ///     Walking animation state for Kirby
    /// </summary>
    public class WalkAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Walk";
            Priority = 2f;
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when grounded, moving horizontally, and not running
            Controller.IsGrounded &&
            Mathf.Abs(Controller.Rigidbody.linearVelocity.x) >= 0.1f &&
            Mathf.Abs(Controller.Rigidbody.linearVelocity.x) < Controller.Stats.runSpeed * 0.8f;
    }

    /// <summary>
    ///     Running animation state for Kirby
    /// </summary>
    public class RunAnimationState : KirbyAnimationState
    {
        protected override void OnInitialize()
        {
            AnimationName = "Run";
            Priority = 3f;
            ShouldLoop = true;
        }

        public override bool ShouldBeActive(InputContext input) =>
            // Active when grounded and moving fast horizontally
            Controller.IsGrounded &&
            Mathf.Abs(Controller.Rigidbody.linearVelocity.x) >= Controller.Stats.runSpeed * 0.8f;
    }
}
