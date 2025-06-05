using System;
using System.Collections.Generic;
using Animation.Flow.Parameters;
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

        #region Runtime Fields

        // Controller that uses this asset (not serialized, only for editor usage)
        [NonSerialized] private AnimationFlowController _controller;

        #endregion

        #region Serialized Fields

        [Tooltip("All states in this animation flow")] [SerializeReference]
        public List<FlowState> states = new();

        [Tooltip("All parameters used in this flow")]
        public List<FlowParameter> parameters = new();

        [Tooltip("The context that manages parameters at runtime")] [SerializeField]
        private AnimationContext context = new();

        #endregion

        #region Validation

        /// <summary>
        ///     Validate this asset to ensure all references are correct
        /// </summary>
        private void Validate()
        {
            ValidateStates();
            ValidateParameters();
        }

        /// <summary>
        ///     Validate states to ensure IDs are unique and one state is marked as initial
        /// </summary>
        private void ValidateStates()
        {
            foreach (FlowState state in states)
            {
                state.Validate();
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
                if (!parameters[i].Validate())
                {
                    parameters.RemoveAt(i);
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

        }

        /// <summary>
        ///     Setup the controller's context with parameters from this asset
        /// </summary>
        private void SetupControllerContext(AnimationFlowController controller)
        {
            // Set the context in the controller
            controller.SetAnimationContext(context);
        }

        #endregion

        #region Parameter Management

        /// <summary>
        ///     Get all parameters
        /// </summary>
        public IReadOnlyList<FlowParameter> GetAllParameters() => parameters;

        /// <summary>
        ///     Get the animation context
        /// </summary>
        public AnimationContext GetContext() => context;

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
