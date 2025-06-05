using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Core;
using Animation.Flow.Editor.Managers;
using Animation.Flow.Editor.Panels.Conditions;
using Animation.Flow.Editor.Panels.Parameters;
using Animation.Flow.Interfaces;
using Animation.Flow.Parameters;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels
{
    /// <summary>
    ///     Main transition editor panel that contains parameter and condition panels
    /// </summary>
    public class TransitionEditorPanel : VisualElement
    {

        #region Constructor

        public TransitionEditorPanel(VisualElement parentContainer, IAnimator animator, AnimationFlowAsset flowAsset)
        {
            _parentContainer = parentContainer;
            _animator = animator;
            _flowAsset = flowAsset;

            // Load and apply stylesheets from Editor Resources
            StyleSheet styleSheet = Resources.Load<StyleSheet>("Stylesheets/TransitionEditorPanel");

            if (styleSheet)
            {
                styleSheets.Add(styleSheet);
            }

            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Create parameter panel with parameters from flow asset
            var parameters = GetParametersFromAsset();
            _parameterPanel = new ParameterPanel(_parentContainer, parameters);
            _parameterPanel.OnParameterDragStart += OnParameterDragStart;
            _parentContainer.Add(_parameterPanel);

            // Create condition panel (right side)
            _conditionPanel = new ConditionListPanel(_parentContainer);
            _conditionPanel.OnConditionsChanged += OnConditionsChanged;
            _conditionPanel.Hide();
            _parentContainer.Add(_conditionPanel);

            // Register with EdgeInspector
            EdgeInspector.Instance.SetEditorPanel(this);
        }

        #endregion

        #region Fields

        private readonly VisualElement _parentContainer;
        private readonly AnimationFlowAsset _flowAsset;
        private ParameterPanel _parameterPanel;
        private ConditionListPanel _conditionPanel;
        private AnimationFlowEdge _currentEdge;
        private readonly IAnimator _animator;
        private string _edgeId;

        #endregion

        #region Asset Data Retrieval

        private List<FlowParameter> GetParametersFromAsset()
        {
            if (_flowAsset == null)
            {
                Debug.LogWarning("No flow asset available for TransitionEditorPanel");
                return new List<FlowParameter>();
            }

            var assetParameters = _flowAsset.GetAllParameters();
            return new List<FlowParameter>(assetParameters);
        }

        private List<FlowState> GetStatesFromAsset()
        {
            if (_flowAsset == null)
            {
                Debug.LogWarning("No flow asset available for TransitionEditorPanel");
                return new List<FlowState>();
            }

            return new List<FlowState>(_flowAsset.states);
        }

        private AnimationContext GetContextFromAsset() => _flowAsset?.GetContext();

        #endregion

        #region Public Methods

        public void Show(AnimationFlowEdge edge)
        {
            if (edge == null)
            {
                Debug.LogWarning("Attempted to show TransitionEditorPanel with null edge");
                Hide();
                return;
            }

            _currentEdge = edge;
            _edgeId = EdgeConditionManager.GetEdgeId(edge);

            if (string.IsNullOrEmpty(_edgeId))
            {
                Debug.LogWarning("Invalid edge ID for transition editor");
                Hide();
                return;
            }

            // Get conditions from the manager
            var conditions = EdgeConditionManager.Instance.GetConditions(_edgeId);

            // Create deep copies of conditions to avoid reference issues
            var conditionCopies = new List<FlowCondition>();
            foreach (FlowCondition condition in conditions)
            {
                if (condition != null)
                {
                    conditionCopies.Add(condition.Clone());
                }
            }


            // Only show condition panel as parameter panel should already be visible
            _conditionPanel.Show(conditionCopies, GetTransitionTitle());
        }

        public void Hide()
        {
            _conditionPanel.Hide();
            // Parameter panel should always remain visible
        }


        // Called when the panel is about to be destroyed
        public void Cleanup()
        {
            if (_parameterPanel != null)
            {
                _parameterPanel.OnParameterDragStart -= OnParameterDragStart;
            }

            if (_conditionPanel != null)
            {
                _conditionPanel.OnConditionsChanged -= OnConditionsChanged;
            }

            _currentEdge = null;
            _edgeId = null;
        }

        public bool IsBeingInteracted() => _conditionPanel.IsBeingInteracted();

        #endregion

        #region Private Methods

        private string GetTransitionTitle()
        {
            if (_currentEdge?.output?.node is AnimationStateNode sourceNode &&
                _currentEdge.input?.node is AnimationStateNode targetNode)
            {
                return $"{sourceNode.AnimationName} → {targetNode.AnimationName}";
            }

            return "Transition";
        }

        private void OnParameterDragStart(FlowParameter parameter)
        {
            // Only show the drop indicator if conditions panel is visible
            if (_conditionPanel != null && _conditionPanel.style.display == DisplayStyle.Flex)
            {
                _conditionPanel.PrepareForParameterDrop(parameter);
            }
        }

        private void OnConditionsChanged(List<FlowCondition> conditions)
        {
            if (string.IsNullOrEmpty(_edgeId) || _currentEdge == null)
            {
                Debug.LogWarning("Cannot update conditions: Invalid edge or edge ID");
                return;
            }

            // Store conditions in the manager
            EdgeConditionManager.Instance.SetConditions(_edgeId, conditions);

            // Update the edge conditions
            _currentEdge.Conditions = conditions;

            // Update the flow asset if possible
            UpdateAssetTransitions(conditions);

            // Mark the editor window as dirty to ensure changes are saved
            EditorUtility.SetDirty(EditorWindow.GetWindow<AnimationFlowEditorWindow>());
        }

        private void UpdateAssetTransitions(List<FlowCondition> conditions)
        {
            if (_flowAsset == null || _currentEdge?.output?.node is not AnimationStateNode sourceNode ||
                _currentEdge.input?.node is not AnimationStateNode targetNode)
            {
                return;
            }

            // Find the source state in the asset
            FlowState sourceState = FindStateInAsset(sourceNode.ID);
            if (sourceState == null)
            {
                Debug.LogWarning($"Cannot find source state {sourceNode.ID} in flow asset");
                return;
            }

            // Update transitions in the asset would require changes to FlowState structure
            // For now, just log the update
            Debug.Log($"Updated transition conditions from {sourceNode.AnimationName} to {targetNode.AnimationName}");
        }

        private FlowState FindStateInAsset(string stateId)
        {
            if (_flowAsset == null) return null;

            foreach (FlowState state in _flowAsset.states)
            {
                if (state.Id == stateId)
                {
                    return state;
                }
            }

            return null;
        }

        #endregion

    }
}
