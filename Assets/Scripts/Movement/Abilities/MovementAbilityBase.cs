using System;
using Movement;

/// <summary>
///     Base class for all movement abilities
/// </summary>
public abstract class MovementAbilityBase : IMovementAbility
{
    // Character controller reference
    protected CharacterController2D _character;

    // State
    protected bool _isEnabled = true;

    /// <summary>
    ///     Whether this ability is currently available
    /// </summary>
    public virtual bool IsAvailable => _isEnabled;

    /// <summary>
    ///     Priority of this ability (higher priority abilities take precedence)
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    ///     Initialize the ability
    /// </summary>
    public virtual void Initialize(CharacterController2D character)
    {
        _character = character;
    }

    /// <summary>
    ///     Enable the ability
    /// </summary>
    public virtual void Enable()
    {
        _isEnabled = true;
    }

    /// <summary>
    ///     Disable the ability
    /// </summary>
    public virtual void Disable()
    {
        _isEnabled = false;
    }

    /// <summary>
    ///     Process movement for this ability
    /// </summary>
    public abstract bool ProcessMovement(MovementContext context);

    /// <summary>
    ///     Handle input for this ability
    /// </summary>
    public abstract bool HandleInput(InputContext context);

    // Events
    public event Action<MovementStateType> OnStateChanged;

    /// <summary>
    ///     Notify state changed
    /// </summary>
    protected void NotifyStateChanged(MovementStateType state)
    {
        OnStateChanged?.Invoke(state);
    }
}
