using Movement;
using UnityEngine;

/// <summary>
///     Ability for walking and running
/// </summary>
public class WalkAbility : MovementAbilityBase
{
    private readonly float _runSpeed = 8f;
    private readonly float _runThreshold = 0.7f;

    private float _speedMultiplier = 1f;

    // Parameters
    private readonly float _walkSpeed = 5f;

    /// <summary>
    ///     Priority of walking ability (lowest priority)
    /// </summary>
    public override int Priority => 0;

    /// <summary>
    ///     Set the speed multiplier for this ability (useful for different forms)
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    /// <summary>
    ///     Handle input for this ability
    /// </summary>
    public override bool HandleInput(InputContext context) =>
        // Walking doesn't need to handle any specific inputs
        false;

    /// <summary>
    ///     Process movement for walking/running
    /// </summary>
    public override bool ProcessMovement(MovementContext context)
    {
        // Only process when grounded
        if (!context.IsGrounded)
            return false;

        // Get horizontal input
        float horizontal = context.Rigidbody.linearVelocity.x;
        float moveInput = context.Velocity.x;

        // Determine state
        MovementStateType state;
        float speed;

        if (Mathf.Abs(moveInput) > _runThreshold)
        {
            state = MovementStateType.Run;
            speed = _runSpeed * _speedMultiplier;
        }
        else if (Mathf.Abs(moveInput) > 0.1f)
        {
            state = MovementStateType.Walk;
            speed = _walkSpeed * _speedMultiplier;
        }
        else
        {
            state = MovementStateType.Idle;
            speed = 0f;
        }

        // Update state for animation
        NotifyStateChanged(state);

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
