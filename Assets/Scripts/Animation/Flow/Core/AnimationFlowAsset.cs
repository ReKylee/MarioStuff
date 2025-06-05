using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using Animation.Flow.Parameters;
using Animation.Flow.States;
using UnityEngine;

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
        #region Serialized Fields

        [Tooltip("All states in this animation flow")]
        public List<AnimationStateData> states = new();

        [Tooltip("All transitions between states")] [SerializeReference]
        public List<FlowTransition> transitions = new();

        [Tooltip("All parameters used in this flow")]
        [SerializeField] private List<FlowParameter> parameters = new();

        [Tooltip("The context that manages parameters at runtime")]
        [SerializeField] private AnimationContext _context = new();

        #endregion

        #region Runtime Fields

        // Controller that uses this asset (not serialized, only for editor usage)
        [NonSerialized] private AnimationFlowController _controller;

        #endregion

        #region Validation

        /// <summary>
        ///     Validate this asset to ensure all references are correct
        /// </summary>
        private void Validate()
        {
            ValidateStates();
            ValidateTransitions();
            ValidateParameters();
        }

        /// <summary>
        ///     Validate states to ensure IDs are unique and one state is marked as initial
        /// </summary>
        private void ValidateStates()
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
        }

        /// <summary>
        ///     Validate transitions to ensure they reference valid states
        /// </summary>
        private void ValidateTransitions()
        {
            if (transitions == null) return;

            // Create a set of state IDs for faster lookup
            HashSet<string> stateIds = new();
            foreach (AnimationStateData state in states)
            {
                if (!string.IsNullOrEmpty(state.Id))
                {
                    stateIds.Add(state.Id);
                }
            }

            foreach (FlowTransition transition in transitions)
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

        /// <summary>
        ///     Validate parameters to ensure they are valid
        /// </summary>
        private void ValidateParameters()
        {
            // Ensure all parameters have valid names
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                if (parameters[i] == null || string.IsNullOrEmpty(parameters[i].Name) || !parameters[i].Validate())
                {
                    parameters.RemoveAt(i);
                }
            }

            // Remove duplicate parameter names, keeping the first one
            HashSet<string> paramNames = new();
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                if (paramNames.Contains(parameters[i].Name))
                {
                    parameters.RemoveAt(i);
                }
                else
                {
                    paramNames.Add(parameters[i].Name);
                }
            }

            // Update the context with parameter definitions
            SyncContextWithParameters();
        }

        /// <summary>
        ///     Synchronize the context with parameter definitions
        /// </summary>
        private void SyncContextWithParameters()
        {
            if (_context == null)
            {
                _context = new AnimationContext();
            }


            // Add all parameters from the parameter list to the context
            foreach (var param in parameters)
            {
                if (param != null && !string.IsNullOrEmpty(param.Name))
                {
                    _context.AddParameterDefinition(param.Clone());
                }
            }
        }

        #endregion

        #region Controller Building

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

            // Setup the animation context in the controller
            SetupControllerContext(controller);

            // Create dictionary to map state IDs to actual state instances
            var stateMap = new Dictionary<string, IAnimationState>();

            // Create and add states
            foreach (AnimationStateData stateData in states)
            {
                // Create state based on its type using the factory
                IAnimationState state = StateRegistry.CreateFromData(stateData);

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
            foreach (FlowTransition transitionData in transitions)
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
                            foreach (ConditionData conditionData in transitionData.Conditions)
                            {
                                if (conditionData != null)
                                {
                                    AddConditionToTransition(transition, conditionData);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Setup the controller's context with parameters from this asset
        /// </summary>
        private void SetupControllerContext(AnimationFlowController controller)
        {
            // Create a copy of the context for the controller
            var controllerContext = new AnimationContext();

            // Add all parameter definitions to the controller's context
            foreach (var param in parameters)
            {
                if (param != null && !string.IsNullOrEmpty(param.Name) && param.Validate())
                {
                    controllerContext.AddParameterDefinition(param.Clone());
                }
            }

            // Set the context in the controller
            controller.SetAnimationContext(controllerContext);
        }

        /// <summary>
        ///     Add a condition to a transition based on serialized data
        /// </summary>
        private static void AddConditionToTransition(AnimationTransition transition, ConditionData conditionData)
        {
            if (transition == null || conditionData == null)
                return;

            // Use the ConditionFactory to create a condition from the serialized data
            FlowCondition condition = ConditionFactory.CreateFromData(conditionData);
            if (condition != null)
            {
                transition.AddCondition(condition);
            }
        }

        #endregion

        #region Parameter Management

        /// <summary>
        ///     Add a parameter to this asset
        /// </summary>
        public void AddParameter(FlowParameter parameter)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.Name) || !parameter.Validate())
                return;

            // Remove any existing parameter with the same name
            RemoveParameter(parameter.Name);

            // Add the new parameter
            parameters.Add(parameter);

            // Update the context
            _context.AddParameterDefinition(parameter.Clone());

            // Register with the global registry
            ParameterRegistry.RegisterParameter(parameter.Clone());
        }

        /// <summary>
        ///     Remove a parameter from this asset
        /// </summary>
        public void RemoveParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return;

            // Remove from parameters list
            parameters.RemoveAll(p => p != null && p.Name == parameterName);

            // Remove from context
            _context.RemoveParameterDefinition(parameterName);
        }

        /// <summary>
        ///     Get a parameter by name
        /// </summary>
        public FlowParameter GetParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return null;

            return parameters.Find(p => p != null && p.Name == parameterName);
        }

        /// <summary>
        ///     Get all parameters
        /// </summary>
        public IReadOnlyList<FlowParameter> GetAllParameters() => parameters;

        /// <summary>
        ///     Get the animation context
        /// </summary>
        public AnimationContext GetContext() => _context;

        #endregion

        #region Editor Support

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
            if (controller is not null && _controller == controller)
            {
                _controller = null;
            }
        }

        /// <summary>
        ///     Get a controller that uses this asset
        /// </summary>
        public AnimationFlowController GetController() => _controller;

        private void OnValidate()
        {
            // Validate the asset whenever it changes in the editor
            Validate();
        }
#endif

        #endregion
    }
}
