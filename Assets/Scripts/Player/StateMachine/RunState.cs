using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Run state for when Kirby is moving horizontally
    /// </summary>
    public class RunState : PlayerState
    {
        public RunState(IStateMachineContext context) : base(context) { }
        
        public override void Enter()
        {
            Context.SetAnimation("IsRunning", true);
        }
        
        public override void Exit()
        {
            Context.SetAnimation("IsRunning", false);
        }
        
        public override void HandleInput()
        {
            // Transition to idle if not pressing movement keys
            if (Mathf.Abs(Context.MovementInput.x) < 0.1f)
            {
                Context.ChangeState<IdleState>();
                return;
            }
            
            // Transition to jump state if jump is pressed
            if (Context.JumpPressed && Context.IsGrounded && Context.JumpTimer <= 0)
            {
                Context.ChangeState<JumpState>();
                return;
            }
            
            // Transition to fall state if not grounded
            if (!Context.IsGrounded && Context.Velocity.y < 0)
            {
                Context.ChangeState<FallState>();
                return;
            }
        }
        
        public override void Update()
        {
            // Calculate target speed (full speed in the input direction)
            float targetSpeed = Context.MovementInput.x * Context.MoveSpeed;
            
            // Apply acceleration/deceleration to reach the target speed
            // Kirby accelerates quickly to feel responsive
            Vector2 velocity = Context.Velocity;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                targetSpeed,
                Context.Acceleration * Time.deltaTime
            );
            Context.Velocity = velocity;
        }
    }
}
