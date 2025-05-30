using System;
using UnityEngine;

/// <summary>
///     Kirby-specific implementation of the CharacterController2D
/// </summary>
public class KirbyController : CharacterController2D
{
    [Header("Kirby Settings")] [SerializeField]
    private float fullFormSpeedMultiplier = 0.8f;

    private CrouchAbility _crouchAbility;

    // Current movement state for animation
    private FloatAbility _floatAbility;
    private FlyAbility _flyAbility;
    private InhaleAbility _inhaleAbility;
    private JumpAbility _jumpAbility;

    // References to specific ability instances
    private WalkAbility _walkAbility;

    /// <summary>
    ///     Access to the current movement state for animation
    /// </summary>
    public MovementStateType CurrentMovementState { get; private set; } = MovementStateType.Idle;

    protected override void OnDisable()
    {
        base.OnDisable();

        // Unsubscribe from movement state changes
        if (_walkAbility != null) _walkAbility.OnStateChanged -= HandleMovementStateChanged;
        if (_jumpAbility != null) _jumpAbility.OnStateChanged -= HandleMovementStateChanged;
        if (_crouchAbility != null) _crouchAbility.OnStateChanged -= HandleMovementStateChanged;
        if (_flyAbility != null) _flyAbility.OnStateChanged -= HandleMovementStateChanged;
        if (_floatAbility != null) _floatAbility.OnStateChanged -= HandleMovementStateChanged;
        if (_inhaleAbility != null) _inhaleAbility.OnStateChanged -= HandleMovementStateChanged;
    }

    // Events for animation
    public event Action<MovementStateType> OnMovementStateChanged;

    /// <summary>
    ///     Initialize Kirby's movement abilities
    /// </summary>
    protected override void InitializeAbilities()
    {
        base.InitializeAbilities();

        // Create ability instances
        _walkAbility = new WalkAbility();
        _jumpAbility = new JumpAbility();
        _crouchAbility = new CrouchAbility();
        _flyAbility = new FlyAbility();
        _floatAbility = new FloatAbility();
        _inhaleAbility = new InhaleAbility();

        // Add abilities to the controller
        AddAbility(_inhaleAbility); // Highest priority
        AddAbility(_flyAbility);
        AddAbility(_floatAbility);
        AddAbility(_jumpAbility);
        AddAbility(_crouchAbility);
        AddAbility(_walkAbility); // Lowest priority

        // Subscribe to movement state changes
        _walkAbility.OnStateChanged += HandleMovementStateChanged;
        _jumpAbility.OnStateChanged += HandleMovementStateChanged;
        _crouchAbility.OnStateChanged += HandleMovementStateChanged;
        _flyAbility.OnStateChanged += HandleMovementStateChanged;
        _floatAbility.OnStateChanged += HandleMovementStateChanged;
        _inhaleAbility.OnStateChanged += HandleMovementStateChanged;
    }

    /// <summary>
    ///     Handle movement state changes from abilities
    /// </summary>
    private void HandleMovementStateChanged(MovementStateType newState)
    {
        if (CurrentMovementState != newState)
        {
            CurrentMovementState = newState;
            OnMovementStateChanged?.Invoke(newState);
        }
    }

    /// <summary>
    ///     Change Kirby's form
    /// </summary>
    public override void ChangeForm(CharacterForm newForm)
    {
        base.ChangeForm(newForm);

        // Configure abilities based on form
        ConfigureAbilitiesForForm(newForm);
    }

    /// <summary>
    ///     Configure abilities based on character form
    /// </summary>
    private void ConfigureAbilitiesForForm(CharacterForm form)
    {
        switch (form)
        {
            case CharacterForm.Normal:
                // Normal form has all abilities
                EnableAllAbilities();
                break;

            case CharacterForm.Full:
                // Full form cannot fly or float
                EnableAllAbilities();
                _flyAbility.Disable();
                _floatAbility.Disable();
                break;

            case CharacterForm.Rider:
                // Rider can only run fast in two directions
                DisableAllAbilities();
                _walkAbility.Enable();
                _walkAbility.SetSpeedMultiplier(1.5f); // Faster running
                break;

            case CharacterForm.Fire:
                // Fire form has all abilities but may have different parameters
                EnableAllAbilities();
                // Configure specific parameters for Fire form
                break;

            case CharacterForm.Ice:
                // Ice form has all abilities but may have different parameters
                EnableAllAbilities();
                // Configure specific parameters for Ice form
                break;
        }
    }

    /// <summary>
    ///     Enable all movement abilities
    /// </summary>
    private void EnableAllAbilities()
    {
        foreach (IMovementAbility ability in _abilities)
        {
            ability.Enable();
        }
    }

    /// <summary>
    ///     Disable all movement abilities
    /// </summary>
    private void DisableAllAbilities()
    {
        foreach (IMovementAbility ability in _abilities)
        {
            ability.Disable();
        }
    }
}
