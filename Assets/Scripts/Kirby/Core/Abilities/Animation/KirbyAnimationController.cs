using GabrielBigardi.SpriteAnimator;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Handles Kirby animations by monitoring KirbyController state and input
    /// </summary>
    [RequireComponent(typeof(SpriteAnimator))]
    [RequireComponent(typeof(KirbyController))]
    public class KirbyAnimationController : MonoBehaviour
    {

        #region Settings

        [Header("Animation Settings")] [SerializeField]
        private AnimationSettings settings = new();

        #endregion

        private KirbyAbilityStateHandler _abilityStateHandler;
        private KirbyAnimator _kirbyAnimator;
        private KirbyPhysicsController _physicsController;
        private KirbyAnimationStateMachine _stateMachine;

        // SOLID Refactored components
        private AnimationStateTracker _stateTracker;

        #region Animation Events

        public void OnAnimationComplete()
        {
            // Delegate to the state machine
            _stateMachine.OnAnimationComplete();
        }

        #endregion

        #region Components

        [Header("Components")] [SerializeField]
        private SpriteAnimator animator;

        [SerializeField] private KirbyController kirbyController;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private SpriteRenderer spriteRenderer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Get components
            if (!animator) animator = GetComponent<SpriteAnimator>();
            if (!kirbyController) kirbyController = GetComponent<KirbyController>();
            if (!inputHandler) inputHandler = GetComponent<InputHandler>();
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            _stateTracker = new AnimationStateTracker();
            _kirbyAnimator = new KirbyAnimator(animator, kirbyController, _stateTracker, spriteRenderer, this);
            _stateMachine = new KirbyAnimationStateMachine(_stateTracker, kirbyController, _kirbyAnimator, settings,
                kirbyController.GroundLayers);

            _physicsController = new KirbyPhysicsController(kirbyController, _stateTracker, settings);
            _abilityStateHandler = new KirbyAbilityStateHandler(_stateTracker);

            // Make sure we're subscribed to animation complete events
            if (animator)
            {
                animator.OnAnimationComplete -= OnAnimationComplete; // Avoid double subscription
                animator.OnAnimationComplete += OnAnimationComplete;
            }

        }

        private void Start()
        {
            // Set initial state
            _stateTracker.ChangeState(AnimState.Idle);
            _kirbyAnimator.PlayStateAnimation(_stateTracker.CurrentState);
        }

        private void Update()
        {
            if (!inputHandler || !kirbyController) return;

            // Track ground and vertical state
            _physicsController.TrackGroundState(transform);
            _physicsController.TrackVerticalState(transform);

            // Track ability states
            _abilityStateHandler.TrackInhaleState(inputHandler.CurrentInput);

            // Update animation state based on input and character state
            _stateMachine.UpdateState(inputHandler.CurrentInput, kirbyController.IsGrounded, kirbyController.Velocity);

            // Update sprite direction based on input
            _kirbyAnimator.UpdateSpriteDirection(inputHandler.CurrentInput, settings.moveInputThreshold);

            // Update animation timers
            _stateTracker.UpdateTimers(Time.deltaTime);

            // Ensure the animation is playing for the current state
            _kirbyAnimator.PlayStateAnimation(_stateTracker.CurrentState);
            // Don't subscribe to event every frame as it causes issues
        }

        private void FixedUpdate()
        {
            // Apply physics forces for certain states
            _physicsController.ApplyAnimationPhysics();
        }

        private void OnDestroy()
        {
            // Clean up event handlers
            if (animator)
            {
                animator.OnAnimationComplete -= OnAnimationComplete;
            }

            _kirbyAnimator.Cleanup();
        }

        #endregion

    }
}
