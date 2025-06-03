using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.States;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Animation.Flow
{
    /// <summary>
    ///     Serializable asset that stores an animation flow configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimationFlow", menuName = "Animation/Flow Asset", order = 120)]
    public class AnimationFlowAsset : ScriptableObject
    {
        [Tooltip("All states in this animation flow")]
        public List<AnimationStateData> states = new();

        [Tooltip("All transitions between states")]
        public List<TransitionData> transitions = new();

        // List of controllers that use this asset (not serialized, only for editor usage)
        [NonSerialized] private readonly List<AnimationFlowController> _controllers = new();

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
            foreach (TransitionData transition in transitions)
            {
                // Ensure from and to states exist
                bool fromExists = stateIds.Contains(transition.FromStateId);
                bool toExists = stateIds.Contains(transition.ToStateId);

                if (!fromExists || !toExists)
                {
                    Debug.LogWarning(
                        $"Invalid transition in {name}: {(fromExists ? "" : "Source")} {(fromExists ? "and" : "")} {(toExists ? "" : "Target")} state(s) not found.");
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
                if (stateMap.TryGetValue(transitionData.FromStateId, out IAnimationState fromState) &&
                    stateMap.TryGetValue(transitionData.ToStateId, out IAnimationState toState))
                {
                    // Get the base state to add the transition
                    if (fromState is AnimationStateBase baseState)
                    {
                        // Create the transition
                        AnimationTransition transition = baseState.TransitionTo(toState.Id);

                        // Add conditions
                        foreach (ConditionData condition in transitionData.Conditions)
                        {
                            AddConditionToTransition(transition, condition);
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
            if (transition is null || conditionData is null)
                return;

            ConditionType conditionType = conditionData.GetConditionType();

            try
            {
                ITransitionCondition condition = conditionType switch
                {
                    ConditionType.Bool => new BoolCondition(conditionData.ParameterName, conditionData.BoolValue),

                    ConditionType.FloatEquals => new FloatRangeCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue - 0.0001f,
                        conditionData.FloatValue + 0.0001f),

                    ConditionType.FloatLessThan => new FloatRangeCondition(
                        conditionData.ParameterName,
                        float.MinValue,
                        conditionData.FloatValue),

                    ConditionType.FloatGreaterThan => new FloatRangeCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue,
                        float.MaxValue),

                    ConditionType.StringEquals => new StringEqualsCondition(
                        conditionData.ParameterName,
                        conditionData.StringValue),

                    ConditionType.AnimationComplete => new AnimationCompleteCondition(),

                    ConditionType.TimeElapsed => new TimeElapsedCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue),

                    ConditionType.AnyCondition => new AnyCondition(new BoolCondition("__placeholder__")),

                    ConditionType.AllCondition => new AllCondition(new BoolCondition("__placeholder__")),

                    _ => null
                };

                // Log a warning for unknown condition types
                if (condition is null)
                {
                    Debug.LogWarning($"Unknown condition type: {conditionType}");
                    return;
                }

                transition.AddCondition(condition);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating condition of type {conditionType}: {ex.Message}");
                // Create a fallback condition that will never be satisfied
                transition.AddCondition(new BoolCondition("__error__", false));
            }
        }

        /// <summary>
        ///     Create a new animation flow asset from a template
        /// </summary>
        public static AnimationFlowAsset CreateFromTemplate(string templateName)
        {
            AnimationFlowAsset asset = CreateInstance<AnimationFlowAsset>();

            // Add some default states based on the template
            switch (templateName)
            {
                case "BasicLoop":
                    // Simple Idle loop template
                    AnimationStateData idleState = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        StateType = "Looping",
                        AnimationName = "Idle",
                        IsInitialState = true,
                        Position = new Vector2(100, 100)
                    };

                    asset.states.Add(idleState);
                    break;

                case "IdleWalk":
                    // Idle-Walk transition template
                    string idleId = Guid.NewGuid().ToString();
                    string walkId = Guid.NewGuid().ToString();

                    AnimationStateData idle = new()
                    {
                        Id = idleId,
                        StateType = "Looping",
                        AnimationName = "Idle",
                        IsInitialState = true,
                        Position = new Vector2(100, 100)
                    };

                    AnimationStateData walk = new()
                    {
                        Id = walkId,
                        StateType = "Looping",
                        AnimationName = "Walk",
                        IsInitialState = false,
                        Position = new Vector2(300, 100)
                    };

                    TransitionData idleToWalk = new()
                    {
                        FromStateId = idleId,
                        ToStateId = walkId,
                        Conditions = new List<ConditionData>
                        {
                            new()
                            {
                                Type = "Bool",
                                ParameterName = "IsMoving",
                                BoolValue = true
                            }
                        }
                    };

                    TransitionData walkToIdle = new()
                    {
                        FromStateId = walkId,
                        ToStateId = idleId,
                        Conditions = new List<ConditionData>
                        {
                            new()
                            {
                                Type = "Bool",
                                ParameterName = "IsMoving",
                                BoolValue = false
                            }
                        }
                    };

                    asset.states.Add(idle);
                    asset.states.Add(walk);
                    asset.transitions.Add(idleToWalk);
                    asset.transitions.Add(walkToIdle);
                    break;
            }

            return asset;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Called when this asset is assigned to a controller
        /// </summary>
        public void RegisterController(AnimationFlowController controller)
        {
            if (controller is not null && !_controllers.Contains(controller))
            {
                _controllers.Add(controller);
            }
        }

        /// <summary>
        ///     Called when this asset is unassigned from a controller
        /// </summary>
        public void UnregisterController(AnimationFlowController controller)
        {
            if (controller is not null)
            {
                _controllers.Remove(controller);
            }
        }

        /// <summary>
        ///     Get a controller that uses this asset (returns the first valid one)
        /// </summary>
        public AnimationFlowController GetController()
        {
            // Remove any null/destroyed controllers
            _controllers.RemoveAll(c =>
                !c || !c.gameObject || !c.gameObject.scene.IsValid()
            );

            // Return the first valid controller
            return _controllers.Count > 0 ? _controllers[0] : null;
        }

        /// <summary>
        ///     Get all controllers that use this asset
        /// </summary>
        public List<AnimationFlowController> GetControllers()
        {
            // Remove any null/destroyed controllers
            _controllers.RemoveAll(c =>
                !c || !c.gameObject || !c.gameObject.scene.IsValid()
            );

            // Return a copy of the list to prevent external modification
            return new List<AnimationFlowController>(_controllers);
        }

        private void OnValidate()
        {
            // Validate the asset whenever it changes in the editor
            Validate();
        }
#endif
    }


}
