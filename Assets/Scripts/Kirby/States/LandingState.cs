using UnityEngine;

namespace Kirby.States
{
    public class LandingState : KirbyStateBase
    {
        private readonly float landingTime = 0.2f; // Duration for the landing animation/state
        private float stateEnterTime;

        public LandingState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // Play landing animation (could be end part of Fall.anim or a specific Landing.anim)
            // For now, using "Fall" as a placeholder, assuming animator handles the bounce/end part.
            PlayStateAnimation("Fall", kirbyController.IsFull);
            stateEnterTime = Time.time;

            // Ensure Kirby is not moving when landing
            kirbyController.MovementController.ResetVelocity();
        }

        public override void LogicUpdate()
        {
            // After a short duration, transition to Idle or Walk based on input
            if (Time.time >= stateEnterTime + landingTime)
            {
                if (Mathf.Abs(kirbyController.InputHandler.HorizontalInput) > 0.01f)
                {
                    kirbyController.TransitionToState(new WalkState(kirbyController));
                }
                else
                {
                    kirbyController.TransitionToState(new IdleState(kirbyController));
                }
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection (should still be grounded)
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Ensure Kirby stays put during landing animation
            kirbyController.MovementController.MoveHorizontal(0, false);
        }
    }
}
