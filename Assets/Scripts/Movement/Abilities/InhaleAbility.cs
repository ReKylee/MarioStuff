using Movement;

/// <summary>
///     Ability for inhaling objects or enemies
/// </summary>
public class InhaleAbility : MovementAbilityBase
{
    // Parameters
    private readonly float _inhaleDuration = 2f;
    private float _inhaleTimer;
    private bool _isInhaling;

    /// <summary>
    ///     Priority of inhaling ability
    /// </summary>
    public override int Priority => 30;

    /// <summary>
    ///     Handle input for inhaling
    /// </summary>
    public override bool HandleInput(InputContext context)
    {
        if (context.EventType == InputEventType.Pressed && context.AbilityPressed)
        {
            StartInhale();
            return true;
        }

        if (context.EventType == InputEventType.Update && _isInhaling)
        {
            UpdateInhaleTimer(context.DeltaTime);
            return true;
        }

        if (context.EventType == InputEventType.Released && _isInhaling)
        {
            StopInhale();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Process movement for inhaling
    /// </summary>
    public override bool ProcessMovement(MovementContext context) =>
        // Inhaling does not affect movement directly
        _isInhaling;

    /// <summary>
    ///     Start inhaling
    /// </summary>
    private void StartInhale()
    {
        _isInhaling = true;
        _inhaleTimer = _inhaleDuration;
        NotifyStateChanged(MovementStateType.Inhale);
    }

    /// <summary>
    ///     Stop inhaling
    /// </summary>
    private void StopInhale()
    {
        _isInhaling = false;
        NotifyStateChanged(MovementStateType.Idle);
    }

    /// <summary>
    ///     Update inhale timer
    /// </summary>
    private void UpdateInhaleTimer(float deltaTime)
    {
        if (_inhaleTimer > 0)
        {
            _inhaleTimer -= deltaTime;

            if (_inhaleTimer <= 0)
            {
                StopInhale();
            }
        }
    }
}
