using System;
using System.Collections.Generic;
using GabrielBigardi.SpriteAnimator;
using Kirby.Abilities.Animation;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Animation data for a specific copy ability
    /// </summary>
    [CreateAssetMenu(fileName = "NewCopyAbilityAnimations", menuName = "Kirby/Copy Ability Animations")]
    public class CopyAbilityAnimationData : ScriptableObject
    {
        [Header("Animation Settings")] [SerializeField]
        private SpriteAnimationObject spriteAnimations;

        [Header("Animation States")] [SerializeField]
        private bool useCustomAnimationStates;

        [SerializeField] private List<string> availableStates = new();

        // Cache of animation state types to create
        private readonly Dictionary<string, Type> _animationStateTypes = new();

        // Initialize the state machine for this ability
        public KirbyAnimationStateMachine CreateStateMachine(KirbyController controller, SpriteAnimator animator)
        {
            // Create a new state machine
            KirbyAnimationStateMachine stateMachine = new(controller, animator);

            // Apply the sprite animations to the animator
            if (spriteAnimations)
            {
                animator.ChangeAnimationObject(spriteAnimations);
            }

            // Populate all animation state types if not already done
            if (_animationStateTypes.Count == 0)
            {
                RegisterDefaultAnimationStates();
            }

            // Create and register animation states
            var statesToRegister = new List<KirbyAnimationState>();

            // If using custom animation states, only create the ones specified
            if (useCustomAnimationStates)
            {
                foreach (string stateName in availableStates)
                {
                    if (_animationStateTypes.TryGetValue(stateName, out Type stateType))
                    {
                        KirbyAnimationState state = Activator.CreateInstance(stateType) as KirbyAnimationState;
                        if (state != null)
                        {
                            statesToRegister.Add(state);
                        }
                    }
                }
            }
            // Otherwise create all default animation states
            else
            {
                foreach (Type stateType in _animationStateTypes.Values)
                {
                    KirbyAnimationState state = Activator.CreateInstance(stateType) as KirbyAnimationState;
                    if (state != null)
                    {
                        statesToRegister.Add(state);
                    }
                }
            }

            // Register all states with the state machine
            stateMachine.RegisterStates(statesToRegister);

            return stateMachine;
        }

        // Register all default animation state types
        private void RegisterDefaultAnimationStates()
        {
            // Basic movement states
            _animationStateTypes["Idle"] = typeof(IdleAnimationState);
            _animationStateTypes["Walk"] = typeof(WalkAnimationState);
            _animationStateTypes["Run"] = typeof(RunAnimationState);

            // Jump states
            _animationStateTypes["JumpStart"] = typeof(JumpStartAnimationState);
            _animationStateTypes["JumpRelease"] = typeof(JumpReleaseAnimationState);
            _animationStateTypes["Fall"] = typeof(FallAnimationState);

            // Fly states
            _animationStateTypes["Fly"] = typeof(FlyAnimationState);
            _animationStateTypes["Float"] = typeof(FloatAnimationState);

            // Add any other animation states here
        }

        // Editor method to populate available states
        public void PopulateAvailableStates()
        {
            availableStates.Clear();
            RegisterDefaultAnimationStates();
            availableStates.AddRange(_animationStateTypes.Keys);
        }
    }
}
