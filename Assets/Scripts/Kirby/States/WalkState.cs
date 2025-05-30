using UnityEngine;

namespace Kirby.States
{
    public class WalkState : KirbyStateBase
    {
        public WalkState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            PlayStateAnimation("Walk", kirbyController.IsFull);
        }

        public override void LogicUpdate()
        {
            // Transition to Idle
            if (Mathf.Abs(kirbyController.InputHandler.HorizontalInput) < 0.01f)
            {
                kirbyController.TransitionToState(new IdleState(kirbyController));
                return;
            }

            // Transition to Jump
            if (kirbyController.InputHandler.JumpPressed)
            {
                kirbyController.TransitionToState(new JumpState(kirbyController));
                return;
            }

            // Transition to Crouch
            if (kirbyController.InputHandler.CrouchHeld && kirbyController.CurrentTransformation.CanCrouch)
            {
                kirbyController.TransitionToState(new CrouchState(kirbyController));
                return;
            }

            // Transition to Inhale or Spit (based on full status)
            if (kirbyController.InputHandler.AttackPressed)
            {
                if (kirbyController.IsFull)
                {
                    kirbyController.TransitionToState(new SpitState(kirbyController));
                }
                else if (kirbyController.CurrentTransformation.CanInhale)
                {
                    kirbyController.TransitionToState(new InhaleState(kirbyController));
                }
            }

            // TODO: Add transition to RunState if a separate run input/logic is implemented
            // For now, WalkState covers both walking and running based on analog input magnitude handled by MovementController
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // If somehow not grounded, transition to Fall
            if (!kirbyController.MovementController.IsGrounded)
            {
                kirbyController.TransitionToState(new FallState(kirbyController));
                return;
            }

            // Handle horizontal movement
            float horizontalInput = kirbyController.InputHandler.HorizontalInput;
            // Assuming MovementController handles walk/run speed based on input magnitude or a run flag
            bool isRunning = Mathf.Abs(horizontalInput) > 0.75f; // Example: consider running if input is strong
            kirbyController.MovementController.MoveHorizontal(horizontalInput, isRunning);

            // Update animation based on whether walking or running
            if (isRunning)
            {
                PlayStateAnimation("Run", kirbyController.IsFull);
            }
            else
            {
                PlayStateAnimation("Walk", kirbyController.IsFull);
            }
        }
    }
}
