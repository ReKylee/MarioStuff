using Kirby.Interfaces;
using UnityEngine;

namespace Kirby.States
{
    /// <summary>
    ///     Transition state from flying to falling
    /// </summary>
    public class FlyEndState : KirbyStateBase
    {
        private float stateStartTime;
        private readonly float transitionDuration = 0.3f; // Duration of the transition animation

        public FlyEndState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // Play the fly-end animation
            PlayStateAnimation("FlyEnd", kirbyController.IsFull);

            // Start applying falling physics
            kirbyController.MovementController.ApplyFallingPhysics();

            // Record the start time for animation timing
            stateStartTime = Time.time;
        }

        public override void LogicUpdate()
        {
            // Check if transition animation has completed
            if (Time.time - stateStartTime >= transitionDuration)
            {
                kirbyController.TransitionToState(new FallState(kirbyController));
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Handle horizontal movement in air
            float horizontalInput = kirbyController.InputHandler.HorizontalInput;
            kirbyController.MovementController.MoveHorizontal(horizontalInput, false);

            // Apply falling physics
            kirbyController.MovementController.ApplyFallingPhysics();

            // If we hit the ground during this transition, go to landing state
            if (kirbyController.MovementController.IsGrounded)
            {
                kirbyController.TransitionToState(new LandingState(kirbyController));
            }
        }

        public override IKirbyState CheckTransitions() =>
            // All transitions are handled in LogicUpdate and PhysicsUpdate
            null;

        public override void ExitState()
        {
            // Ensure falling physics are applied as we exit
            kirbyController.MovementController.ApplyFallingPhysics();
        }
    }
}
