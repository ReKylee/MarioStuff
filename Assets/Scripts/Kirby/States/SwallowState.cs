using Kirby.Interfaces;
using UnityEngine;

namespace Kirby.States
{
    /// <summary>
    ///     State for Kirby swallowing an inhaled enemy
    /// </summary>
    public class SwallowState : KirbyStateBase
    {
        private readonly float animationDuration = 0.5f; // Duration of swallow animation
        private float stateStartTime;

        public SwallowState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            // Play swallow animation
            PlayStateAnimation("Swallow", false);

            // Record start time for animation timing
            stateStartTime = Time.time;

            // Make Kirby swallow the held object
            kirbyController.Swallow();
        }

        public override void LogicUpdate()
        {
            // Check if swallow animation has completed
            if (Time.time - stateStartTime >= animationDuration)
            {
                // After swallowing, return to idle or fall state based on grounding
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

            // No horizontal movement during swallow animation
            kirbyController.MovementController.MoveHorizontal(0, false);
        }

        public override IKirbyState CheckTransitions() =>
            // All transitions are handled in LogicUpdate
            null;

        public override void ExitState()
        {
            // No specific cleanup needed
        }
    }
}
