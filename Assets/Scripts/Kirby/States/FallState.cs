namespace Kirby.States
{
    public class FallState : KirbyStateBase
    {
        public FallState(KirbyController controller) : base(controller)
        {
        }

        public override void EnterState()
        {
            PlayStateAnimation("Fall", kirbyController.IsFull);
            kirbyController.MovementController.ApplyFallingPhysics();
        }

        public override void LogicUpdate()
        {
            // Handle spit input when full
            if (kirbyController.InputHandler.ExhalePressed && kirbyController.IsFull)
            {
                kirbyController.TransitionToState(new SpitState(kirbyController));
                return;
            }

            // Handle double jump (transition directly to fly if allowed for this form)
            // This allows Kirby to enter fly state even when falling
            if (kirbyController.InputHandler.JumpPressed && kirbyController.CurrentTransformation.CanFly)
            {
                PlayStateAnimation("JumpToFly", kirbyController.IsFull);
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

            // Apply falling physics (gravity and terminal velocity)
            kirbyController.MovementController.ApplyFallingPhysics();

            // Check if we've landed
            if (kirbyController.MovementController.IsGrounded)
            {
                kirbyController.TransitionToState(new LandingState(kirbyController));
            }
        }
    }
}
