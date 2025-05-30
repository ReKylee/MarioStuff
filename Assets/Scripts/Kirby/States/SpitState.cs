using UnityEngine;

namespace Kirby.States
{
    public class SpitState : KirbyStateBase
    {
        private readonly float spitAnimDuration = 0.3f; // Duration of the spit animation
        private float stateEnterTime;

        public SpitState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            PlayStateAnimation("Spit", false); // Kirby is no longer full after spitting
            kirbyController.Spit(); // KirbyController handles the logic for spitting
            stateEnterTime = Time.time;
        }

        public override void LogicUpdate()
        {
            // After animation, transition to Idle or Fall
            if (Time.time >= stateEnterTime + spitAnimDuration)
            {
                if (kirbyController.MovementController.IsGrounded)
                {
                    kirbyController.TransitionToState(new IdleState(kirbyController));
                }
                else
                {
                    kirbyController.TransitionToState(new FallState(kirbyController));
                }
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Allow minimal horizontal control or none during spit, depending on desired game feel
            // For now, allowing some control:
            float horizontalInput = kirbyController.InputHandler.HorizontalInput;
            kirbyController.MovementController.MoveHorizontal(horizontalInput * 0.5f, false); // Reduced control

            // Apply appropriate physics (e.g., if spitting in air, should still be affected by gravity)
            if (!kirbyController.MovementController.IsGrounded)
            {
                kirbyController.MovementController.ApplyFallingPhysics();
            }
        }
    }
}
