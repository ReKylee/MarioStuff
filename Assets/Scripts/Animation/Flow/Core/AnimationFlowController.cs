using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Base controller for the animation flow system.
    ///     Handles state transitions, parameter management, and animation flow execution.
    /// </summary>
    public abstract class AnimationFlowController : MonoBehaviour
    {
        [Tooltip("The animation flow asset that defines the state machine")] [SerializeField]
        private AnimationFlowAsset flowAsset;

        [Tooltip("The initial state ID to enter when the controller starts")] [SerializeField]
        private string initialStateId;

        [Tooltip("Enable debug visualization in the game view")] [SerializeField]
        private bool debugVisualization;

        // Runtime state machine data
        private readonly Dictionary<string, IAnimationState> _states = new();

        // Animation context that provides parameters and animator access
        private AnimationContext _animationContext;

        // Cached animator adapter instance
        private IAnimator _animator;
        private IAnimationState _currentState;

        // Asset tracking
        private AnimationFlowAsset _previousFlowAsset;
        private float _timeInCurrentState;

        // Public access to the flow asset
        public AnimationFlowAsset FlowAsset
        {
            get => flowAsset;
            set
            {
                if (flowAsset == value) return;

#if UNITY_EDITOR
                // Unregister from previous asset
                flowAsset?.UnregisterController(this);

                // Assign new asset
                flowAsset = value;

                // Register with new asset
                flowAsset?.RegisterController(this);

                // Rebuild even in editor mode
                if (!Application.isPlaying)
                {
                    if (flowAsset is not null)
                    {
                        flowAsset.BuildFlowController(this);
                    }
                    else
                    {
                        ClearStates();
                    }
                }
#else
                _flowAsset = value;
#endif

                // Rebuild if in play mode
                if (Application.isPlaying)
                {
                    InitializeStateMachine();
                }
            }
        }

        /// <summary>
        ///     Reset static data between domain reloads
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            // This is a good place to reset any static data
        }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeAnimator();
            InitializeStateMachine();
        }

        protected virtual void OnEnable()
        {
            // Ensure we're initialized when enabled
            if (_animator is null)
            {
                InitializeAnimator();
            }

            if (_states.Count == 0 && flowAsset is not null)
            {
                InitializeStateMachine();
            }
        }

        protected virtual void Start()
        {
            // Enter initial state if defined
            if (!string.IsNullOrEmpty(initialStateId) &&
                _states.TryGetValue(initialStateId, out IAnimationState initialState))
            {
                TransitionToState(initialState);
            }
            else if (_states.Count > 0)
            {
                // If no initial state is specified but we have states,
                // just enter the first one as a fallback
                foreach (IAnimationState state in _states.Values)
                {
                    TransitionToState(state);
                    break;
                }
            }
        }

        protected virtual void Update()
        {
            if (_currentState is null)
                return;

            // Update time in current state
            _timeInCurrentState += Time.deltaTime;
            _animationContext.SetParameter("StateTime", _timeInCurrentState);

            // Update current state
            _currentState.OnUpdate(_animationContext, Time.deltaTime);

            // Check for transitions
            string nextStateId = _currentState.CheckTransitions(_animationContext);
            if (!string.IsNullOrEmpty(nextStateId) &&
                _states.TryGetValue(nextStateId, out IAnimationState nextState))
            {
                TransitionToState(nextState);
            }
        }

        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            // Unregister from asset when destroyed
            if (flowAsset is not null)
            {
                flowAsset.UnregisterController(this);
            }
