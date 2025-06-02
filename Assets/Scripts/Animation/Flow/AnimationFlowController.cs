using System.Collections.Generic;
using Animation.Flow.Adapters;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    ///     Main controller for the animation flow system
    /// </summary>
    public class AnimationFlowController : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator _spriteAnimator;
        [SerializeField] private string _initialStateId;
        private readonly Dictionary<string, IAnimationState> _states = new();

        private IAnimator _animator;
        private AnimationContext _context;
        private IAnimationState _currentState;

        private void Awake()
        {
            // Create adapter for the animator
            _animator = new SpriteAnimatorAdapter(_spriteAnimator);

            // Create animation context
            _context = new AnimationContext(_animator, gameObject);

            // Initialize states - this would be set up by a configuration system
            InitializeStates();
        }

        private void Start()
        {
            // Enter initial state if defined
            if (!string.IsNullOrEmpty(_initialStateId) &&
                _states.TryGetValue(_initialStateId, out IAnimationState initialState))
            {
                TransitionToState(initialState);
            }
        }

        private void Update()
        {
            if (_currentState == null)
                return;

            // Update current state
            _currentState.OnUpdate(_context, Time.deltaTime);

            // Check for transitions
            string nextStateId = _currentState.CheckTransitions(_context);
            if (!string.IsNullOrEmpty(nextStateId) && _states.TryGetValue(nextStateId, out IAnimationState nextState))
            {
                TransitionToState(nextState);
            }
        }

        /// <summary>
        ///     Transition to a new animation state
        /// </summary>
        private void TransitionToState(IAnimationState newState)
        {
            // Exit current state
            _currentState?.OnExit(_context);

            // Enter new state
            _currentState = newState;
            _currentState.OnEnter(_context);

            Debug.Log($"Transitioned to animation state: {newState.Id}");
        }

        /// <summary>
        ///     Add a state to this controller
        /// </summary>
        public void AddState(IAnimationState state)
        {
            _states[state.Id] = state;
        }

        /// <summary>
        ///     Force a transition to a specific state
        /// </summary>
        public bool ForceTransition(string stateId)
        {
            if (_states.TryGetValue(stateId, out IAnimationState state))
            {
                TransitionToState(state);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Set a parameter in the animation context
        /// </summary>
        public void SetParameter<T>(string name, T value)
        {
            _context.SetParameter(name, value);
        }

        /// <summary>
        ///     Get a parameter from the animation context
        /// </summary>
        public T GetParameter<T>(string name) => _context.GetParameter<T>(name);

        /// <summary>
        ///     Initialize animation states - this would be replaced by a configuration system
        /// </summary>
        protected virtual void InitializeStates()
        {
            // Override in derived classes or replace with configuration loading
        }
    }


}
