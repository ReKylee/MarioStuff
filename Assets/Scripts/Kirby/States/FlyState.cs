using Kirby.Interfaces;
using UnityEngine;

namespace Kirby.States
{
    /// <summary>
    ///     Kirby's flying state
    /// </summary>
    public class FlyState : KirbyStateBase
    {
        private float lastFlapTime;

        public FlyState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // The animation should already be playing JumpToFly, which will transition to Fly in the animator
            // Only play Fly animation directly if we're not coming from JumpToFly
            if (!kirbyController.AnimationHandler.IsPlayingAnimation("JumpToFly"))
            {
                PlayStateAnimation("Fly", kirbyController.IsFull);
            }

            // Initialize the flying state in the movement controller
            (kirbyController.MovementController as KirbyMovementController)?.InitializeFlying();

            // Initial flap to gain height
            kirbyController.MovementController.FlyFlap();
            lastFlapTime = Time.time;

            // Zero out vertical velocity to prevent momentum carrying over
            Vector2 currentVelocity = kirbyController.MovementController.CurrentVelocity;
            kirbyController.MovementController.ResetVelocity();

            // But preserve horizontal velocity
            kirbyController.MovementController.MoveHorizontal(
                Mathf.Sign(currentVelocity.x) * Mathf.Min(Mathf.Abs(currentVelocity.x), 5f),
                false
            );
        }

        public override void LogicUpdate()
        {
            // Handle spit/exhale input to end flying
            if (kirbyController.InputHandler.ExhalePressed)
            {
                // Play the fly-end animation which will transition to Fall in the animator
                PlayStateAnimation("FlyEnd", kirbyController.IsFull);

                // Transition to FlyEndState which will handle the transition animation
                kirbyController.TransitionToState(new FlyEndState(kirbyController));
                return;
            }

            // Handle flap input
            if (kirbyController.InputHandler.JumpPressed)
            {
                kirbyController.MovementController.FlyFlap();

                // Replay the fly animation for visual feedback
                PlayStateAnimation("Fly", kirbyController.IsFull);
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Handle horizontal movement in air
            float horizontalInput = kirbyController.InputHandler.HorizontalInput;
            kirbyController.MovementController.MoveHorizontal(horizontalInput, false);

            // Apply gentle descent physics when not flapping
            kirbyController.MovementController.FlyGentleDescent();

            // Check if we've landed
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
            // No specific cleanup needed, transitions will handle proper physics/animation
        }
    }
}
