using Kirby.Interfaces;
using UnityEngine;

namespace Kirby.States
{
    /// <summary>
    ///     Kirby's jumping state
    /// </summary>
    public class JumpState : KirbyStateBase
    {
        private readonly bool canDoubleJump = true;
        private float jumpStartTime;

        public JumpState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // Play jump animation
            PlayStateAnimation("Jump", kirbyController.IsFull);

            // Apply initial jump force
            kirbyController.MovementController.Jump(true);

            // Record jump start time
            jumpStartTime = Time.time;
        }

        public override void LogicUpdate()
        {
            // Handle spit input when full
            if (kirbyController.InputHandler.ExhalePressed && kirbyController.IsFull)
            {
                kirbyController.TransitionToState(new SpitState(kirbyController));
                return;
            }

            // Handle swallow input when full
            if (kirbyController.InputHandler.AttackPressed && kirbyController.IsFull)
            {
                kirbyController.TransitionToState(new SwallowState(kirbyController));
                return;
            }

            // Handle double jump (transition directly to fly if allowed for this form)
            if (kirbyController.InputHandler.JumpPressed && canDoubleJump &&
                kirbyController.CurrentTransformation.CanFly)
            {
                // Play JumpToFly animation which will transition to Fly in the animator
                PlayStateAnimation("JumpToFly", kirbyController.IsFull);

                // Transition directly to FlyState
                kirbyController.TransitionToState(new FlyState(kirbyController));
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Handle horizontal movement in air
            float horizontalInput = kirbyController.InputHandler.HorizontalInput;
            kirbyController.MovementController.MoveHorizontal(horizontalInput, false);

            // Apply variable jump height based on button hold
            kirbyController.MovementController.Jump(kirbyController.InputHandler.JumpHeld);

            // Check if we're falling
            if (kirbyController.MovementController.CurrentVelocity.y <= 0)
            {
                kirbyController.TransitionToState(new FallState(kirbyController));
                return;
            }

            // Check if we've landed
            if (kirbyController.MovementController.IsGrounded)
            {
                kirbyController.TransitionToState(new LandingState(kirbyController));
            }
        }

        public override IKirbyState CheckTransitions() =>
            // All transitions are handled in LogicUpdate and PhysicsUpdate
            null;
    }
}
