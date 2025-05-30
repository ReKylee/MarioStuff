namespace Kirby.States
{
    public class CrouchState : KirbyStateBase
    {
        public CrouchState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            PlayStateAnimation("Crouch", kirbyController.IsFull);
            // Optional: Apply any physics changes for crouching, e.g., smaller collider
            kirbyController.AnimationHandler.SetCrouchStatus(true);
        }

        public override void LogicUpdate()
        {
            // Transition to Idle if crouch is released
            if (!kirbyController.InputHandler.CrouchHeld)
            {
                kirbyController.TransitionToState(new IdleState(kirbyController));
                return;
            }

            // Transition to Swallow if attack/inhale pressed while full (as per requirements doc: "Swallowing: ... Done by crouching while full")
            // Note: The requirements also say "Swallowing: ... or object." - this implies an action while already full and crouching.
            // Assuming 'AttackPressed' is the action to initiate swallow from crouch+full state.
            if (kirbyController.IsFull && kirbyController.InputHandler.AttackPressed)
            {
                kirbyController.TransitionToState(new SwallowState(kirbyController));
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
                return;
            }

            // Crouching should prevent movement
            kirbyController.MovementController.MoveHorizontal(0, false);
        }

        public override void ExitState()
        {
            kirbyController.AnimationHandler.SetCrouchStatus(false);
            // Optional: Revert any physics changes made for crouching
        }
    }
}
