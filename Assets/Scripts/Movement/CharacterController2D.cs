using System;
using System.Collections.Generic;
using System.Linq;
using Movement;
using UnityEngine;
using UnityEngine.InputSystem;

//TODO: Make this use a CharacterController instead of Rigidbody2D
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Ground Detection")] [SerializeField]
    private LayerMask groundLayer;

    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float slopeThreshold = 30f;
    [SerializeField] private Vector2 groundCheckSize = new(0.9f, 0.1f);

    [Header("Movement Settings")] [SerializeField]
    private float groundedGravity = -1f;

    [SerializeField] private float maxFallSpeed = 15f;

    // Movement abilities
    protected List<IMovementAbility> _abilities = new();
    protected bool _abilityPressed;
    protected Animator _animator;
    protected Collider2D _collider;
    protected IMovementAbility _currentAbility;
    protected CharacterForm _currentForm = CharacterForm.Normal;
    protected float _fallDistance;
    protected float _groundAngle;
    protected Vector2 _groundNormal = Vector2.up;
    protected InputSystem_Actions _inputActions;
    protected InputContext _inputContext = new();
    protected bool _isFacingRight = true;

    // State tracking
    protected bool _isGrounded;
    protected bool _jumpHeld;
    protected bool _jumpPressed;

    // Input tracking
    protected Vector2 _moveInput;

    // Contexts
    protected MovementContext _movementContext = new();

    // References
    protected Rigidbody2D _rigidbody;
    protected SpriteRenderer _spriteRenderer;

    // Events
    public event Action<CharacterForm> OnFormChanged;
    public event Action<SlopeType> OnSlopeChanged;
    public event Action<bool> OnDirectionChanged;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        // Get component references
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // Initialize input system
        _inputActions = new InputSystem_Actions();

        // Initialize abilities
        InitializeAbilities();
    }

    protected virtual void OnEnable()
    {
        // Enable input system
        _inputActions.Enable();

        // Set up input callbacks
        _inputActions.Player.Move.performed += OnMoveInput;
        _inputActions.Player.Move.canceled += OnMoveInput;

        _inputActions.Player.Jump.performed += OnJumpInput;
        _inputActions.Player.Jump.canceled += OnJumpInputReleased;

        _inputActions.Player.Attack.performed += OnAbilityInput;

        // Enable all abilities
        foreach (IMovementAbility ability in _abilities)
        {
            ability.Enable();
        }
    }

    protected virtual void OnDisable()
    {
        // Remove input callbacks
        _inputActions.Player.Move.performed -= OnMoveInput;
        _inputActions.Player.Move.canceled -= OnMoveInput;

        _inputActions.Player.Jump.performed -= OnJumpInput;
        _inputActions.Player.Jump.canceled -= OnJumpInputReleased;

        _inputActions.Player.Attack.performed -= OnAbilityInput;

        // Disable input system
        _inputActions.Disable();

        // Disable all abilities
        foreach (IMovementAbility ability in _abilities)
        {
            ability.Disable();
        }
    }

    protected virtual void Update()
    {
        // Update ground state
        CheckGrounded();

        // Update movement context
        UpdateMovementContext();

        // Update input context for regular update
        UpdateInputContext(InputEventType.Update);

        // Process input in abilities
        ProcessAbilityInput();

        // Calculate fall distance when falling
        if (!_isGrounded && _rigidbody.linearVelocity.y < 0)
        {
            _fallDistance += -_rigidbody.linearVelocity.y * Time.deltaTime;
        }
        else if (_isGrounded)
        {
            _fallDistance = 0;
        }
    }

    protected virtual void FixedUpdate()
    {
        // Process movement in abilities
        ProcessAbilityMovement();

        // Apply movement
        ApplyMovement();
    }

    #endregion

    #region Initialization

    /// <summary>
    ///     Initialize movement abilities
    /// </summary>
    protected virtual void InitializeAbilities()
    {
        // Override in derived classes to add specific abilities
    }

    /// <summary>
    ///     Add a movement ability to the controller
    /// </summary>
    public void AddAbility(IMovementAbility ability)
    {
        if (!_abilities.Contains(ability))
        {
            ability.Initialize(this);
            _abilities.Add(ability);

            // Sort abilities by priority (higher priority first)
            _abilities = _abilities.OrderByDescending(a => a.Priority).ToList();

            if (isActiveAndEnabled)
            {
                ability.Enable();
            }
        }
    }

    /// <summary>
    ///     Remove a movement ability from the controller
    /// </summary>
    public void RemoveAbility(IMovementAbility ability)
    {
        if (_abilities.Contains(ability))
        {
            ability.Disable();
            _abilities.Remove(ability);
        }
    }

    #endregion

    #region Input Handling

    protected virtual void OnMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();

        // Update input context
        UpdateInputContext(InputEventType.ValueChanged, context);

        // Process input in abilities
        ProcessAbilityInput();

        // Handle sprite flipping
        if (_moveInput.x > 0 && !_isFacingRight)
        {
            FlipSprite();
        }
        else if (_moveInput.x < 0 && _isFacingRight)
        {
            FlipSprite();
        }
    }

    protected virtual void OnJumpInput(InputAction.CallbackContext context)
    {
        _jumpPressed = true;
        _jumpHeld = true;

        // Update input context
        UpdateInputContext(InputEventType.Pressed, context);

        // Process input in abilities
        ProcessAbilityInput();
    }

    protected virtual void OnJumpInputReleased(InputAction.CallbackContext context)
    {
        _jumpPressed = false;
        _jumpHeld = false;

        // Update input context
        UpdateInputContext(InputEventType.Released, context);

        // Process input in abilities
        ProcessAbilityInput();
    }

    protected virtual void OnAbilityInput(InputAction.CallbackContext context)
    {
        _abilityPressed = true;

        // Update input context
        UpdateInputContext(InputEventType.Pressed, context);

        // Process input in abilities
        ProcessAbilityInput();

        // Reset after processing
        _abilityPressed = false;
    }

    protected virtual void UpdateInputContext(InputEventType eventType, InputAction.CallbackContext? context = null)
    {
        _inputContext.MoveInput = _moveInput;
        _inputContext.JumpPressed = _jumpPressed;
        _inputContext.JumpHeld = _jumpHeld;
        _inputContext.AbilityPressed = _abilityPressed;
        _inputContext.DeltaTime = Time.deltaTime;
        _inputContext.EventType = eventType;
        _inputContext.OriginalContext = context;
    }

    /// <summary>
    ///     Process input in movement abilities
    /// </summary>
    protected virtual void ProcessAbilityInput()
    {
        // Process input in abilities by priority until one handles it
        foreach (IMovementAbility ability in _abilities.Where(a => a.IsAvailable))
        {
            if (ability.HandleInput(_inputContext))
            {
                _currentAbility = ability;
                break;
            }
        }
    }

    #endregion

    #region Movement Processing

    protected virtual void CheckGrounded()
    {
        // Create a box for ground detection
        RaycastHit2D hit = Physics2D.BoxCast(
            _collider.bounds.center,
            groundCheckSize,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        bool wasGrounded = _isGrounded;
        _isGrounded = hit.collider != null;

        if (_isGrounded)
        {
            // Store ground normal for slope calculations
            _groundNormal = hit.normal;
            _groundAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Determine slope type
            UpdateSlopeType();
        }
        else
        {
            _groundNormal = Vector2.up;
            _groundAngle = 0;
        }
    }

    protected virtual void UpdateSlopeType()
    {
        SlopeType newSlopeType;

        if (_groundAngle < 1f)
        {
            newSlopeType = SlopeType.Flat;
        }
        else if (_groundAngle < slopeThreshold)
        {
            newSlopeType = SlopeType.Gentle;
        }
        else
        {
            newSlopeType = SlopeType.Steep;
        }

        // Notify of slope change
        if (_movementContext.SlopeType != newSlopeType)
        {
            _movementContext.SlopeType = newSlopeType;
            OnSlopeChanged?.Invoke(newSlopeType);
        }
    }

    protected virtual void UpdateMovementContext()
    {
        _movementContext.Velocity = _rigidbody.linearVelocity;
        _movementContext.IsGrounded = _isGrounded;
        _movementContext.GroundNormal = _groundNormal;
        _movementContext.GroundAngle = _groundAngle;
        _movementContext.IsFacingRight = _isFacingRight;
        _movementContext.CurrentForm = _currentForm;
        _movementContext.DeltaTime = Time.fixedDeltaTime;
        _movementContext.FallDistance = _fallDistance;
        _movementContext.Rigidbody = _rigidbody;
        _movementContext.Collider = _collider;
        _movementContext.IsUphill = CalculateIsUphill();
    }

    /// <summary>
    ///     Process movement in abilities
    /// </summary>
    protected virtual void ProcessAbilityMovement()
    {
        // Initialize desired velocity based on current velocity
        _movementContext.DesiredVelocity = _rigidbody.linearVelocity;

        // Process movement in abilities by priority until one handles it
        bool movementHandled = false;
        foreach (IMovementAbility ability in _abilities.Where(a => a.IsAvailable))
        {
            if (ability.ProcessMovement(_movementContext))
            {
                movementHandled = true;
                _currentAbility = ability;
                break;
            }
        }

        // If no ability handled movement, apply default movement
        if (!movementHandled)
        {
            ApplyDefaultMovement();
        }
    }

    /// <summary>
    ///     Apply default movement when no ability handles it
    /// </summary>
    protected virtual void ApplyDefaultMovement()
    {
        // Default movement logic
        Vector2 velocity = _rigidbody.linearVelocity;

        if (_isGrounded)
        {
            // Apply ground movement
            velocity.x = 0;

            // Apply gravity when grounded (helps stick to slopes)
            velocity.y = groundedGravity;
        }
        else
        {
            // Cap fall speed
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        }

        _movementContext.DesiredVelocity = velocity;
    }

    /// <summary>
    ///     Apply movement to the character
    /// </summary>
    protected virtual void ApplyMovement()
    {
        _rigidbody.linearVelocity = _movementContext.DesiredVelocity;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    ///     Flip the character's sprite
    /// </summary>
    protected virtual void FlipSprite()
    {
        _isFacingRight = !_isFacingRight;
        _spriteRenderer.flipX = !_isFacingRight;

        // Notify direction change
        OnDirectionChanged?.Invoke(_isFacingRight);
    }

    /// <summary>
    ///     Calculate if the character is facing uphill
    /// </summary>
    protected virtual bool CalculateIsUphill()
    {
        if (_groundAngle < 1f)
        {
            return false;
        }

        // Calculate if the character is facing uphill based on ground normal and facing direction
        float facingDirection = _isFacingRight ? 1f : -1f;
        Vector2 movementDirection = new(facingDirection, 0f);

        // Dot product will be positive if moving uphill, negative if downhill
        float dot = Vector2.Dot(movementDirection, _groundNormal);

        return dot < 0; // The ground normal points away from the slope, so negative dot means uphill
    }

    /// <summary>
    ///     Change the character's form
    /// </summary>
    public virtual void ChangeForm(CharacterForm newForm)
    {
        if (_currentForm != newForm)
        {
            _currentForm = newForm;
            _movementContext.CurrentForm = newForm;
            OnFormChanged?.Invoke(newForm);
        }
    }

    #endregion

    #region Accessors

    public bool IsGrounded => _isGrounded;
    public bool IsFacingRight => _isFacingRight;
    public CharacterForm CurrentForm => _currentForm;
    public SlopeType CurrentSlopeType => _movementContext.SlopeType;
    public Rigidbody2D Rigidbody => _rigidbody;
    public Collider2D Collider => _collider;

    #endregion

}
