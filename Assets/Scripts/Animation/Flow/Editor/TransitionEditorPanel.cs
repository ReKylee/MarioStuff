using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Main transition editor panel that contains parameter and condition panels
    /// </summary>
    public class TransitionEditorPanel : VisualElement
    {

        #region Constructor

        public TransitionEditorPanel(VisualElement parentContainer)
        {
            _parentContainer = parentContainer;
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Load stylesheet
            StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/Animation/Flow/Editor/TransitionEditorPanel.uss");

            if (stylesheet)
                styleSheets.Add(stylesheet);

            // Create parameter panel (left side)
            _parameterPanel = new ParameterPanel(_parentContainer);
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
        private ParameterPanel _parameterPanel;
        private ConditionListPanel _conditionPanel;
        private AnimationFlowEdge _currentEdge;
        private string _edgeId;

        #endregion

        #region Public Methods

        public void Show(AnimationFlowEdge edge)
        {
            _currentEdge = edge;
            _edgeId = EdgeConditionManager.GetEdgeId(edge);

            var conditions = !string.IsNullOrEmpty(_edgeId)
                ? EdgeConditionManager.Instance.GetConditions(_edgeId)
                : new List<ConditionData>();

            _conditionPanel.Show(conditions, GetTransitionTitle());
        }

        public void Hide()
        {
            _conditionPanel.Hide();
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

        private void OnParameterDragStart(ParameterData parameter)
        {
            // Only show the drop indicator if conditions panel is visible
            if (_conditionPanel != null && _conditionPanel.style.display == DisplayStyle.Flex)
            {
                _conditionPanel.PrepareForParameterDrop(parameter);
            }
        }

        private void OnConditionsChanged(List<ConditionData> conditions)
        {
            if (!string.IsNullOrEmpty(_edgeId))
            {
                EdgeConditionManager.Instance.SetConditions(_edgeId, conditions);
                EditorUtility.SetDirty(EditorWindow.GetWindow<AnimationFlowEditorWindow>());
            }
        }

        #endregion

    }
}
