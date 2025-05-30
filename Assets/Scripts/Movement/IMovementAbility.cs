using Movement;

/// <summary>
///     Interface for movement abilities
/// </summary>
public interface IMovementAbility
{
    /// <summary>
    ///     Whether this movement ability is currently available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    ///     Priority of this movement ability (higher priority abilities take precedence)
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Process movement for this ability
    /// </summary>
    /// <param name="movementContext">Current movement context</param>
    /// <returns>True if this ability handled movement, false otherwise</returns>
    bool ProcessMovement(MovementContext movementContext);

    /// <summary>
    ///     Handle input for this ability
    /// </summary>
    /// <param name="inputContext">Current input context</param>
    /// <returns>True if this ability handled input, false otherwise</returns>
    bool HandleInput(InputContext inputContext);

    /// <summary>
    ///     Initialize the ability
    /// </summary>
    /// <param name="character">Character controller reference</param>
    void Initialize(CharacterController2D character);

    /// <summary>
    ///     Enable the ability
    /// </summary>
    void Enable();

    /// <summary>
    ///     Disable the ability
    /// </summary>
    void Disable();
}
