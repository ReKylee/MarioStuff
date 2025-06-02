using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.States;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    ///     Serializable asset that stores an animation flow configuration
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimationFlow", menuName = "Animation/Flow Asset")]
    public class AnimationFlowAsset : ScriptableObject
    {
        public List<AnimationStateData> States = new();
        public List<TransitionData> Transitions = new();

        /// <summary>
        ///     Create a runtime flow controller from this asset
        /// </summary>
        public void BuildFlowController(AnimationFlowController controller)
        {
            // Clear existing states
            controller.ClearStates();

            // Create dictionary to map state IDs to actual state instances
            var stateMap = new Dictionary<string, IAnimationState>();

            // Create and add states
            foreach (AnimationStateData stateData in States)
            {
                IAnimationState state = CreateStateFromData(stateData);
                controller.AddState(state);
                stateMap[stateData.Id] = state;

                // Set initial state if this is marked as such
                if (stateData.IsInitialState)
                {
                    controller.SetInitialState(stateData.Id);
                }
            }

            // Create transitions
            foreach (TransitionData transitionData in Transitions)
            {
                if (stateMap.TryGetValue(transitionData.FromStateId, out IAnimationState fromState) &&
                    stateMap.TryGetValue(transitionData.ToStateId, out IAnimationState toState))
                {
                    // Get the base state to add the transition
                    AnimationStateBase baseState = fromState as AnimationStateBase;
                    if (baseState != null)
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

        private IAnimationState CreateStateFromData(AnimationStateData stateData)
        {
            switch (stateData.StateType)
            {
                case "HoldFrame":
                    return new HoldFrameState(stateData.Id, stateData.AnimationName);

                case "OneTime":
                    return new OneTimeState(stateData.Id, stateData.AnimationName);

                case "Looping":
                    return new LoopingState(stateData.Id, stateData.AnimationName);

                default:
                    Debug.LogWarning($"Unknown state type: {stateData.StateType}, creating default OneTime state");
                    return new OneTimeState(stateData.Id, stateData.AnimationName);
            }
        }

        private void AddConditionToTransition(AnimationTransition transition, ConditionData conditionData)
        {
            ITransitionCondition condition = null;

            switch (conditionData.Type)
            {
                case "Bool":
                    condition = new BoolCondition(conditionData.ParameterName, conditionData.BoolValue);
                    break;

                case "FloatEquals":
                    condition = new FloatRangeCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue - 0.0001f,
                        conditionData.FloatValue + 0.0001f);

                    break;

                case "FloatLessThan":
                    condition = new FloatRangeCondition(
                        conditionData.ParameterName,
                        float.MinValue,
                        conditionData.FloatValue);

                    break;

                case "FloatGreaterThan":
                    condition = new FloatRangeCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue,
                        float.MaxValue);

                    break;

                case "AnimationComplete":
                    condition = new AnimationCompleteCondition();
                    break;

                case "TimeElapsed":
                    condition = new TimeElapsedCondition(
                        conditionData.ParameterName,
                        conditionData.FloatValue);

                    break;
            }

            if (condition != null)
            {
                transition.AddCondition(condition);
            }
        }
    }

    /// <summary>
    ///     Serializable data for an animation state
    /// </summary>
    [Serializable]
    public class AnimationStateData
    {
        public string Id;
        public string StateType;
        public string AnimationName;
        public bool IsInitialState;
        public Vector2 Position; // Used only in the editor
        public int FrameToHold; // Added for HoldFrameState
    }

    /// <summary>
    ///     Serializable data for a transition between states
    /// </summary>
    [Serializable]
    public class TransitionData
    {
        public string FromStateId;
        public string ToStateId;
        public List<ConditionData> Conditions = new();
    }

    /// <summary>
    ///     Serializable data for a transition condition
    /// </summary>
    [Serializable]
    public class ConditionData
    {
        public string Type;
        public string ParameterName;
        public bool BoolValue;
        public float FloatValue;
    }
}
