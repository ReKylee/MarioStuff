using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Animation.Flow
{
    /// <summary>
    ///     Main controller for the animation flow system
    /// </summary>
    public abstract class AnimationFlowController : MonoBehaviour
    {
        [SerializeField] private string _initialStateId;
        [SerializeField] private AnimationFlowAsset _flowAsset;
        [SerializeField] private bool _debugVisualization;

        private readonly Dictionary<string, IAnimationState> _states = new();
        private readonly Dictionary<string, float> _stateTimers = new();
        private AnimationContext _animationContext; // Renamed for clarity from _context

        private IAnimator _animatorAdapter;
        private IAnimationState _currentState;

        // For tracking the previous asset to handle unregistering
        private AnimationFlowAsset _previousFlowAsset;
        private float _timeInCurrentState;

        // Public property to get/set the flow asset with proper registration handling
        public AnimationFlowAsset FlowAsset
        {
            get => _flowAsset;
            set
            {
#if UNITY_EDITOR
                // Unregister from previous asset
                if (_flowAsset)
                {
                    _flowAsset.UnregisterController(this);
                }

                // Assign new asset
                _flowAsset = value;

                // Register with new asset
                if (_flowAsset)
                {
                    _flowAsset.RegisterController(this);
                }

                // If in editor, rebuild the controller with the new asset
                if (!Application.isPlaying)
                {
                    // Only rebuild if the asset is valid
                    if (_flowAsset)
                    {
                        _flowAsset.BuildFlowController(this);
                    }
                    else
                    {
                        ClearStates();
                    }
                }
#else
                _flowAsset = value;
#endif
            }
        }

        protected void Awake()
        {
            InitializeController();
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

        protected void Update()
        {
            if (_currentState == null)
                return;

            // Update time in current state
            _timeInCurrentState += Time.deltaTime;
            _animationContext.SetParameter("StateTime", _timeInCurrentState);

            // Update current state
            _currentState.OnUpdate(_animationContext, Time.deltaTime);

            // Check for transitions
            string nextStateId = _currentState.CheckTransitions(_animationContext);
            if (!string.IsNullOrEmpty(nextStateId) && _states.TryGetValue(nextStateId, out IAnimationState nextState))
            {
                TransitionToState(nextState);
            }
        }

        private void OnEnable()
        {
            // Ensure we're initialized when object is enabled/re-enabled
            if (_animatorAdapter == null)
            {
                InitializeController();
            }
        }

        // Handle controller destruction - unregister from asset
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (_flowAsset != null)
            {
                _flowAsset.UnregisterController(this);
            }
#endif
        }

        private void OnGUI()
        {
            if (!_debugVisualization || _currentState == null)
                return;

            // Draw debug information in the game view
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUI.color = Color.black;
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.color = Color.white;

            GUILayout.Label($"Current State: {_currentState.Id} ({_currentState.AnimationName})");
            GUILayout.Label($"Time in State: {_timeInCurrentState:F2}s");

            // Show available transitions
            string nextStateId = _currentState.CheckTransitions(_animationContext);
            if (!string.IsNullOrEmpty(nextStateId) && _states.TryGetValue(nextStateId, out IAnimationState nextState))
            {
                GUILayout.Label($"Next Transition: → {nextState.Id}");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // Called in OnValidate to handle inspector changes
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Handle flow asset changes in the inspector
            if (_flowAsset != _previousFlowAsset)
            {
                // Unregister from previous asset
                if (_previousFlowAsset != null)
                {
                    _previousFlowAsset.UnregisterController(this);
                }

                // Register with new asset
                if (_flowAsset != null)
                {
                    _flowAsset.RegisterController(this);
                }

                _previousFlowAsset = _flowAsset;
            }
        }
#endif

        /// <summary>
        ///     Get the animator adapter for this controller.
        ///     Must be implemented by derived classes to provide an appropriate IAnimator implementation.
        /// </summary>
        /// <returns>The animator adapter that will be used for animation control</returns>
        protected abstract IAnimator CreateAnimatorAdapter();

        /// <summary>
        ///     Public method to expose the animator adapter to the editor and other components
        /// </summary>
        public IAnimator GetAnimator()
        {
            _animatorAdapter ??= CreateAnimatorAdapter();

            if (_animatorAdapter is null)
            {
                Debug.LogWarning(
                    $"CreateAnimatorAdapter returned null in {GetType().Name}. Animation list may be empty.", this);
            }

            return _animatorAdapter;
        }

        // Initialization for Enter Play Mode Options support
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            // Reset any static data here that needs to be reset between domain reloads
            // This is important when using Enter Play Mode Options with domain reload disabled
        }

        private void InitializeController()
        {
            _animatorAdapter = GetAnimator();

            if (_animatorAdapter == null)
            {
                Debug.LogError(
                    "Animator adapter was not provided by CreateAnimatorAdapter(). Override this method in your derived class.",
                    this);

                enabled = false;
                return;
            }

            _animationContext = new AnimationContext(_animatorAdapter, gameObject);

            if (_flowAsset)
            {
                _flowAsset.BuildFlowController(this);
            }
            else
            {
                InitializeStates();
            }
        }

        /// <summary>
        ///     Transition to a new animation state
        /// </summary>
        private void TransitionToState(IAnimationState newState)
        {
            // Exit current state
            _currentState?.OnExit(_animationContext);

            // Enter new state
            _currentState = newState;
            _timeInCurrentState = 0f;
            _currentState.OnEnter(_animationContext);

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
        ///     Clear all states from this controller
        /// </summary>
        public void ClearStates()
        {
            _states.Clear();
            _currentState = null;
        }

        /// <summary>
        ///     Set the initial state ID
        /// </summary>
        public void SetInitialState(string stateId)
        {
            _initialStateId = stateId;
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
            _animationContext.SetParameter(name, value);
        }

        /// <summary>
        ///     Get a parameter from the animation context
        /// </summary>
        public T GetParameter<T>(string name) => _animationContext.GetParameter<T>(name);

        /// <summary>
        ///     Initialize animation states - this would be replaced by a configuration system
        /// </summary>
        protected virtual void InitializeStates()
        {
            // Override in derived classes or replace with configuration loading
        }
    }
}
