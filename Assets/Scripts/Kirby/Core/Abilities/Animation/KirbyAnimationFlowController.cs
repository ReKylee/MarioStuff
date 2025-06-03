using System;
using System.Collections.Generic;
using Animation.Flow;
using Animation.Flow.Adapters;
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

        #region Animation Flow Control

        /// <summary>
        ///     Set the animation flow asset
        /// </summary>
        public void SetAnimationFlowAsset(AnimationFlowAsset flowAsset)
        {
            FlowAsset = flowAsset;
        }


        /// <summary>
        ///     Create default animation states when no flow asset is provided
        /// </summary>
        protected override void InitializeDefaultStates()
        {
            // Use the factory to create a default jump animation flow
            AnimationFlowAsset defaultFlow = KirbyJumpAnimationFactory.CreateJumpAnimationFlow();

            // Build the flow controller from the default flow
            defaultFlow.BuildFlowController(this);
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

    /// <summary>
    ///     Factory for creating Kirby's jump animation flow
    /// </summary>
    public static class KirbyJumpAnimationFactory
    {
        // State IDs
        private static readonly string JumpStartStateId = Guid.NewGuid().ToString();
        private static readonly string JumpStateId = Guid.NewGuid().ToString();
        private static readonly string FallStateId = Guid.NewGuid().ToString();
        private static readonly string BounceStateId = Guid.NewGuid().ToString();
        private static readonly string LandStateId = Guid.NewGuid().ToString();

        /// <summary>
        ///     Create a default jump animation flow programmatically
        /// </summary>
        public static AnimationFlowAsset CreateJumpAnimationFlow()
        {
            AnimationFlowAsset asset = ScriptableObject.CreateInstance<AnimationFlowAsset>();

            // Create and add states
            asset.states.AddRange(CreateStates());

            // Create and add transitions
            asset.transitions.AddRange(CreateTransitions());

            return asset;
        }

        /// <summary>
        ///     Create the states for the jump animation flow
        /// </summary>
        private static List<AnimationStateData> CreateStates() =>
            new()
            {
                // Jump start state (hold frame)
                new AnimationStateData
                {
                    Id = JumpStartStateId,
                    StateType = AnimationStateType.HoldFrame.ToString(),
                    AnimationName = "JumpStart",
                    IsInitialState = true,
                    Position = new Vector2(100, 100)
                },

                // Jump state (one time)
                new AnimationStateData
                {
                    Id = JumpStateId,
                    StateType = AnimationStateType.OneTime.ToString(),
                    AnimationName = "Jump",
                    Position = new Vector2(300, 100)
                },

                // Fall state (looping)
                new AnimationStateData
                {
                    Id = FallStateId,
                    StateType = AnimationStateType.Looping.ToString(),
                    AnimationName = "Fall",
                    Position = new Vector2(500, 100)
                },

                // Bounce state (one time)
                new AnimationStateData
                {
                    Id = BounceStateId,
                    StateType = AnimationStateType.OneTime.ToString(),
                    AnimationName = "BounceOffFloor",
                    Position = new Vector2(500, 300)
                },

                // Land state (one time)
                new AnimationStateData
                {
                    Id = LandStateId,
                    StateType = AnimationStateType.OneTime.ToString(),
                    AnimationName = "Land",
                    Position = new Vector2(300, 300)
                }
            };

        /// <summary>
        ///     Create the transitions for the jump animation flow
        /// </summary>
        private static List<TransitionData> CreateTransitions() =>
            new()
            {
                // JumpStart -> Jump (when button released or near apex)
                new TransitionData
                {
                    FromStateId = JumpStartStateId,
                    ToStateId = JumpStateId,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = ConditionType.AnyCondition.ToString(),
                            ParameterName = ""
                        },
                        new()
                        {
                            Type = ConditionType.Bool.ToString(),
                            ParameterName = "JumpReleased",
                            BoolValue = true
                        },
                        new()
                        {
                            Type = ConditionType.FloatLessThan.ToString(),
                            ParameterName = "VerticalVelocity",
                            FloatValue = 3.0f
                        }
                    }
                },

                // Jump -> Fall (when falling)
                new TransitionData
                {
                    FromStateId = JumpStateId,
                    ToStateId = FallStateId,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = ConditionType.FloatLessThan.ToString(),
                            ParameterName = "VerticalVelocity",
                            FloatValue = -0.5f
                        }
                    }
                },

                // Fall -> Bounce (when long fall and near ground)
                new TransitionData
                {
                    FromStateId = FallStateId,
                    ToStateId = BounceStateId,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = ConditionType.Bool.ToString(),
                            ParameterName = "IsLongFall",
                            BoolValue = true
                        },
                        new()
                        {
                            Type = ConditionType.Bool.ToString(),
                            ParameterName = "IsGrounded",
                            BoolValue = true
                        }
                    }
                },

                // Fall -> Land (when grounded and not long fall)
                new TransitionData
                {
                    FromStateId = FallStateId,
                    ToStateId = LandStateId,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = ConditionType.Bool.ToString(),
                            ParameterName = "IsGrounded",
                            BoolValue = true
                        }
                    }
                }
            };
    }


}
