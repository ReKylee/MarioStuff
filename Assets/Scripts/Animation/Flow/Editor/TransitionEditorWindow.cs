using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Editor window to edit transition conditions for an animation flow edge
    /// </summary>
    public class TransitionEditorWindow : EditorWindow
    {
        private bool _boolValue = true;
        private Toggle _boolValueToggle;
        private string _compareType = "Equals";
        private EnumField _compareTypeField;
        private List<ConditionData> _conditions;

        // UI Elements
        private ScrollView _conditionsScrollView;
        private AnimationFlowEdge _currentEdge;
        private string _edgeId;
        private float _floatValue;
        private FloatField _floatValueField;
        private Toggle _isBoolean;

        // Data for new condition
        private string _parameterName = "";
        private TextField _parameterNameField;
        private AnimationStateNode _sourceNode;
        private AnimationStateNode _targetNode;

        private void OnEnable()
        {
            // Set up the UI
            CreateUI();
        }

        public static void ShowWindow(AnimationFlowEdge edge)
        {
            TransitionEditorWindow window = GetWindow<TransitionEditorWindow>("Edit Transition");
            window.minSize = new Vector2(400, 300);
            window.Initialize(edge);
            window.Show();
        }

        private void Initialize(AnimationFlowEdge edge)
        {
            _currentEdge = edge;
            _sourceNode = edge.output?.node as AnimationStateNode;
            _targetNode = edge.input?.node as AnimationStateNode;

            // Get the edge ID and conditions
            _edgeId = EdgeConditionManager.Instance.GetEdgeId(edge);
            if (!string.IsNullOrEmpty(_edgeId))
            {
                _conditions = EdgeConditionManager.Instance.GetConditions(_edgeId);
            }
            else
            {
                _conditions = new List<ConditionData>();
            }
        }

        private void CreateUI()
        {
            VisualElement root = rootVisualElement;

            // Add some padding
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            // Title section
            if (_sourceNode != null && _targetNode != null)
            {
                Label titleLabel = new($"Transition: {_sourceNode.AnimationName} → {_targetNode.AnimationName}");
                titleLabel.style.fontSize = 16;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.marginBottom = 10;
                root.Add(titleLabel);
            }

            // Existing conditions section
            Label conditionsLabel = new("Conditions");
            conditionsLabel.style.fontSize = 14;
            conditionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            conditionsLabel.style.marginTop = 10;
            root.Add(conditionsLabel);

            // Scroll view for conditions
            _conditionsScrollView = new ScrollView();
            _conditionsScrollView.style.height = 150;
            _conditionsScrollView.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            _conditionsScrollView.style.borderBottomWidth = 1;
            _conditionsScrollView.style.borderTopWidth = 1;
            _conditionsScrollView.style.borderLeftWidth = 1;
            _conditionsScrollView.style.borderRightWidth = 1;
            _conditionsScrollView.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            root.Add(_conditionsScrollView);

            RefreshConditionsList();

            // Add new condition section
            Label newConditionLabel = new("Add Condition");
            newConditionLabel.style.fontSize = 14;
            newConditionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            newConditionLabel.style.marginTop = 15;
            newConditionLabel.style.marginBottom = 5;
            root.Add(newConditionLabel);

            // Parameter name field
            _parameterNameField = new TextField("Parameter Name");
            _parameterNameField.RegisterValueChangedCallback(evt => _parameterName = evt.newValue);
            root.Add(_parameterNameField);

            // Is Boolean toggle
            _isBoolean = new Toggle("Is Boolean");
            _isBoolean.value = true;
            _isBoolean.RegisterValueChangedCallback(evt =>
            {
                _boolValueToggle.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                _compareTypeField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
                _floatValueField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            root.Add(_isBoolean);

            // Boolean value toggle (only shown when Is Boolean is true)
            _boolValueToggle = new Toggle("Value");
            _boolValueToggle.RegisterValueChangedCallback(evt => _boolValue = evt.newValue);
            root.Add(_boolValueToggle);

            // Compare type dropdown (only shown when Is Boolean is false)
            var compareOptions = new List<string> { "Equals", "Less Than", "Greater Than" };
            _compareTypeField = new EnumField("Comparison", ComparisonType.Equals);
            _compareTypeField.RegisterValueChangedCallback(evt =>
            {
                ComparisonType value = (ComparisonType)evt.newValue;
                switch (value)
                {
                    case ComparisonType.Equals:
                        _compareType = "Equals";
                        break;
                    case ComparisonType.LessThan:
                        _compareType = "Less Than";
                        break;
                    case ComparisonType.GreaterThan:
                        _compareType = "Greater Than";
                        break;
                }
            });

            _compareTypeField.style.display = DisplayStyle.None;
            root.Add(_compareTypeField);

            // Float value field (only shown when Is Boolean is false)
            _floatValueField = new FloatField("Value");
            _floatValueField.RegisterValueChangedCallback(evt => _floatValue = evt.newValue);
            _floatValueField.style.display = DisplayStyle.None;
            root.Add(_floatValueField);

            // Add Condition button
            Button addButton = new(AddCondition) { text = "Add Condition" };
            addButton.style.marginTop = 10;
            root.Add(addButton);

            // Special conditions section
            Label specialConditionsLabel = new("Special Conditions");
            specialConditionsLabel.style.fontSize = 14;
            specialConditionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            specialConditionsLabel.style.marginTop = 20;
            specialConditionsLabel.style.marginBottom = 5;
            root.Add(specialConditionsLabel);

            // Add Animation Complete button
            Button animCompleteButton = new(AddAnimationCompleteCondition)
                { text = "Add Animation Complete Condition" };

            root.Add(animCompleteButton);

            // Add Time Elapsed button
            Button timeElapsedButton = new(AddTimeElapsedCondition) { text = "Add Time Elapsed Condition (0.5s)" };
            root.Add(timeElapsedButton);
        }

        private void RefreshConditionsList()
        {
            _conditionsScrollView.Clear();

            if (_conditions == null || _conditions.Count == 0)
            {
                Label noConditionsLabel = new("No conditions. This transition will always occur.");
                noConditionsLabel.style.paddingBottom = 10;
                noConditionsLabel.style.paddingTop = 10;
                noConditionsLabel.style.paddingLeft = 10;
                noConditionsLabel.style.paddingRight = 10;
                noConditionsLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                _conditionsScrollView.Add(noConditionsLabel);
                return;
            }

            for (int i = 0; i < _conditions.Count; i++)
            {
                int index = i; // Capture index for lambda
                ConditionData condition = _conditions[i];

                VisualElement conditionContainer = new();
                conditionContainer.style.flexDirection = FlexDirection.Row;
                conditionContainer.style.justifyContent = Justify.SpaceBetween;
                conditionContainer.style.paddingLeft = 5;
                conditionContainer.style.paddingRight = 5;
                conditionContainer.style.paddingTop = 3;
                conditionContainer.style.paddingBottom = 3;

                // Alternating row colors
                if (i % 2 == 0)
                    conditionContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.1f);

                // Condition description
                string description = GetConditionDescription(condition);
                Label descLabel = new(description);
                descLabel.style.flexGrow = 1;
                conditionContainer.Add(descLabel);

                // Remove button
                Button removeButton = new(() => RemoveCondition(index)) { text = "Remove" };
                removeButton.style.width = 70;
                conditionContainer.Add(removeButton);

                _conditionsScrollView.Add(conditionContainer);
            }
        }

        private void AddCondition()
        {
            if (string.IsNullOrEmpty(_parameterName))
            {
                EditorUtility.DisplayDialog("Invalid Condition", "Parameter name cannot be empty.", "OK");
                return;
            }

            ConditionData condition = new()
            {
                ParameterName = _parameterName
            };

            if (_isBoolean.value)
            {
                condition.Type = "Bool";
                condition.BoolValue = _boolValue;
            }
            else
            {
                condition.Type = "Float" + _compareType.Replace(" ", "");
                condition.FloatValue = _floatValue;
            }

            _conditions.Add(condition);
            SaveConditions();
            RefreshConditionsList();

            // Clear parameter name field
            _parameterNameField.value = "";
        }

        private void AddAnimationCompleteCondition()
        {
            ConditionData condition = new()
            {
                Type = "AnimationComplete",
                ParameterName = ""
            };

            _conditions.Add(condition);
            SaveConditions();
            RefreshConditionsList();
        }

        private void AddTimeElapsedCondition()
        {
            ConditionData condition = new()
            {
                Type = "TimeElapsed",
                ParameterName = "StateTime",
                FloatValue = 0.5f
            };

            _conditions.Add(condition);
            SaveConditions();
            RefreshConditionsList();
        }

        private void RemoveCondition(int index)
        {
            if (index >= 0 && index < _conditions.Count)
            {
                _conditions.RemoveAt(index);
                SaveConditions();
                RefreshConditionsList();
            }
        }

        private void SaveConditions()
        {
            if (!string.IsNullOrEmpty(_edgeId))
            {
                EdgeConditionManager.Instance.SetConditions(_edgeId, _conditions);

                // Mark the asset dirty so changes are saved
                // Note: We don't directly mark the edge as dirty since it's not a UnityEngine.Object
                AnimationFlowEditorWindow editorWindow = GetWindow<AnimationFlowEditorWindow>();
                if (editorWindow != null)
                {
                    EditorUtility.SetDirty(editorWindow);
                }
            }
        }

        private string GetConditionDescription(ConditionData condition)
        {
            switch (condition.Type)
            {
                case "Bool":
                    return $"{condition.ParameterName} {(condition.BoolValue ? "is true" : "is false")}";
                case "FloatEquals":
                    return $"{condition.ParameterName} = {condition.FloatValue}";
                case "FloatLessThan":
                    return $"{condition.ParameterName} < {condition.FloatValue}";
                case "FloatGreaterThan":
                    return $"{condition.ParameterName} > {condition.FloatValue}";
                case "AnimationComplete":
                    return "Animation is complete";
                case "TimeElapsed":
                    return $"Time in state > {condition.FloatValue}s";
                default:
                    return $"Unknown condition: {condition.Type}";
            }
        }
    }

    public enum ComparisonType
    {
        Equals,
        LessThan,
        GreaterThan
    }
}
