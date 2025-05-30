using UnityEngine;

/// <summary>
/// Ability for crouching
/// </summary>
public class CrouchAbility : MovementAbilityBase
{
    // Parameters
    private float _crouchSpeed = 3f;
    private float _crouchThreshold = -0.5f;
    
    // State
    private bool _isCrouching = false;
    
    /// <summary>
    /// Priority of crouching ability
    /// </summary>
    public override int Priority => 5;
    
    /// <summary>
    /// Handle input for crouching
    /// </summary>
    public override bool HandleInput(InputContext context)
    {
        // Check for crouch input
        if (context.EventType == InputEventType.Update || context.EventType == InputEventType.ValueChanged)
        {
            bool shouldCrouch = _character.IsGrounded && context.MoveInput.y < _crouchThreshold;
            
            if (shouldCrouch != _isCrouching)
            {
                _isCrouching = shouldCrouch;
                
                // Update state for animation
                NotifyStateChanged(_isCrouching ? MovementStateType.Crouch : MovementStateType.Idle);
                
                return _isCrouching;
            }
            
            return _isCrouching;
        }
        
        return false;
    }
    
    /// <summary>
    /// Process movement for crouching
    /// </summary>
    public override bool ProcessMovement(MovementContext context)
    {
        // Only process when crouching
        if (!_isCrouching || !context.IsGrounded)
            return false;
            
        // Get horizontal input with reduced speed
        float moveInput = context.Velocity.x;
        float speed = _crouchSpeed;
        
        // Apply form specific modifiers if needed
        if (context.CurrentForm == CharacterForm.Full)
        {
            speed *= 0.8f;
        }
        
        // Apply horizontal movement
        Vector2 velocity = context.Velocity;
        velocity.x = moveInput * speed;
        
        // Apply movement on slope if needed
        if (context.SlopeType != SlopeType.Flat && context.GroundAngle > 0)
        {
            // Calculate movement along the slope
            velocity.x = moveInput * speed * Mathf.Sign(Vector2.Dot(context.GroundNormal, Vector2.right));
            velocity.y = moveInput * speed * Mathf.Sign(Vector2.Dot(context.GroundNormal, Vector2.up));
        }
        
        context.DesiredVelocity = velocity;
        return true;
    }
}
