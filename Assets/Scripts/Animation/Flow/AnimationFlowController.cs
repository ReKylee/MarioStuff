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

        private IAnimator _animatorAdapter; // Renamed for clarity from _animator
        private IAnimationState _currentState;
        private float _timeInCurrentState;

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

        // Initialization for Enter Play Mode Options support
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            // Reset any static data here that needs to be reset between domain reloads
            // This is important when using Enter Play Mode Options with domain reload disabled
        }

        private void InitializeController()
        {
            _animatorAdapter = GetAnimatorAdapter();
            if (_animatorAdapter == null)
            {
                Debug.LogError("Animator adapter is not provided by the child class.", this);
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

        protected abstract IAnimator GetAnimatorAdapter();

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
