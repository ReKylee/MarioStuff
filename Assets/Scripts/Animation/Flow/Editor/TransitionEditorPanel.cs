using System.Collections.Generic;
using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Editor panel to edit transition conditions for an animation flow edge
    /// </summary>
    public class TransitionEditorPanel
    {
        private readonly Vector2 _minSize = new(250, 300);
        private readonly VisualElement _parentContainer;

        // Container for the panel
        private readonly VisualElement _root;
        private bool _boolValue = true;
        private Toggle _boolValueToggle;
        private string _compareType = "Equals";
        private EnumField _compareTypeField;
        private List<ConditionData> _conditions;

        // UI Elements
        private ScrollView _conditionsScrollView;
        private AnimationFlowEdge _currentEdge;

        // For dragging
        private Vector2 _dragStartPosition;
        private string _edgeId;
        private float _floatValue;
        private FloatField _floatValueField;
        private Toggle _isBoolean;
        private bool _isDragging;

        // Track whether the panel is being interacted with
        private bool _isInteracting;

        // For resizing
        private bool _isResizing;

        // Data for new condition
        private string _parameterName = "";
        private TextField _parameterNameField;

        // Dimensions and position
        private Vector2 _position;
        private Vector2 _resizeStartPanelPosition;
        private Vector2 _resizeStartPosition;
        private Vector2 _resizeStartSize;
        private Vector2 _size = new(300, 400);
        private AnimationStateNode _sourceNode;
        private AnimationStateNode _targetNode;

        public TransitionEditorPanel(VisualElement container)
        {
            _parentContainer = container;

            // Position in top right corner with padding, accounting for toolbar height
            // Estimate toolbar height (standard Unity editor toolbar is typically around 20-24px)
            const float toolbarHeight = 24;
            _position = new Vector2(container.worldBound.width - _size.x - 20, 20 + toolbarHeight);

            // Create root element that will contain the panel
            _root = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    width = _size.x,
                    height = _size.y,
                    left = _position.x,
                    top = _position.y,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f),
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                    borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                    borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                    borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                    borderBottomRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderTopLeftRadius = 3
                }
            };

            // Add a title bar at the top
            VisualElement titleBar = new()
            {
                style =
                {
                    height = 24,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    paddingLeft = 8,
                    paddingRight = 4,
                    borderTopRightRadius = 3,
                    borderTopLeftRadius = 3
                }
            };

            // Make title bar draggable
            titleBar.RegisterCallback<MouseDownEvent>(OnTitlebarMouseDown);

            // Add title text
            Label titleText = new("Transition Editor")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white
                }
            };

            titleBar.Add(titleText);

            // Add close button
            Button closeButton = new(() => Hide())
            {
                text = "×",
                style =
                {
                    width = 20,
                    height = 20,
                    fontSize = 16,
                    backgroundColor = new Color(0, 0, 0, 0),
                    color = Color.white,
                    borderBottomWidth = 0,
                    borderTopWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0
                }
            };

            titleBar.Add(closeButton);

            _root.Add(titleBar);

            // Add content container with padding
            VisualElement content = new()
            {
                style =
                {
                    paddingBottom = 10,
                    paddingTop = 10,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            _root.Add(content);

            // Add resize handle in bottom left corner 
            VisualElement resizeHandle = new()
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    bottom = 0,
                    width = 16,
                    height = 16,
                    cursor = new StyleCursor(StyleKeyword.Auto),
                    backgroundImage = EditorGUIUtility.IconContent("d_WindowBottomResize").image as Texture2D
                }
            };

            resizeHandle.RegisterCallback<MouseDownEvent>(OnResizeHandleMouseDown);

            _root.Add(resizeHandle);

            // Register for mouse move and up events for dragging and resizing
            container.RegisterCallback<MouseMoveEvent>(OnMouseMove);

            container.RegisterCallback<MouseUpEvent>(_ =>
            {
                _isDragging = false;
                _isResizing = false;
            });

            // Add to container but hide initially
            container.Add(_root);
            _root.style.display = DisplayStyle.None;

            // Register with the EdgeInspector
            EdgeInspector.Instance.SetEditorPanel(this);

            // Build UI within the content area
            CreateUI(content);
        }
        public bool IsVisible { get; private set; }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            OnDrag(evt);
            OnResize(evt);
            evt.StopPropagation();
        }
        private void OnResize(MouseMoveEvent evt)
        {
            if (!_isResizing)
                return;

            // Calculate the mouse movement delta from the start of the drag
            Vector2 delta = evt.mousePosition - _resizeStartPosition;

            // --- Calculate target dimensions and position for bottom-left resize ---
            // For a bottom-left resize:
            // - The width changes by -delta.x (dragging right decreases width, dragging left increases it)
            // - The height changes by +delta.y (dragging down increases height, dragging up decreases it)
            // - The top-right corner of the panel should remain fixed relative to its position at the start of the drag.

            float targetWidth = _resizeStartSize.x - delta.x;
            float targetHeight = _resizeStartSize.y + delta.y;

            // Apply minimum size constraints
            float newWidth = Mathf.Max(targetWidth, _minSize.x);
            float newHeight = Mathf.Max(targetHeight, _minSize.y);

            // Determine the fixed top-right X coordinate from the start of the drag
            float fixedTopRightX = _resizeStartSize.x + _resizeStartPanelPosition.x;

            // Calculate the new X position for the panel's left edge, maintaining the fixed top-right corner
            float newX = fixedTopRightX - newWidth;

            // Ensure panel stays within bounds and adjust dimensions
            if (newX < 0)
            {
                newX = 0;
                newWidth = Mathf.Clamp(fixedTopRightX, _minSize.x, _parentContainer.worldBound.width);
            }

            // Ensure panel height stays within container bounds
            newHeight = Mathf.Clamp(newHeight, _minSize.y,
                _parentContainer.worldBound.height - _position.y);

            _position.x = newX;

            _root.style.left = _position.x;
            _root.style.top = _position.y;

            _size = new Vector2(newWidth, newHeight);
            _root.style.width = _size.x;
            _root.style.height = _size.y;
        }

        private void OnDrag(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            // Calculate new position
            Vector2 newPosition = evt.mousePosition - _dragStartPosition;

            // Clamp position to ensure panel stays completely within container bounds
            // Prevent moving too far right or bottom
            newPosition.x = Mathf.Clamp(newPosition.x, 0, _parentContainer.worldBound.width - _size.x);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, _parentContainer.worldBound.height - _size.y);

            // Also prevent moving too far left or top (should always be >= 0)
            newPosition.x = Mathf.Max(0, newPosition.x);
            newPosition.y = Mathf.Max(0, newPosition.y);

            _position = newPosition;
            _root.style.left = _position.x;
            _root.style.top = _position.y;
        }
        private void OnTitlebarMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                _isDragging = true;
                _dragStartPosition = evt.mousePosition - _position;
                evt.StopPropagation();
            }
        }
        private void OnResizeHandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                _isResizing = true;
                _resizeStartSize = _size;
                _resizeStartPosition = evt.mousePosition;
                _resizeStartPanelPosition = _position;
                evt.StopPropagation();
            }
        }

        public void Show(AnimationFlowEdge edge)
        {
            Initialize(edge);

            // If we need to reposition to the top right (e.g., when showing the first time)
            if (!IsVisible)
            {
                // Update position to be in top right corner of the current container
                _position = new Vector2(_parentContainer.worldBound.width - _size.x - 20, 20);
                _root.style.left = _position.x;
                _root.style.top = _position.y;
            }

            // Register panel-wide mouse events
            RegisterPanelEvents();

            _root.style.display = DisplayStyle.Flex;
            IsVisible = true;
        }

        public void Hide()
        {
            // Unregister panel events when hiding
            UnregisterPanelEvents();

            _root.style.display = DisplayStyle.None;
            IsVisible = false;
            _isInteracting = false;
        }

        private void RegisterPanelEvents()
        {
            // Register panel-wide events to track interaction
            _root.RegisterCallback<MouseDownEvent>(OnPanelMouseDown);
            _root.RegisterCallback<MouseUpEvent>(OnPanelMouseUp);
            _root.RegisterCallback<MouseLeaveEvent>(OnPanelMouseLeave);
        }

        private void UnregisterPanelEvents()
        {
            // Unregister panel events
            _root.UnregisterCallback<MouseDownEvent>(OnPanelMouseDown);
            _root.UnregisterCallback<MouseUpEvent>(OnPanelMouseUp);
            _root.UnregisterCallback<MouseLeaveEvent>(OnPanelMouseLeave);
        }

        private void OnPanelMouseDown(MouseDownEvent evt)
        {
            _isInteracting = true;
            evt.StopPropagation();
        }

        private void OnPanelMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        private void OnPanelMouseLeave(MouseLeaveEvent evt)
        {
            // Only clear interaction state if not dragging or resizing
            if (!_isDragging && !_isResizing)
            {
                _isInteracting = false;
            }
        }

        // Check if panel is currently being interacted with
        public bool IsBeingInteracted() => IsVisible && (_isInteracting || _isDragging || _isResizing);

        public void Toggle(AnimationFlowEdge edge)
        {
            if (IsVisible && _currentEdge == edge)
            {
                Hide();
            }
            else
            {
                Show(edge);
            }
        }

        private void Initialize(AnimationFlowEdge edge)
        {
            _currentEdge = edge;
            _sourceNode = edge.output?.node as AnimationStateNode;
            _targetNode = edge.input?.node as AnimationStateNode;

            // Get the edge ID and conditions
            _edgeId = EdgeConditionManager.GetEdgeId(edge);
            if (!string.IsNullOrEmpty(_edgeId))
            {
                _conditions = EdgeConditionManager.Instance.GetConditions(_edgeId);
            }
            else
            {
                _conditions = new List<ConditionData>();
            }

            // Update title to show the transition
            if (_sourceNode != null && _targetNode != null)
            {
                VisualElement titleBar = _root.ElementAt(0);
                Label titleText = titleBar.ElementAt(0) as Label;
                if (titleText != null)
                {
                    titleText.text = $"{_sourceNode.AnimationName} → {_targetNode.AnimationName}";
                }
            }

            // Refresh the conditions list
            RefreshConditionsList();
        }

        private void CreateUI(VisualElement content)
        {
            // Existing conditions section
            Label conditionsLabel = new("Conditions");
            conditionsLabel.style.fontSize = 14;
            conditionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            conditionsLabel.style.marginTop = 5;
            content.Add(conditionsLabel);

            // Scroll view for conditions
            _conditionsScrollView = new ScrollView();
            _conditionsScrollView.style.height = 120;
            _conditionsScrollView.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            _conditionsScrollView.style.borderBottomWidth = 1;
            _conditionsScrollView.style.borderTopWidth = 1;
            _conditionsScrollView.style.borderLeftWidth = 1;
            _conditionsScrollView.style.borderRightWidth = 1;
            _conditionsScrollView.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            _conditionsScrollView.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            content.Add(_conditionsScrollView);

            // Add new condition section
            Label newConditionLabel = new("Add Condition");
            newConditionLabel.style.fontSize = 14;
            newConditionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            newConditionLabel.style.marginTop = 10;
            newConditionLabel.style.marginBottom = 5;
            content.Add(newConditionLabel);

            // Parameter name field
            _parameterNameField = new TextField("Parameter Name");
            _parameterNameField.RegisterValueChangedCallback(evt => _parameterName = evt.newValue);
            content.Add(_parameterNameField);

            // Is Boolean toggle
            _isBoolean = new Toggle("Is Boolean");
            _isBoolean.value = true;
            _isBoolean.RegisterValueChangedCallback(evt =>
            {
                _boolValueToggle.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                _compareTypeField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
                _floatValueField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            content.Add(_isBoolean);

            // Boolean value toggle (only shown when Is Boolean is true)
            _boolValueToggle = new Toggle("Value");
            _boolValueToggle.RegisterValueChangedCallback(evt => _boolValue = evt.newValue);
            content.Add(_boolValueToggle);

            // Compare type dropdown (only shown when Is Boolean is false)
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
            content.Add(_compareTypeField);

            // Float value field (only shown when Is Boolean is false)
            _floatValueField = new FloatField("Value");
            _floatValueField.RegisterValueChangedCallback(evt => _floatValue = evt.newValue);
            _floatValueField.style.display = DisplayStyle.None;
            content.Add(_floatValueField);

            // Add Condition button
            Button addButton = new(AddCondition) { text = "Add Condition" };
            addButton.style.marginTop = 8;
            content.Add(addButton);

            // Special conditions section
            Label specialConditionsLabel = new("Special Conditions");
            specialConditionsLabel.style.fontSize = 14;
            specialConditionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            specialConditionsLabel.style.marginTop = 12;
            specialConditionsLabel.style.marginBottom = 5;
            content.Add(specialConditionsLabel);

            // Add Animation Complete button
            Button animCompleteButton = new(AddAnimationCompleteCondition)
                { text = "Add Animation Complete" };

            content.Add(animCompleteButton);

            // Add Time Elapsed button
            Button timeElapsedButton = new(AddTimeElapsedCondition) { text = "Add Time Elapsed (0.5s)" };
            content.Add(timeElapsedButton);
        }

        private void RefreshConditionsList()
        {
            if (_conditionsScrollView == null) return;

            _conditionsScrollView.Clear();

            if (_conditions == null || _conditions.Count == 0)
            {
                Label noConditionsLabel = new("No conditions. This transition will always occur.");
                noConditionsLabel.style.paddingBottom = 8;
                noConditionsLabel.style.paddingTop = 8;
                noConditionsLabel.style.paddingLeft = 8;
                noConditionsLabel.style.paddingRight = 8;
                noConditionsLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
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
                    conditionContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.3f);


                // Remove button
                Button removeButton = new(() => RemoveCondition(index)) { text = "×" };
                removeButton.style.width = 20;
                removeButton.style.height = 20;
                removeButton.style.backgroundColor = new Color(0.5f, 0.1f, 0.1f, 0.6f);
                removeButton.style.color = Color.white;
                removeButton.style.borderBottomRightRadius = 3;
                removeButton.style.borderBottomLeftRadius = 3;
                removeButton.style.borderTopRightRadius = 3;
                removeButton.style.borderTopLeftRadius = 3;
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
                DataType = ConditionDataType.Animation,
                ComparisonType = ComparisonType.Completed,
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
                DataType = ConditionDataType.Time,
                ComparisonType = ComparisonType.Elapsed,
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
                AnimationFlowEditorWindow editorWindow = EditorWindow.GetWindow<AnimationFlowEditorWindow>();
                if (editorWindow)
                {
                    EditorUtility.SetDirty(editorWindow);
                }
            }
        }
    }


}
