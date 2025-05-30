using UnityEngine;

/// <summary>
///     Ability for jumping
/// </summary>
public class JumpAbility : MovementAbilityBase
{
    private bool _isJumping;

    // Parameters
    private readonly float _jumpForce = 10f;

    /// <summary>
    ///     Priority of jumping ability
    /// </summary>
    public override int Priority => 10;

    /// <summary>
    ///     Handle input for jumping
    /// </summary>
    public override bool HandleInput(InputContext context)
    {
        if (context.EventType == InputEventType.Pressed && context.JumpPressed && _character.IsGrounded)
        {
            StartJump();
            return true;
        }

        if (context.EventType == InputEventType.Update && _isJumping)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Process movement for jumping
    /// </summary>
    public override bool ProcessMovement(MovementContext context)
    {
        if (_isJumping)
        {
            Vector2 velocity = context.Velocity;
            velocity.y = _jumpForce;
            context.DesiredVelocity = velocity;
            _isJumping = false; // Reset jumping state after applying force
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Start the jump
    /// </summary>
    private void StartJump()
    {
        _isJumping = true;
        NotifyStateChanged(MovementStateType.Jump);
    }
}