#endif
        }

        protected virtual void OnGUI()
        {
            if (!debugVisualization || _currentState is null)
                return;

            // Draw debug information in the game view
            GUILayout.BeginArea(new Rect(10, 10, 300, 120));
            GUI.color = Color.black;
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.color = Color.white;

            GUILayout.Label($"Current State: {_currentState.Id} ({_currentState.AnimationName})");
            GUILayout.Label($"Time in State: {_timeInCurrentState:F2}s");

            // Show available transitions
            string nextStateId = _currentState.CheckTransitions(_animationContext);
            if (!string.IsNullOrEmpty(nextStateId) &&
                _states.TryGetValue(nextStateId, out IAnimationState nextState))
            {
                GUILayout.Label($"Next Transition: → {nextState.Id}");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Called when inspector values change
        /// </summary>
        protected virtual void OnValidate()
        {
            // Handle flow asset changes in the inspector
            if (flowAsset != _previousFlowAsset)
            {
                // Unregister from previous asset
                if (_previousFlowAsset is not null)
                {
                    _previousFlowAsset.UnregisterController(this);
                }

                // Register with new asset
                if (flowAsset is not null)
                {
                    flowAsset.RegisterController(this);
                }

                _previousFlowAsset = flowAsset;
            }

            // Ensure the controller is initialized for editor mode
            if (!Application.isPlaying)
            {
                if (_animator is null)
                {
                    InitializeAnimator();
                }

                if (flowAsset is not null && _states.Count == 0)
                {
                    InitializeStateMachine();
                }
            }
        }
#endif

        #endregion

        #region Initialization

        /// <summary>
        ///     Initialize the animator component
        /// </summary>
        protected virtual void InitializeAnimator()
        {
            try
            {
                _animator = CreateAnimator();

                if (_animator is null)
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Failed to create animator. Make sure the necessary components exist.",
                        this);

                    return;
                }

                // Create animation context with the animator
                _animationContext = new AnimationContext(_animator, gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] Error initializing animator: {ex.Message}", this);
            }
        }

        /// <summary>
        ///     Initialize the state machine from the flow asset
        /// </summary>
        protected virtual void InitializeStateMachine()
        {
            // Clear existing states
            ClearStates();

            // Make sure we have an animator
            if (_animator is null)
            {
                InitializeAnimator();

                if (_animator is null)
                {
                    Debug.LogError($"[{GetType().Name}] Cannot initialize state machine: No animator available.", this);
                    return;
                }
            }

            // Make sure we have an animation context
            if (_animationContext is null)
            {
                _animationContext = new AnimationContext(_animator, gameObject);
            }

            // Build from asset if available
            if (flowAsset is not null)
            {
                flowAsset.BuildFlowController(this);
            }
            else
            {
                // Fall back to default state initialization if no asset
                InitializeDefaultStates();
            }
        }

        /// <summary>
        ///     Create default states when no flow asset is provided
        ///     Override in derived classes to provide default behavior
        /// </summary>
        protected virtual void InitializeDefaultStates()
        {
            // Base implementation does nothing
            // Override in derived classes to create default states
        }

        #endregion

        #region State Management

        /// <summary>
        ///     Transition to a new animation state
        /// </summary>
        private void TransitionToState(IAnimationState newState)
        {
            if (newState is null)
                return;

            // Exit current state
            _currentState?.OnExit(_animationContext);

            // Enter new state
            _currentState = newState;
            _timeInCurrentState = 0f;
            _currentState.OnEnter(_animationContext);

            if (debugVisualization)
            {
                Debug.Log($"[{GetType().Name}] Transitioned to animation state: {newState.Id}", this);
            }
        }

        /// <summary>
        ///     Add a state to this controller
        /// </summary>
        public void AddState(IAnimationState state)
        {
            if (state is null) return;
            _states[state.Id] = state;
        }

        /// <summary>
        ///     Clear all states from this controller
        /// </summary>
        public void ClearStates()
        {
            _states.Clear();
            _currentState = null;
            _timeInCurrentState = 0f;
        }

        /// <summary>
        ///     Set the initial state ID
        /// </summary>
        public void SetInitialState(string stateId)
        {
            initialStateId = stateId;
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
        ///     Get the current state ID
        /// </summary>
        public string GetCurrentStateId() => _currentState?.Id;

        /// <summary>
        ///     Check if a state with the given ID exists
        /// </summary>
        public bool HasState(string stateId) => _states.ContainsKey(stateId);

        /// <summary>
        ///     Get all state IDs
        /// </summary>
        public IEnumerable<string> GetStateIds() => _states.Keys;

        #endregion

        #region Parameter Management

        /// <summary>
        ///     Set a parameter in the animation context
        /// </summary>
        public void SetParameter<T>(string name, T value)
        {
            if (_animationContext is null)
            {
                // Create context if it doesn't exist yet
                if (_animator is null)
                {
                    InitializeAnimator();
                }

                if (_animator is not null)
                {
                    _animationContext = new AnimationContext(_animator, gameObject);
                }
                else
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Cannot set parameter '{name}' - animation context not initialized.", this);

                    return;
                }
            }

            _animationContext.SetParameter(name, value);
        }

        /// <summary>
        ///     Get a parameter from the animation context
        /// </summary>
        public T GetParameter<T>(string name)
        {
            if (_animationContext is null)
                return default;

            return _animationContext.GetParameter<T>(name);
        }

        /// <summary>
        ///     Check if a parameter exists
        /// </summary>
        public bool HasParameter(string name) => _animationContext is not null && _animationContext.HasParameter(name);

        #endregion

        #region Animation Interface

        /// <summary>
        ///     Get available animations from the controller's animator
        /// </summary>
        public List<string> GetAvailableAnimations()
        {
            if (_animator is null)
            {
                InitializeAnimator();

                if (_animator is null)
                    return new List<string>();
            }

            return _animator.GetAvailableAnimations();
        }

        /// <summary>
        ///     Play a specific animation directly
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            if (_animator is null)
            {
                InitializeAnimator();

                if (_animator is null)
                    return;
            }

            _animator.Play(animationName);
        }

        /// <summary>
        ///     Get the animator instance used by this controller
        /// </summary>
        public IAnimator GetAnimator()
        {
            if (_animator is null)
            {
                InitializeAnimator();
            }

            return _animator;
        }

        /// <summary>
        ///     Create an animator adapter for this controller
        ///     Must be implemented by derived classes
        /// </summary>
        protected abstract IAnimator CreateAnimator();

        #endregion

    }
}
