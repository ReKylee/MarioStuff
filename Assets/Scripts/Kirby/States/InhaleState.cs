using Kirby.Interfaces;
using UnityEngine;

namespace Kirby.States
{
    /// <summary>
    ///     State for Kirby inhaling enemies/objects
    /// </summary>
    public class InhaleState : KirbyStateBase
    {
        private readonly float maxInhaleTime = 2.0f; // Maximum time Kirby can inhale
        private bool hasInhaledObject;
        private float inhaleTimer;

        public InhaleState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // Play inhale animation
            PlayStateAnimation("Inhale", false);

            // Start inhale physics/logic
            kirbyController.Inhale();

            // Reset timer and tracking
            inhaleTimer = 0f;
            hasInhaledObject = false;
        }

        public override void LogicUpdate()
        {
            // Increment timer
            inhaleTimer += Time.deltaTime;

            // Check if we've successfully inhaled something
            if (hasInhaledObject)
            {
                // If we've inhaled something, transition to full state
                kirbyController.SetFullState(true);
                kirbyController.TransitionToState(new IdleState(kirbyController));
                return;
            }

            // Check if the player releases the inhale button or max time is reached
            if (!kirbyController.InputHandler.AttackHeld || inhaleTimer >= maxInhaleTime)
            {
                // If we didn't inhale anything, go back to idle
                kirbyController.TransitionToState(new IdleState(kirbyController));
                return;
            }

            // Check for swallow input (to immediately swallow what's in mouth)
            if (kirbyController.InputHandler.CrouchHeld && hasInhaledObject)
            {
                kirbyController.TransitionToState(new SwallowState(kirbyController));
            }
        }

        public override void PhysicsUpdate()
        {
            // Update ground detection
            (kirbyController.MovementController as KirbyMovementController)?.UpdateGroundDetection();

            // Limited horizontal movement during inhale
            float horizontalInput = kirbyController.InputHandler.HorizontalInput * 0.5f; // Reduced movement
            kirbyController.MovementController.MoveHorizontal(horizontalInput, false);

            // Check if we've fallen off a platform
            if (!kirbyController.MovementController.IsGrounded &&
                kirbyController.MovementController.CurrentVelocity.y < 0)
            {
                kirbyController.TransitionToState(new FallState(kirbyController));
            }
        }

        public override IKirbyState CheckTransitions() =>
            // All transitions are handled in LogicUpdate and PhysicsUpdate
            null;

        public override void ExitState()
        {
            // Stop inhale effect
            kirbyController.StopInhale();
        }

        /// <summary>
        ///     Called when an object is successfully inhaled
        /// </summary>
        /// <param name="inhaledObject">The GameObject that was inhaled</param>
        public void OnObjectInhaled(GameObject inhaledObject)
        {
            hasInhaledObject = true;
            kirbyController.SetSwallowedObject(inhaledObject);
        }
    }
}
