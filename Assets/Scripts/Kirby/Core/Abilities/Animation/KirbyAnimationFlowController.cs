using Animation.Flow.Adapters;
using Animation.Flow.Core;
using Animation.Flow.Interfaces;
using GabrielBigardi.SpriteAnimator;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Animation flow controller specific to Kirby character
    ///     Connects Kirby's inputs and physics to the animation flow system
    /// </summary>
    [RequireComponent(typeof(KirbyController))]
    public class KirbyAnimationFlowController : AnimationFlowController
    {
        [Header("Kirby Animation Settings")] [SerializeField]
        private AnimationFlowAsset _jumpAnimationFlow;

        [SerializeField] private float _jumpApexThreshold = 2.0f;
        [SerializeField] private float _fallingThreshold = -0.5f;
        [SerializeField] private float _jumpStartThreshold = 3.0f;
        [SerializeField] private float _longFallTime = 0.5f;

        // Input values
        private readonly InputContext _currentInput = new();
        private SpriteAnimatorAdapter _animatorAdapter;

        // Long fall tracking
        private float _fallStartTime;
        private bool _isFallingLongEnough;

        // Required components
        private KirbyController _kirbyController;
        private SpriteAnimator _spriteAnimator;

        #region Animation Flow Control

        /// <summary>
        ///     Set the animation flow asset
        /// </summary>
        public void SetAnimationFlowAsset(AnimationFlowAsset flowAsset)
        {
            FlowAsset = flowAsset;
        }

        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            // Initialize components first
            InitializeComponents();

            // Apply jump animation flow if provided
            if (_jumpAnimationFlow is not null)
            {
                SetAnimationFlowAsset(_jumpAnimationFlow);
            }

            // Call base implementation
            base.Awake();
        }

        protected override void Update()
        {
            // Update animation parameters based on Kirby's current state
            UpdateAnimationParameters();

            // Let the base controller handle animation state transitions
            base.Update();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // Initialize components if needed
            if (_kirbyController is null || _spriteAnimator is null)
            {
                InitializeComponents();
            }

            // Call base implementation
            base.OnValidate();
        }
#endif

        #endregion

        #region Initialization

        /// <summary>
        ///     Initialize required components, can be called from editor or runtime
        /// </summary>
        private void InitializeComponents()
        {
            // Get required Kirby controller
            _kirbyController = GetComponent<KirbyController>();
            if (_kirbyController is null)
            {
                Debug.LogError($"[{GetType().Name}] Missing KirbyController component.", this);
                enabled = false;
                return;
            }

            // Get required sprite animator
            _spriteAnimator = GetComponent<SpriteAnimator>();
            if (_spriteAnimator is null)
            {
                Debug.LogError($"[{GetType().Name}] Missing SpriteAnimator component.", this);
                enabled = false;
            }
        }

        /// <summary>
        ///     Create the animator adapter for the animation flow system
        /// </summary>
        protected override IAnimator CreateAnimator()
        {
            // Make sure components are initialized
            if (_spriteAnimator is null)
            {
                InitializeComponents();

                // If still null after initialization, we can't create an adapter
                if (_spriteAnimator is null)
                {
                    return null;
                }
            }

            // Create and cache the adapter
            _animatorAdapter = new SpriteAnimatorAdapter(_spriteAnimator);
            return _animatorAdapter;
        }

        #endregion

        #region Animation Parameters

        /// <summary>
        ///     Update animation parameters based on Kirby's current state
        /// </summary>
        private void UpdateAnimationParameters()
        {
            if (_kirbyController?.Rigidbody is null)
                return;

            // Get Kirby's current velocity
            float verticalVelocity = _kirbyController.Rigidbody.linearVelocity.y;

            // Set physics-based parameters
            SetParameter("VerticalVelocity", verticalVelocity);
            SetParameter("IsGrounded", _kirbyController.IsGrounded);
            SetParameter("isRunning", _kirbyController.CurrentInput.RunInput);
            // Set input-based parameters
            SetParameter("JumpHeld", _currentInput.JumpHeld);
            SetParameter("JumpPressed", _currentInput.JumpPressed);
            SetParameter("JumpReleased", _currentInput.JumpReleased);

            // Track long fall
            UpdateLongFallTracking(verticalVelocity);

            // Calculate jump animation phase
            UpdateJumpPhase(verticalVelocity);
        }

        /// <summary>
        ///     Track long fall state for special landing animations
        /// </summary>
        private void UpdateLongFallTracking(float verticalVelocity)
        {
            switch (_kirbyController.IsGrounded)
            {
                case false when verticalVelocity < _fallingThreshold:
                {
                    if (!_isFallingLongEnough)
                    {
                        float fallTime = Time.time - _fallStartTime;
                        if (fallTime >= _longFallTime)
                        {
                            _isFallingLongEnough = true;
                        }
                    }

                    break;
                }
                case true:
                    _fallStartTime = Time.time;
                    _isFallingLongEnough = false;
                    break;
            }

            // Update parameter
            SetParameter("IsLongFall", _isFallingLongEnough);
        }

        /// <summary>
        ///     Calculate and set the current jump phase parameter
        /// </summary>
        private void UpdateJumpPhase(float verticalVelocity)
        {
            string jumpPhase = "None";

            if (!_kirbyController.IsGrounded)
            {
                if (verticalVelocity > _jumpStartThreshold)
                {
                    jumpPhase = "Rising";
                }
                else if (verticalVelocity >= _fallingThreshold)
                {
                    jumpPhase = "Apex";
                }
                else
                {
                    jumpPhase = "Falling";
                }
            }

            SetParameter("JumpPhase", jumpPhase);
        }

        #endregion

    }


}
