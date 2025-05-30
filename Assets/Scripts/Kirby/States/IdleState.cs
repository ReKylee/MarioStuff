using UnityEngine;

namespace Kirby.States
{
    public class IdleState : KirbyStateBase
    {
        public IdleState(KirbyController controller) : base(controller) // Changed to public
        {
        }

        public override void EnterState()
        {
            PlayStateAnimation("Idle", kirbyController.IsFull);
            // Ensure Kirby is not moving when entering idle
            kirbyController.MovementController.ResetVelocity();
        }

        public override void LogicUpdate()
        {
            // Transition to Walk/Run
            if (Mathf.Abs(kirbyController.InputHandler.HorizontalInput) > 0.01f)
            {
                // Determine if running or walking based on input magnitude or a dedicated run button if you add one
                // For now, let's assume any horizontal input leads to WalkState
                kirbyController.TransitionToState(new WalkState(kirbyController));
                return;
            }

            // Transition to Jump
            if (kirbyController.InputHandler.JumpPressed)
            {
                kirbyController.TransitionToState(new JumpState(kirbyController));
                return;
            }

            // Transition to Crouch or Swallow
            if (kirbyController.InputHandler.CrouchHeld && kirbyController.CurrentTransformation.CanCrouch)
            {
                if (kirbyController.IsFull)
                {
                    kirbyController.TransitionToState(new SwallowState(kirbyController));
                }
                else
                {
                    kirbyController.TransitionToState(new CrouchState(kirbyController));
                }

                return;
            }

            // Transition to Inhale or Spit
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
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // If somehow not grounded, transition to Fall
            if (!kirbyController.MovementController.IsGrounded)
            {
                kirbyController.TransitionToState(new FallState(kirbyController));
            }

            // Ensure Kirby stays put on slopes if idle (minimal slide or stick to ground)
            // This might involve applying a small counter-force or ensuring Rigidbody settings prevent sliding
            // For now, ResetVelocity in EnterState helps, but more robust slope handling might be needed here or in MovementController
        }
    }
}
