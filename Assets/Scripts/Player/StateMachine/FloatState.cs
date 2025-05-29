using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Float state for when Kirby is puffed up and floating
    /// </summary>
    public class FloatState : PlayerState
    {
        private float _floatTime;
        private const float MAX_FLOAT_TIME = 3.0f; // Kirby can float for about 3 seconds in Amazing Mirror
        private const float FLOAT_ASCEND_SPEED = 5.0f; // Speed when floating upward
        private const float FLOAT_PULSE_FREQUENCY = 0.5f; // Kirby pulses while floating about twice per second
        
        public FloatState(IStateMachineContext context) : base(context) { }
        
        public override void Enter()
        {
            Context.SetAnimation("IsFloating", true);
            
            // Initial upward boost when starting to float
            if (Context.Velocity.y < 0)
            {
                Vector2 velocity = Context.Velocity;
                velocity.y = 0;
                Context.Velocity = velocity;
            }
            
            _floatTime = 0;
            
            // Play puff sound effect
            // SoundManager.Instance.PlaySound("Puff");
        }
        
        public override void Exit()
        {
            Context.SetAnimation("IsFloating", false);
        }
        
        public override void HandleInput()
        {
            // Transition to fall state if float time is exceeded
            if (_floatTime >= MAX_FLOAT_TIME)
            {
                Context.ChangeState<FallState>();
                return;
            }
            
            // Transition to fall state if jump is released
            if (!Context.JumpHeld)
            {
                Context.ChangeState<FallState>();
                return;
            }
            
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
        }
        
        public override void Update()
        {
            _floatTime += Time.deltaTime;
            
            // Get current velocity
            Vector2 velocity = Context.Velocity;
            
            // Handle horizontal movement while floating (slightly slower than normal)
            float targetSpeed = Context.MovementInput.x * Context.MoveSpeed * 0.8f;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                targetSpeed,
                Context.AirAcceleration * Time.deltaTime
            );
            
            // Handle vertical movement - pressing up while floating makes Kirby rise
            if (Context.MovementInput.y > 0.1f)
            {
                // Ascend when pressing up
                velocity.y = Mathf.MoveTowards(
                    velocity.y,
                    FLOAT_ASCEND_SPEED,
                    Context.AirAcceleration * Time.deltaTime
                );
            }
            else
            {
                // Gentle floating descent
                velocity.y = Mathf.MoveTowards(
                    velocity.y,
                    -Context.FloatSpeed * 0.5f,
                    Context.FloatSpeed * Time.deltaTime
                );
            }
            
            // Create pulsing effect - Kirby slightly bobs up and down while floating
            float pulseAmount = 0.2f * Mathf.Sin(Time.time * FLOAT_PULSE_FREQUENCY * Mathf.PI * 2);
            velocity.y += pulseAmount;
            
            // Update velocity
            Context.Velocity = velocity;
            
            // Update animator with float time percentage for visual feedback
            Context.SetAnimationFloat("FloatTimePercent", _floatTime / MAX_FLOAT_TIME);
        }
    }
}
