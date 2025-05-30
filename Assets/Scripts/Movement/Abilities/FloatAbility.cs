using Movement;
using UnityEngine;

/// <summary>
///     Ability for floating (slow fall)
/// </summary>
public class FloatAbility : MovementAbilityBase
{
    private readonly float _airControl = 0.8f;

    private readonly float _floatDuration = 2f;

    // Parameters
    private readonly float _floatSlowFallSpeed = 3f;
    private float _floatTimer;

    // State
    private bool _isFloating;

    /// <summary>
    ///     Priority of floating ability
    /// </summary>
    public override int Priority => 15;

    /// <summary>
    ///     Availability check for floating
    /// </summary>
    public override bool IsAvailable => base.IsAvailable && _character.CurrentForm != CharacterForm.Full;

    /// <summary>
    ///     Handle input for floating
    /// </summary>
    public override bool HandleInput(InputContext context)
    {
        // Can't float in Full form
        if (_character.CurrentForm == CharacterForm.Full)
            return false;

        // Check for jump button being held during a fall
        if (context.EventType == InputEventType.Pressed && context.JumpHeld)
        {
            if (!_character.IsGrounded && !_isFloating && _character.Rigidbody.linearVelocity.y < 0)
            {
                StartFloating();
                return true;
            }
        }
        else if (context.EventType == InputEventType.Released && !context.JumpHeld)
        {
            if (_isFloating)
            {
                StopFloating();
                return true;
            }
        }
        else if (context.EventType == InputEventType.Update)
        {
            // Update float timer
            if (_isFloating)
            {
                UpdateFloatTimer(context.DeltaTime);
                return true;
            }
        }

        return _isFloating;
    }

    /// <summary>
    ///     Process movement for floating
    /// </summary>
    public override bool ProcessMovement(MovementContext context)
    {
        // Only process when floating
        if (!_isFloating || context.IsGrounded)
            return false;

        // Get horizontal input with air control
        float moveInput = context.Velocity.x;

        // Apply horizontal movement with air control
        Vector2 velocity = context.Velocity;
        velocity.x = moveInput * _airControl;

        // Apply slow fall
        velocity.y = -_floatSlowFallSpeed;

        context.DesiredVelocity = velocity;
        return true;
    }

    /// <summary>
    ///     Start floating
    /// </summary>
    public void StartFloating()
    {
        if (!_isFloating && !_character.IsGrounded && _character.CurrentForm != CharacterForm.Full)
        {
            _isFloating = true;
            _floatTimer = _floatDuration;

            // Update state for animation
            NotifyStateChanged(MovementStateType.Float);
        }
    }

    /// <summary>
    ///     Stop floating
    /// </summary>
    public void StopFloating()
    {
        if (_isFloating)
        {
            _isFloating = false;

            // Update state for animation if not grounded
            if (!_character.IsGrounded)
            {
                NotifyStateChanged(MovementStateType.Fall);
            }
        }
    }

    /// <summary>
    ///     Update float timer
    /// </summary>
    private void UpdateFloatTimer(float deltaTime)
    {
        if (_isFloating && _floatTimer > 0)
        {
            _floatTimer -= deltaTime;

            // Auto-stop floating when timer runs out
            if (_floatTimer <= 0)
            {
                StopFloating();
            }
        }
    }

    /// <summary>
    ///     Handle initialization
    /// </summary>
    public override void Initialize(CharacterController2D character)
    {
        base.Initialize(character);

        // Subscribe to form changes
        if (character is KirbyController kirbyController)
        {
            kirbyController.OnFormChanged += OnFormChanged;
        }
    }

    /// <summary>
    ///     Handle form changes
    /// </summary>
    private void OnFormChanged(CharacterForm form)
    {
        // If changing to Full form while floating, stop floating
        if (form == CharacterForm.Full && _isFloating)
        {
            StopFloating();
        }
    }
}
