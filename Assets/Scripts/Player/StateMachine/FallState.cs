using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Fall state for when Kirby is falling without floating
    /// </summary>
    public class FallState : PlayerState
    {
        public FallState(IStateMachineContext context) : base(context) { }
        
        public override void Enter()
        {
            Context.SetAnimation("IsFalling", true);
        }
        
        public override void Exit()
        {
            Context.SetAnimation("IsFalling", false);
        }
        
        public override void HandleInput()
        {
            // Transition to idle or run if we hit the ground
            if (Context.IsGrounded)
            {
                if (Mathf.Abs(Context.MovementInput.x) > 0.1f)
                {
                    Context.ChangeState<RunState>();
                }
                else
                {
                    Context.ChangeState<IdleState>();
                }
                return;
            }
            
            // Transition to float state if jump is pressed while falling
            if (Context.JumpPressed || (Context.JumpHeld && Context.Velocity.y < -1f))
            {
                Context.ChangeState<FloatState>();
                return;
            }
            
            // Transition to float state if inhale is pressed
            if (Context.InhalePressed)
            {
                Context.ChangeState<FloatState>();
                return;
            }
        }
        
        public override void Update()
        {
            // Get current velocity
            Vector2 velocity = Context.Velocity;
            
            // Handle horizontal movement in air (similar to run but with air acceleration)
            float targetSpeed = Context.MovementInput.x * Context.MoveSpeed;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                targetSpeed,
                Context.AirAcceleration * Time.deltaTime
            );
            
            // Apply gravity - Kirby falls at a moderate rate in Amazing Mirror
            velocity.y -= Context.FallSpeed * Time.deltaTime;
            
            // Terminal velocity
            velocity.y = Mathf.Max(velocity.y, -15f);
            
            // Update velocity
            Context.Velocity = velocity;
        }
    }
}
