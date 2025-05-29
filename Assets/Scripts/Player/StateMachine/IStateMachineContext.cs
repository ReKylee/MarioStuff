using UnityEngine;

namespace Player.StateMachine
{
    /// <summary>
    /// Interface that defines what a state needs from its context
    /// </summary>
    public interface IStateMachineContext
    {
        // Access to movement parameters
        float MoveSpeed { get; }
        float Acceleration { get; }
        float Deceleration { get; }
        float AirAcceleration { get; }
        float AirDeceleration { get; }
        float JumpForce { get; }
        float FallSpeed { get; }
        float FloatSpeed { get; }
        float JumpCooldown { get; }
        
        // State properties
        Vector2 Velocity { get; set; }
        float JumpTimer { get; set; }
        
        // Ground state
        bool IsGrounded { get; }
        bool WasGrounded { get; }
        float CoyoteTimer { get; set; }
        
        // Input state
        Vector2 MovementInput { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool InhalePressed { get; }
        
        // Animation
        void SetAnimation(string name, bool value);
        void SetAnimationFloat(string name, float value);
        
        // State management
        void ChangeState<T>() where T : PlayerState;
    }
}
