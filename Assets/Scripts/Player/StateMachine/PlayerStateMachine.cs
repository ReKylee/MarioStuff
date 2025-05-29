using UnityEngine;
using System;
using System.Collections.Generic;
using Player.Input;
using Player.Physics;

namespace Player.StateMachine
{
    /// <summary>
    /// Main state machine controller that manages player movement states
    /// </summary>
    public class PlayerStateMachine : MonoBehaviour, IStateMachineContext
    {
        // Dependencies
        private PlayerInputHandler _inputHandler;
        private GroundDetector _groundDetector;
        private Animator _animator;
        
        // Movement parameters
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 60f;
        [SerializeField] private float airAcceleration = 30f;
        [SerializeField] private float airDeceleration = 20f;
        
        // Jump settings
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float fallSpeed = 25f;
        [SerializeField] private float floatSpeed = 8f;
        [SerializeField] private float jumpCooldown = 0.1f;
        
        // IStateMachineContext property implementations
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float AirDeceleration => airDeceleration;
        public float JumpForce => jumpForce;
        public float FallSpeed => fallSpeed;
        public float FloatSpeed => floatSpeed;
        public float JumpCooldown => jumpCooldown;
        
        // Movement state
        public Vector2 Velocity { get; set; }
        public float JumpTimer { get; set; }
        
        // Input state (forwarded from input handler)
        public Vector2 MovementInput => _inputHandler ? _inputHandler.MovementInput : Vector2.zero;
        public bool JumpPressed => _inputHandler ? _inputHandler.JumpPressed : false;
        public bool JumpHeld => _inputHandler ? _inputHandler.JumpHeld : false;
        public bool InhalePressed => _inputHandler ? _inputHandler.InhalePressed : false;
        
        // Ground state (forwarded from ground detector)
        public bool IsGrounded => _groundDetector ? _groundDetector.IsGrounded : false;
        public bool WasGrounded => _groundDetector ? _groundDetector.WasGrounded : false;
        public float CoyoteTimer { 
            get => _groundDetector ? _groundDetector.CoyoteTimer : 0f;
            set { /* CoyoteTimer is managed by GroundDetector */ }
        }
        
        // Current state
        private PlayerState _currentState;
        
        // Dictionary of available states
        private Dictionary<Type, PlayerState> _availableStates;
        
        private void Awake()
        {
            // Get required components
            _inputHandler = GetComponent<PlayerInputHandler>() ?? gameObject.AddComponent<PlayerInputHandler>();
            _groundDetector = GetComponent<GroundDetector>() ?? gameObject.AddComponent<GroundDetector>();
            _animator = GetComponent<Animator>();
            
            // Initialize states
            _availableStates = new Dictionary<Type, PlayerState>
            {
                { typeof(IdleState), new IdleState(this) },
                { typeof(RunState), new RunState(this) },
                { typeof(JumpState), new JumpState(this) },
                { typeof(FallState), new FallState(this) },
                { typeof(FloatState), new FloatState(this) }
            };
            
            // Start in idle state
            ChangeState<IdleState>();
        }
        
        private void Update()
        {
            if (_currentState != null)
            {
                _currentState.HandleInput();
                _currentState.Update();
            }
            
            // Flip character based on direction
            if (MovementInput.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(MovementInput.x);
                transform.localScale = scale;
            }
        }
        
        private void FixedUpdate()
        {
            _currentState?.FixedUpdate();
            
            // Apply movement
            transform.Translate(Velocity * Time.deltaTime);
        }
        
        // Animation helper methods
        public void SetAnimation(string name, bool value)
        {
            _animator?.SetBool(name, value);
        }
        
        public void SetAnimationFloat(string name, float value)
        {
            _animator?.SetFloat(name, value);
        }
        
        // State change method
        public void ChangeState<T>() where T : PlayerState
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }
            
            var type = typeof(T);
            if (_availableStates.TryGetValue(type, out PlayerState newState))
            {
                _currentState = newState;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"State {type.Name} not found in available states!");
            }
        }
    }
}
