using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Interfaces;
using Animation.Flow.States;
using UnityEngine;

// Ensure this is the new Conditions namespace
#if UNITY_EDITOR
#endif

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Serializable asset that stores an animation flow configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimationFlow", menuName = "Animation/Flow Asset", order = 120)]
    public class AnimationFlowAsset : ScriptableObject
    {
        [Tooltip("All states in this animation flow")]
        public List<AnimationStateData> states = new();

        [Tooltip("All transitions between states")] [SerializeReference]
        public List<TransitionData> transitions = new();

        // List of controllers that use this asset (not serialized, only for editor usage)
        [NonSerialized] private AnimationFlowController _controller;

        /// <summary>
        ///     Validate this asset to ensure all references are correct
        /// </summary>
        private void Validate()
        {
            // Validate states
            HashSet<string> stateIds = new();
            bool hasInitialState = false;

            foreach (AnimationStateData state in states)
            {
                // Ensure state IDs are unique
                if (string.IsNullOrEmpty(state.Id))
                {
                    state.Id = Guid.NewGuid().ToString();
                }
                else if (stateIds.Contains(state.Id))
                {
                    // Generate a new ID if duplicate found
                    state.Id = Guid.NewGuid().ToString();
                }

                stateIds.Add(state.Id);

                // Track if we have an initial state
                if (state.IsInitialState)
                {
                    hasInitialState = true;
                }
            }

            // Ensure we have exactly one initial state
            if (states.Count > 0 && !hasInitialState)
            {
                // If no initial state, set the first one as initial
                states[0].IsInitialState = true;
            }
            else if (states.Count > 0)
            {
                // Make sure only one state is marked as initial
                bool foundInitial = false;
                foreach (AnimationStateData state in states)
                {
                    if (state.IsInitialState)
                    {
                        if (foundInitial)
                        {
                            // If we already found an initial state, unmark this one
                            state.IsInitialState = false;
                        }
                        else
                        {
                            foundInitial = true;
                        }
                    }
                }
            }

            // Validate transitions
            if (transitions != null)
            {
                foreach (TransitionData transition in transitions)
                {
                    if (transition == null) continue;

                    // Ensure from and to states exist
                    bool fromExists = !string.IsNullOrEmpty(transition.FromStateId) &&
                                      stateIds.Contains(transition.FromStateId);

                    bool toExists = !string.IsNullOrEmpty(transition.ToStateId) &&
                                    stateIds.Contains(transition.ToStateId);

                    if (!fromExists || !toExists)
                    {
                        Debug.LogWarning(
                            $"Invalid transition in {name}: {(fromExists ? "" : "Source")} {(fromExists ? "and" : "")} {(toExists ? "" : "Target")} state(s) not found.");
                    }
                }
            }
        }

        /// <summary>
        ///     Create a runtime flow controller from this asset
        /// </summary>
        public void BuildFlowController(AnimationFlowController controller)
        {
            if (controller is null) return;

#if UNITY_EDITOR
            // Register the controller when building from this asset
            RegisterController(controller);
#endif

            // Clear existing states
            controller.ClearStates();

            // Validate asset to ensure integrity
            Validate();

            // Create dictionary to map state IDs to actual state instances
            var stateMap = new Dictionary<string, IAnimationState>();

            // Create and add states
            foreach (AnimationStateData stateData in states)
            {
                // Create state based on its type
                IAnimationState state = CreateStateFromData(stateData);

                // Add to controller
                controller.AddState(state);

                // Track for transition creation
                stateMap[stateData.Id] = state;

                // Set initial state if this is marked as such
                if (stateData.IsInitialState)
                {
                    controller.SetInitialState(stateData.Id);
                }
            }

            // Create transitions
            foreach (TransitionData transitionData in transitions)
            {
                if (transitionData is null) continue;
                if (stateMap.TryGetValue(transitionData.FromStateId, out IAnimationState fromState) &&
                    stateMap.TryGetValue(transitionData.ToStateId, out IAnimationState toState))
                {
                    // Get the base state to add the transition
                    if (fromState is AnimationStateBase baseState)
                    {
                        // Create the transition
                        AnimationTransition transition = baseState.TransitionTo(toState.Id);

                        // Add conditions
                        if (transitionData.Conditions != null)
                        {
                            foreach (ConditionData conditionDataNewType in transitionData.Conditions)
                            {
                                if (conditionDataNewType != null)
                                {
                                    AddConditionToTransition(transition, conditionDataNewType);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Create an animation state from serialized data
        /// </summary>
        private static IAnimationState CreateStateFromData(AnimationStateData stateData)
        {
            AnimationStateType stateType = stateData.GetStateType();

            switch (stateType)
            {
                case AnimationStateType.HoldFrame:
                    return new HoldFrameState(stateData.Id, stateData.AnimationName, stateData.FrameToHold);
                case AnimationStateType.OneTime:
                    return new OneTimeState(stateData.Id, stateData.AnimationName);
                case AnimationStateType.Looping:
                    return new LoopingState(stateData.Id, stateData.AnimationName);
                default:
                    Debug.LogWarning($"Unknown state type: {stateType}, creating default OneTime state");
                    return new OneTimeState(stateData.Id, stateData.AnimationName);
            }
        }

        /// <summary>
        ///     Add a condition to a transition based on serialized data
        /// </summary>
        private static void AddConditionToTransition(AnimationTransition transition, ConditionData conditionData)
        {
            if (transition == null || conditionData == null)
                return;

            // Use the ConditionFactory to create a condition from the serialized data
            ICondition condition = ConditionFactory.CreateCondition(conditionData);
            if (condition != null)
            {
                transition.AddCondition(condition);
            }
        }


        /// <summary>
        ///     Create a new animation flow asset from a template
        /// </summary>
        public static AnimationFlowAsset CreateFromTemplate(string templateName) => default;

#if UNITY_EDITOR
        /// <summary>
        ///     Called when this asset is assigned to a controller
        /// </summary>
        public void RegisterController(AnimationFlowController controller)
        {
            if (controller is not null)
            {
                _controller = controller;
            }
        }

        /// <summary>
        ///     Called when this asset is unassigned from a controller
        /// </summary>
        public void UnregisterController(AnimationFlowController controller)
        {
            if (controller is not null)
            {
                _controller = null;
            }
        }

        /// <summary>
        ///     Get a controller that uses this asset (returns the first valid one)
        /// </summary>
        public AnimationFlowController GetController() => _controller;


        private void OnValidate()
        {
            // Validate the asset whenever it changes in the editor
            Validate();
        }
#endif
    }


}
