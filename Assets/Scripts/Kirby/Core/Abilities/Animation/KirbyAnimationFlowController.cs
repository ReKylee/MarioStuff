using System.Collections.Generic;
using Animation.Flow;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Adapter that connects Kirby's specific inputs and physics to the generic animation flow system
    /// </summary>
    [RequireComponent(typeof(KirbyController))]
    public class KirbyAnimationFlowController : AnimationFlowController
    {

        [Header("Kirby Jump Animation Settings")] [SerializeField]
        private AnimationFlowAsset _jumpAnimationFlow;

        [SerializeField] private float _jumpApexThreshold = 2.0f;
        private readonly float _fallingThreshold = -0.5f;
        private readonly float _jumpStartThreshold = 3.0f;
        private readonly float _longFallTime = 0.5f;
        private InputContext _currentInput;
        private float _fallStartTime;
        private bool _isFallingLongEnough;

        private KirbyController _kirbyController;

        // Streamlined Awake method
        protected new void Awake()
        {
            _kirbyController = GetComponent<KirbyController>();
            if (!_kirbyController)
            {
                Debug.LogError("KirbyAnimationFlowController requires a KirbyController component.", this);
                enabled = false;
                return;
            }

            if (_jumpAnimationFlow)
            {
                SetAnimationFlowAsset(_jumpAnimationFlow);
            }

            base.Awake();
        }

        private new void Update()
        {
            // Update animation parameters based on Kirby's state
            UpdateKirbyParameters();

            // Let the base controller handle animation state transitions
            base.Update();
        }

        public void SetAnimationFlowAsset(AnimationFlowAsset flowAsset)
        {
            // Clear existing states
            ClearStates();

            // Build flow controller from asset
            flowAsset.BuildFlowController(this);
        }

        public void SetInput(InputContext input)
        {
            _currentInput = input;
        }

        private void UpdateKirbyParameters()
        {
            if (!_kirbyController || !_kirbyController.Rigidbody)
                return;

            // Get Kirby's velocity
            float verticalVelocity = _kirbyController.Rigidbody.linearVelocity.y;

            // Update parameters for jump animation states
            SetParameter("VerticalVelocity", verticalVelocity);
            SetParameter("IsGrounded", _kirbyController.IsGrounded);

            // For jump states
            SetParameter("JumpHeld", _currentInput.JumpHeld);
            SetParameter("JumpPressed", _currentInput.JumpPressed);
            SetParameter("JumpReleased", _currentInput.JumpReleased);

            // Track long fall
            if (!_kirbyController.IsGrounded && verticalVelocity < _fallingThreshold)
            {
                if (!_isFallingLongEnough)
                {
                    float fallTime = Time.time - _fallStartTime;
                    if (fallTime >= _longFallTime)
                    {
                        _isFallingLongEnough = true;
                    }
                }
            }
            else if (_kirbyController.IsGrounded)
            {
                _fallStartTime = Time.time;
                _isFallingLongEnough = false;
            }

            // Update long fall parameter
            SetParameter("IsLongFall", _isFallingLongEnough);

            // Calculate jump animation phase
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


        /// <summary>
        ///     Factory for creating Kirby's jump animation flow
        /// </summary>
        public class KirbyJumpAnimationFactory
        {
            /// <summary>
            ///     Create a default jump animation flow programmatically
            /// </summary>
            public static AnimationFlowAsset CreateJumpAnimationFlow()
            {
                AnimationFlowAsset asset = ScriptableObject.CreateInstance<AnimationFlowAsset>();

                // Create state data
                AnimationStateData jumpStartState = new()
                {
                    Id = "JumpStart",
                    StateType = "HoldFrame",
                    AnimationName = "JumpStart",
                    IsInitialState = true,
                    Position = new Vector2(100, 100)
                };

                AnimationStateData jumpState = new()
                {
                    Id = "Jump",
                    StateType = "OneTime",
                    AnimationName = "Jump",
                    Position = new Vector2(300, 100)
                };

                AnimationStateData fallState = new()
                {
                    Id = "Fall",
                    StateType = "Looping",
                    AnimationName = "Fall",
                    Position = new Vector2(500, 100)
                };

                AnimationStateData bounceState = new()
                {
                    Id = "Bounce",
                    StateType = "OneTime",
                    AnimationName = "BounceOffFloor",
                    Position = new Vector2(500, 300)
                };

                AnimationStateData landState = new()
                {
                    Id = "Land",
                    StateType = "OneTime",
                    AnimationName = "Land",
                    Position = new Vector2(300, 300)
                };

                // Add states to asset
                asset.States.Add(jumpStartState);
                asset.States.Add(jumpState);
                asset.States.Add(fallState);
                asset.States.Add(bounceState);
                asset.States.Add(landState);

                // Create transitions
                // JumpStart -> Jump (when button released or near apex)
                TransitionData jumpStartToJump = new()
                {
                    FromStateId = jumpStartState.Id,
                    ToStateId = jumpState.Id,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = "AnyCondition",
                            ParameterName = ""
                        }
                    }
                };

                jumpStartToJump.Conditions.Add(new ConditionData
                {
                    Type = "Bool",
                    ParameterName = "JumpReleased",
                    BoolValue = true
                });

                jumpStartToJump.Conditions.Add(new ConditionData
                {
                    Type = "FloatLessThan",
                    ParameterName = "VerticalVelocity",
                    FloatValue = 3.0f
                });

                // Jump -> Fall (when falling)
                TransitionData jumpToFall = new()
                {
                    FromStateId = jumpState.Id,
                    ToStateId = fallState.Id,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = "FloatLessThan",
                            ParameterName = "VerticalVelocity",
                            FloatValue = -0.5f
                        }
                    }
                };

                // Fall -> Bounce (when long fall and near ground)
                TransitionData fallToBounce = new()
                {
                    FromStateId = fallState.Id,
                    ToStateId = bounceState.Id,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = "Bool",
                            ParameterName = "IsLongFall",
                            BoolValue = true
                        }
                    }
                };

                // Fall -> Land (when grounded and not long fall)
                TransitionData fallToLand = new()
                {
                    FromStateId = fallState.Id,
                    ToStateId = landState.Id,
                    Conditions = new List<ConditionData>
                    {
                        new()
                        {
                            Type = "Bool",
                            ParameterName = "IsGrounded",
                            BoolValue = true
                        }
                    }
                };

                // Add transitions to asset
                asset.Transitions.Add(jumpStartToJump);
                asset.Transitions.Add(jumpToFall);
                asset.Transitions.Add(fallToBounce);
                asset.Transitions.Add(fallToLand);

                return asset;
            }
        }
    }
}
