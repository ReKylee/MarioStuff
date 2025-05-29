using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Idle state for when Kirby is standing still
    /// </summary>
    public class IdleState : PlayerState
    {
        public IdleState(IStateMachineContext context) : base(context) { }
        
        public override void Enter()
        {
            Context.SetAnimation("IsIdle", true);
        }
        
        public override void Exit()
        {
            Context.SetAnimation("IsIdle", false);
        }
        
        public override void HandleInput()
        {
            // Transition to run state if moving horizontally
            if (Mathf.Abs(Context.MovementInput.x) > 0.1f)
            {
                Context.ChangeState<RunState>();
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
            // Apply deceleration when idle
            Vector2 velocity = Context.Velocity;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, 
                Context.Deceleration * Time.deltaTime);
            Context.Velocity = velocity;
        }
    }
}
