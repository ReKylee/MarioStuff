using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Jump state for when Kirby is moving upward after a jump
    /// </summary>
    public class JumpState : PlayerState
    {
        private float _jumpStartTime;
        private const float MAX_JUMP_HOLD_TIME = 0.4f; // Maximum time jump can be extended by holding
        
        public JumpState(IStateMachineContext context) : base(context) { }
        
        public override void Enter()
        {
            Context.SetAnimation("IsJumping", true);
            
            // Initialize jump
            Vector2 velocity = Context.Velocity;
            velocity.y = Context.JumpForce;
            Context.Velocity = velocity;
            
            Context.JumpTimer = Context.JumpCooldown;
            _jumpStartTime = Time.time;
            
            // Play jump sound or effect here
            // SoundManager.Instance.PlaySound("Jump");
        }
        
        public override void Exit()
        {
            Context.SetAnimation("IsJumping", false);
        }
        
        public override void HandleInput()
        {
            // Transition to float state if jump is released during ascent
            if (!Context.JumpHeld && Context.Velocity.y > 0)
            {
                Vector2 velocity = Context.Velocity;
                velocity.y *= 0.5f; // Cut jump short if button released early
                Context.Velocity = velocity;
                
                Context.ChangeState<FallState>();
                return;
            }
            
            // Transition to float state if player presses inhale while in air
            if (Context.InhalePressed)
            {
                Context.ChangeState<FloatState>();
                return;
            }
            
            // Transition to fall state if velocity becomes negative
            if (Context.Velocity.y <= 0)
            {
                Context.ChangeState<FallState>();
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
            
            // Apply variable jump height if still in initial jump period
            float jumpHoldTime = Time.time - _jumpStartTime;
            if (Context.JumpHeld && jumpHoldTime < MAX_JUMP_HOLD_TIME)
            {
                // Reduced gravity while holding jump during initial ascent
                // This creates Kirby's characteristic variable-height jumps
                velocity.y -= (Context.FallSpeed * 0.5f) * Time.deltaTime;
            }
            else
            {
                // Normal gravity
                velocity.y -= Context.FallSpeed * Time.deltaTime;
            }
            
            // Update velocity
            Context.Velocity = velocity;
        }
    }
}
