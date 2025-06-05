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

        public TransitionEditorPanel(VisualElement parentContainer, IAnimator animator)
        {
            _parentContainer = parentContainer;
            _animator = animator;

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
            // Create parameter panel (left side)
            _parameterPanel = new ParameterPanel(_parentContainer);
            _parameterPanel.OnParameterDragStart += OnParameterDragStart;
            _parentContainer.Add(_parameterPanel);
            // Parameter panel should always be visible

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
        private ParameterPanel _parameterPanel;
        private ConditionListPanel _conditionPanel;
        private AnimationFlowEdge _currentEdge;
        private IAnimator _animator;
        private string _edgeId;

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
            var conditionCopies = new List<ConditionData>();
            foreach (var condition in conditions)
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

        private void OnConditionsChanged(List<ConditionData> conditions)
        {
            if (string.IsNullOrEmpty(_edgeId) || _currentEdge == null)
            {
                Debug.LogWarning("Cannot update conditions: Invalid edge or edge ID");
                return;
            }

            // Store a copy of the conditions in the manager
            EdgeConditionManager.Instance.SetConditions(_edgeId, conditions);

            // Verify nodes are valid animation state nodes
            if (_currentEdge.output?.node is not AnimationStateNode sourceNode || 
                _currentEdge.input?.node is not AnimationStateNode targetNode)
            {
                Debug.LogWarning("Cannot update conditions: Invalid animation state nodes");
                return;
            }

            // Check if we have a valid animator reference
            if (_animator == null)
            {
                Debug.LogWarning("Cannot update conditions: No animator reference available");
                // Still update the edge, but can't update the animation system
                _currentEdge.Conditions = conditions;
                return;
            }

            _currentEdge.Conditions = conditions;

            // Find the source and target states in the registry
            var source = StateRegistry.GetState(sourceNode.AnimationName);
            var target = StateRegistry.GetState(targetNode.AnimationName);

            if (source == null || target == null)
            {
                Debug.LogWarning($"Cannot update conditions: States not found in registry - {sourceNode.AnimationName} or {targetNode.AnimationName}");
                _currentEdge.Conditions = conditions;
                return;
            }

            // Get the transition between these states
            var transition = StateRegistry.GetTransition(source, target);
            if (transition == null)
            {
                Debug.LogWarning($"Cannot update conditions: No transition found from {sourceNode.AnimationName} to {targetNode.AnimationName}");
                _currentEdge.Conditions = conditions;
                return;
            }

            // Apply the conditions to the transition
            transition.Conditions.Clear();
            foreach (var condition in conditions)
            {
                if (condition != null)
                {
                    transition.AddCondition(condition.Clone());
                }
            }

            // Update the conditions on the edge
            _currentEdge.Conditions = conditions;

            // Notify that transition has been updated - method name depends on the actual API
            StateRegistry.NotifyTransitionModified(transition);

            // Mark the editor window as dirty to ensure changes are saved
            EditorUtility.SetDirty(EditorWindow.GetWindow<AnimationFlowEditorWindow>());
            {
                Debug.LogWarning($"Cannot update conditions: Transition not found from {sourceNode.AnimationName} to {targetNode.AnimationName}");
                return;
            }

            // Clear existing conditions and add the new ones
            transition.Conditions.Clear();

            // Add each condition as a clone to avoid reference issues
            foreach (var condition in conditions)
            {
                if (condition != null)
                {
                    transition.AddCondition(condition.Clone());
                }
            }

            // Also update the conditions directly on the edge
            _currentEdge.Conditions = conditions;

            // Notify the system that the transition has been modified
            activeContext.OnTransitionModified(transition);

            // Mark the editor window as dirty to ensure changes are saved
            EditorUtility.SetDirty(EditorWindow.GetWindow<AnimationFlowEditorWindow>());

                // Apply conditions to the transition in the active context
                activeContext.SetTransitionConditions(
                    sourceNode.AnimationName,
                    targetNode.AnimationName,
                    conditionCopies
                );
            }

            // Also update the conditions directly on the edge
            _currentEdge.Conditions = conditions;

            // Mark the editor window as dirty to ensure changes are saved
            EditorUtility.SetDirty(EditorWindow.GetWindow<AnimationFlowEditorWindow>());
        }

        #endregion

    }
}
