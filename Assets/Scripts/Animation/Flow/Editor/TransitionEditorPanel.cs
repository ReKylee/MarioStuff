using System;
using System.Collections.Generic;
using System.Linq;
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

        // Condition editor support
        private readonly Dictionary<string, VisualElement> _conditionElements = new();
        private readonly Vector2 _minSize = new(250, 300);
        private readonly VisualElement _parentContainer;

        // Container for the panel
        private readonly VisualElement _root;
        private List<ConditionData> _conditions;
        private VisualElement _conditionsContainer;

        // UI Elements
        private ScrollView _conditionsScrollView;
        private VisualElement _contentContainer;
        private AnimationFlowEdge _currentEdge;
        private ConditionData _draggedCondition;

        // Drag and drop support
        private VisualElement _draggedElement;
        private int _draggedStartIndex;

        // For dragging
        private Vector2 _dragStartPosition;
        private VisualElement _dropIndicator;
        private string _edgeId;
        private float _floatValue;
        private FloatField _floatValueField;
        private Toggle _isBoolean;
        private bool _isDragging;
        private bool _isDraggingCondition;

        // Track whether the panel is being interacted with
        private bool _isInteracting;

        // For resizing
        private bool _isResizing;
        private string _newCompositeType = "And";
        private ConditionDataType _newConditionType = ConditionDataType.Boolean;
        private EnumField _newConditionTypeField;

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
                    top = _position.y
                }
            };

            _root.AddToClassList("transition-editor-panel");

            // Add a title bar at the top
            VisualElement titleBar = new();
            titleBar.AddToClassList("panel-title-bar");

            // Make title bar draggable
            titleBar.RegisterCallback<MouseDownEvent>(OnTitlebarMouseDown);

            // Add title text
            Label titleText = new("Transition Editor");
            titleText.AddToClassList("panel-title-text");
            titleBar.Add(titleText);

            // Add close button
            Button closeButton = new(() => Hide())
            {
                text = "×"
            };

            closeButton.AddToClassList("panel-close-button");

            titleBar.Add(closeButton);

            _root.Add(titleBar);

            // Add content container
            VisualElement content = new();
            content.AddToClassList("panel-content");
            _root.Add(content);

            // Add resize handle in bottom left corner 
            VisualElement resizeHandle = new();
            resizeHandle.AddToClassList("panel-resize-handle");
            resizeHandle.style.backgroundImage =
                EditorGUIUtility.IconContent("d_WindowBottomResize").image as Texture2D;

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
            // Handle drag-and-drop of conditions
            if (_isDraggingCondition && _draggedElement != null)
            {
                UpdateDropTarget(evt.mousePosition);
                evt.StopPropagation();
                return;
            }

            // Handle panel dragging and resizing
            OnDrag(evt);
            OnResize(evt);
            evt.StopPropagation();
        }

        private void UpdateDropTarget(Vector2 mousePosition)
        {
            // Find what element is under the mouse
            bool foundTarget = false;

            foreach (VisualElement element in _conditionElements.Values)
            {
                if (element == _draggedElement) continue;

                if (element.worldBound.Contains(mousePosition))
                {
                    foundTarget = true;
                    ConditionData targetCondition = element.userData as ConditionData;

                    // Show drop indicator
                    _dropIndicator.style.display = DisplayStyle.Flex;

                    // Determine drop position relative to the target element
                    float relativeY = mousePosition.y - element.worldBound.y;
                    float elementHeight = element.worldBound.height;

                    if (targetCondition.DataType == ConditionDataType.Composite &&
                        relativeY > elementHeight * 0.3f && relativeY < elementHeight * 0.7f)
                    {
                        // Dropping inside composite - highlight the entire composite
                        element.style.backgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.3f);
                    }
                    else
                    {
                        // Position indicator for dropping above or below
                        _conditionsContainer.Remove(_dropIndicator);

                        if (relativeY <= elementHeight * 0.5f)
                        {
                            // Position above element
                            int index = _conditionsContainer.IndexOf(element);
                            _conditionsContainer.Insert(index, _dropIndicator);
                        }
                        else
                        {
                            // Position below element
                            int index = _conditionsContainer.IndexOf(element) + 1;
                            if (index < _conditionsContainer.childCount)
                                _conditionsContainer.Insert(index, _dropIndicator);
                            else
                                _conditionsContainer.Add(_dropIndicator);
                        }
                    }

                    break;
                }
            }

            if (!foundTarget)
            {
                _dropIndicator.style.display = DisplayStyle.None;

                // Reset any highlighted elements
                foreach (VisualElement element in _conditionElements.Values)
                {
                    if (element.userData is ConditionData condition &&
                        condition.DataType == ConditionDataType.Composite)
                    {
                        element.style.backgroundColor = new Color(0.2f, 0.3f, 0.4f, 0.4f);
                    }
                }
            }
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

            // Adjust scroll view height to accommodate the conditions list
            if (_conditionsScrollView != null)
            {
                // Calculate available height for scroll view by subtracting other elements' heights
                float availableHeight = _size.y - 200; // Rough estimate for other UI elements
                _conditionsScrollView.style.height = Mathf.Max(100, availableHeight);
            }
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

            // Initialize condition positions if needed
            InitializeConditionPositions();

            // Clear cached condition elements before refreshing
            _conditionElements.Clear();

            // Refresh the conditions list
            RefreshConditionsList();
        }

        private void InitializeConditionPositions()
        {
            if (_conditions == null || _conditions.Count == 0) return;

            // For any conditions without group indices set, assign sequential indices based on list order
            int rootIndex = 0;
            var compositeIndices = new Dictionary<string, int>();

            foreach (ConditionData condition in _conditions)
            {
                // Ensure UniqueId is set
                if (string.IsNullOrEmpty(condition.UniqueId))
                {
                    condition.UniqueId = Guid.NewGuid().ToString();
                }

                // If no parent group (root level condition)
                if (string.IsNullOrEmpty(condition.ParentGroupId))
                {
                    condition.GroupIndex = rootIndex++;
                    condition.NestingLevel = 0;
                }
                else
                {
                    // Child of a composite
                    if (!compositeIndices.ContainsKey(condition.ParentGroupId))
                    {
                        compositeIndices[condition.ParentGroupId] = 0;
                    }

                    condition.GroupIndex = compositeIndices[condition.ParentGroupId]++;

                    // Find parent's nesting level
                    ConditionData parent = _conditions.FirstOrDefault(c => c.UniqueId == condition.ParentGroupId);
                    if (parent != null)
                    {
                        condition.NestingLevel = parent.NestingLevel + 1;
                    }
                }
            }
        }

        private void CreateUI(VisualElement content)
        {
            // Store content container for later use with auto-sizing
            _contentContainer = content;

            // Conditions Section
            Label conditionsLabel = new("Conditions");
            conditionsLabel.AddToClassList("transition-editor-panel-header");
            content.Add(conditionsLabel);

            // Create a container for the conditions list
            VisualElement conditionsListContainer = new();
            conditionsListContainer.AddToClassList("droppable-area");
            conditionsListContainer.style.minHeight = 100; // Minimum height
            content.Add(conditionsListContainer);

            // Scroll view for conditions
            _conditionsScrollView = new ScrollView();
            _conditionsScrollView.AddToClassList("flex-container");
            conditionsListContainer.Add(_conditionsScrollView);

            // Create container for draggable conditions
            _conditionsContainer = new VisualElement();
            _conditionsContainer.AddToClassList("flex-container");
            _conditionsScrollView.Add(_conditionsContainer);

            // Create drop indicator element (hidden by default)
            _dropIndicator = new VisualElement();
            _dropIndicator.AddToClassList("drop-indicator");
            _dropIndicator.style.display = DisplayStyle.None;
            _conditionsContainer.Add(_dropIndicator);

            // Add New Condition Section
            VisualElement addConditionSection = new(); // Create the missing container
            addConditionSection.AddToClassList("add-condition-section");
            content.Add(addConditionSection);

            // Add New Condition Section
            Label newConditionLabel = new("Add New Condition");
            newConditionLabel.style.fontSize = 14;
            newConditionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            newConditionLabel.style.marginTop = 15;
            newConditionLabel.style.marginBottom = 5;
            content.Add(newConditionLabel);

            // Condition Type Selector
            _newConditionTypeField = new EnumField("Type", ConditionDataType.Boolean);
            _newConditionTypeField.RegisterValueChangedCallback(evt =>
            {
                _newConditionType = (ConditionDataType)evt.newValue;
                RefreshNewConditionFields();
            });

            content.Add(_newConditionTypeField);

            // Container for type-specific fields that will change based on selection
            VisualElement typeSpecificContainer = new();
            typeSpecificContainer.name = "typeSpecificContainer";
            typeSpecificContainer.AddToClassList("condition-content");
            addConditionSection.Add(typeSpecificContainer);

            // Create Add Condition Button
            Button addButton = new(AddNewCondition) { text = "Add Condition" };
            addButton.AddToClassList("top-margin");
            addConditionSection.Add(addButton);

            // Add Composite Condition Section
            VisualElement compositeSection = new();
            compositeSection.AddToClassList("add-condition-section");
            content.Add(compositeSection);

            Label compositeLabel = new("Add Composite Condition");
            compositeLabel.AddToClassList("transition-editor-panel-header");
            compositeSection.Add(compositeLabel);

            // Composite Type Selection
            VisualElement compositeTypeContainer = new();
            compositeTypeContainer.AddToClassList("quick-add-container");

            // AND Button
            Button andButton = new(() => AddCompositeCondition("And")) { text = "AND Group" };
            andButton.AddToClassList("quick-add-button");
            andButton.AddToClassList("group-button");
            compositeTypeContainer.Add(andButton);

            // OR Button
            Button orButton = new(() => AddCompositeCondition("Or")) { text = "OR Group" };
            orButton.AddToClassList("quick-add-button");
            orButton.AddToClassList("group-button");
            compositeTypeContainer.Add(orButton);

            compositeSection.Add(compositeTypeContainer);

            // Special Conditions Section
            VisualElement specialSection = new();
            specialSection.AddToClassList("add-condition-section");
            content.Add(specialSection);

            Label specialConditionsLabel = new("Special Conditions");
            specialConditionsLabel.AddToClassList("transition-editor-panel-header");
            specialSection.Add(specialConditionsLabel);

            // Special Conditions Container
            VisualElement specialButtonsContainer = new();
            specialButtonsContainer.AddToClassList("quick-add-container");

            // Add Animation Complete Button
            Button animCompleteButton = new(AddAnimationCompleteCondition) { text = "Animation Complete" };
            animCompleteButton.AddToClassList("quick-add-button");
            specialButtonsContainer.Add(animCompleteButton);

            // Add Time Elapsed Button
            Button timeElapsedButton = new(AddTimeElapsedCondition) { text = "Time Elapsed (0.5s)" };
            timeElapsedButton.AddToClassList("quick-add-button");
            specialButtonsContainer.Add(timeElapsedButton);

            specialSection.Add(specialButtonsContainer);

            // Initialize the new condition fields
            RefreshNewConditionFields();
        }

        private void RefreshConditionsList()
        {
            if (_conditionsContainer == null) return;

            // Clear the conditions dictionary and container
            _conditionElements.Clear();
            _conditionsContainer.Clear();
            _conditionsContainer.Add(_dropIndicator); // Re-add the drop indicator (still hidden)

            // Adjust panel height based on number of conditions (with min and max limits)
            int conditionsCount = _conditions?.Count ?? 0;
            float calculatedHeight = Math.Max(_minSize.y, 300 + conditionsCount * 40);
            _size.y = Math.Min(calculatedHeight, _parentContainer.worldBound.height * 0.8f);
            _root.style.height = _size.y;

            if (_conditions == null || _conditions.Count == 0)
            {
                Label noConditionsLabel = new("No conditions. This transition will always occur.");
                noConditionsLabel.AddToClassList("empty-state-label");
                _conditionsContainer.Add(noConditionsLabel);
                return;
            }

            // Sort conditions by nesting level and group index
            var sortedConditions = _conditions
                .OrderBy(c => c.NestingLevel)
                .ThenBy(c => c.GroupIndex)
                .ToList();

            // Create UI for each condition
            for (int i = 0; i < sortedConditions.Count; i++)
            {
                ConditionData condition = sortedConditions[i];
                CreateConditionElement(condition, i);
            }
        }

        private VisualElement CreateConditionElement(ConditionData condition, int index)
        {
            // Create the condition container
            VisualElement conditionElement = new();
            string conditionId = condition.UniqueId;
            conditionElement.userData = condition; // Store the condition data in the element

            // Set styles based on condition type and nesting
            SetupConditionStyles(conditionElement, condition, index);

            // Make element draggable
            MakeElementDraggable(conditionElement);

            // Add to dictionary for easy reference
            _conditionElements[conditionId] = conditionElement;

            // Add to container
            _conditionsContainer.Add(conditionElement);

            // Create the content based on condition type
            if (condition.DataType == ConditionDataType.Composite)
            {
                CreateCompositeConditionContent(conditionElement, condition);
            }
            else
            {
                CreateStandardConditionContent(conditionElement, condition);
            }

            return conditionElement;
        }

        private void SetupConditionStyles(VisualElement element, ConditionData condition, int index)
        {
            // Basic styles for all condition elements
            element.style.flexDirection = FlexDirection.Row;
            element.style.paddingLeft = 5 + condition.NestingLevel * 15; // Indent based on nesting level
            element.style.paddingRight = 5;
            element.style.paddingTop = 3;
            element.style.paddingBottom = 3;
            element.style.marginTop = 2;
            element.style.marginBottom = 2;

            // Visual indicator for drop target
            element.style.borderLeftWidth = 2;
            element.style.borderLeftColor = condition.DataType == ConditionDataType.Composite
                ? new Color(0.4f, 0.7f, 0.9f, 0.8f)
                : new Color(0.4f, 0.4f, 0.4f, 0.4f);

            // Alternating row colors
            if (index % 2 == 0)
                element.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.3f);

            // Special style for composite conditions
            if (condition.DataType == ConditionDataType.Composite)
            {
                element.style.backgroundColor = new Color(0.2f, 0.3f, 0.4f, 0.4f);
            }
        }

        private void CreateStandardConditionContent(VisualElement container, ConditionData condition)
        {
            // Drag handle
            VisualElement dragHandle = CreateDragHandle();
            container.Add(dragHandle);

            // Parameter name field (editable)
            TextField paramNameField = new();
            paramNameField.AddToClassList("parameter-field");
            paramNameField.value = condition.ParameterName;
            paramNameField.RegisterValueChangedCallback(evt =>
            {
                condition.ParameterName = evt.newValue;
                SaveConditions();
            });

            container.Add(paramNameField);

            // Value and comparison type - type-specific UI
            VisualElement valueContainer = CreateValueEditor(condition);
            container.Add(valueContainer);

            // Remove button
            Button removeButton = new(() => RemoveCondition(condition)) { text = "×" };
            removeButton.AddToClassList("remove-button");
            container.Add(removeButton);
        }

        private void CreateCompositeConditionContent(VisualElement container, ConditionData condition)
        {
            // Drag handle
            VisualElement dragHandle = CreateDragHandle();
            container.Add(dragHandle);

            // Composite type indicator
            string compositeType = condition.StringValue;
            string typeName = string.IsNullOrEmpty(compositeType) ? "AND" : compositeType.ToUpper();
            Label compositeLabel = new(typeName);
            compositeLabel.AddToClassList("condition-type-tag");
            compositeLabel.AddToClassList(typeName.ToLower());
            container.Add(compositeLabel);

            // Toggle between AND/OR
            Button toggleTypeButton = new(() => ToggleCompositeType(condition)) { text = "Toggle" };
            toggleTypeButton.AddToClassList("quick-add-button");
            toggleTypeButton.style.minWidth = 60;
            container.Add(toggleTypeButton);

            // Spacer
            VisualElement spacer = new();
            spacer.AddToClassList("flex-container");
            container.Add(spacer);

            // Remove button
            Button removeButton = new(() => RemoveCondition(condition)) { text = "×" };
            removeButton.AddToClassList("condition-action-button");
            container.Add(removeButton);
        }

        private VisualElement CreateDragHandle()
        {
            VisualElement dragHandle = new();
            dragHandle.style.width = 12;
            dragHandle.style.height = 20;
            dragHandle.style.marginRight = 5;
            dragHandle.style.backgroundImage =
                EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon").image as Texture2D;

            dragHandle.style.opacity = 0.6f;
            return dragHandle;
        }

        private VisualElement CreateValueEditor(ConditionData condition)
        {
            VisualElement container = new();
            container.style.flexDirection = FlexDirection.Row;

            switch (condition.DataType)
            {
                case ConditionDataType.Boolean:
                    CreateBooleanEditor(container, condition);
                    break;
                case ConditionDataType.Float:
                    CreateFloatEditor(container, condition);
                    break;
                case ConditionDataType.Integer:
                    CreateIntegerEditor(container, condition);
                    break;
                case ConditionDataType.String:
                    CreateStringEditor(container, condition);
                    break;
                case ConditionDataType.Time:
                    CreateTimeEditor(container, condition);
                    break;
                case ConditionDataType.Animation:
                    CreateAnimationEditor(container, condition);
                    break;
            }

            return container;
        }

        private void CreateBooleanEditor(VisualElement container, ConditionData condition)
        {
            // Comparison type dropdown (Is/Is Not)
            EnumField comparisonField = new(condition.ComparisonType);
            comparisonField.style.width = 80;
            comparisonField.style.marginRight = 5;
            comparisonField.RegisterValueChangedCallback(evt =>
            {
                condition.ComparisonType = (ComparisonType)evt.newValue;
                SaveConditions();
            });

            container.Add(comparisonField);

            // Toggle for boolean value
            Toggle valueToggle = new("Value");
            valueToggle.value = condition.BoolValue;
            valueToggle.RegisterValueChangedCallback(evt =>
            {
                condition.BoolValue = evt.newValue;
                SaveConditions();
            });

            container.Add(valueToggle);
        }

        private void CreateFloatEditor(VisualElement container, ConditionData condition)
        {
            // Comparison type dropdown
            EnumField comparisonField = new(condition.ComparisonType);
            comparisonField.style.width = 80;
            comparisonField.style.marginRight = 5;
            comparisonField.RegisterValueChangedCallback(evt =>
            {
                condition.ComparisonType = (ComparisonType)evt.newValue;
                SaveConditions();
            });

            container.Add(comparisonField);

            // Float value field
            FloatField valueField = new();
            valueField.value = condition.FloatValue;
            valueField.RegisterValueChangedCallback(evt =>
            {
                condition.FloatValue = evt.newValue;
                SaveConditions();
            });

            container.Add(valueField);
        }

        private void CreateIntegerEditor(VisualElement container, ConditionData condition)
        {
            // Comparison type dropdown
            EnumField comparisonField = new(condition.ComparisonType);
            comparisonField.style.width = 80;
            comparisonField.style.marginRight = 5;
            comparisonField.RegisterValueChangedCallback(evt =>
            {
                condition.ComparisonType = (ComparisonType)evt.newValue;
                SaveConditions();
            });

            container.Add(comparisonField);

            // Integer value field
            IntegerField valueField = new();
            valueField.value = condition.IntValue;
            valueField.RegisterValueChangedCallback(evt =>
            {
                condition.IntValue = evt.newValue;
                SaveConditions();
            });

            container.Add(valueField);
        }

        private void CreateStringEditor(VisualElement container, ConditionData condition)
        {
            // Comparison type dropdown
            EnumField comparisonField = new(condition.ComparisonType);
            comparisonField.style.width = 80;
            comparisonField.style.marginRight = 5;
            comparisonField.RegisterValueChangedCallback(evt =>
            {
                condition.ComparisonType = (ComparisonType)evt.newValue;
                SaveConditions();
            });

            container.Add(comparisonField);

            // String value field
            TextField valueField = new();
            valueField.style.flexGrow = 1;
            valueField.value = condition.StringValue;
            valueField.RegisterValueChangedCallback(evt =>
            {
                condition.StringValue = evt.newValue;
                SaveConditions();
            });

            container.Add(valueField);
        }

        private void CreateTimeEditor(VisualElement container, ConditionData condition)
        {
            // Label for time condition
            Label timeLabel = new("Wait ");
            container.Add(timeLabel);

            // Float value field for seconds
            FloatField valueField = new();
            valueField.style.width = 50;
            valueField.value = condition.FloatValue;
            valueField.RegisterValueChangedCallback(evt =>
            {
                condition.FloatValue = evt.newValue;
                SaveConditions();
            });

            container.Add(valueField);

            // Seconds label
            Label secondsLabel = new(" seconds");
            container.Add(secondsLabel);
        }

        private void CreateAnimationEditor(VisualElement container, ConditionData condition)
        {
            // Just a label for animation complete condition
            Label animLabel = new("Animation Complete");
            container.Add(animLabel);
        }

        private void RefreshNewConditionFields()
        {
            // Get the container for type-specific fields
            VisualElement container = _contentContainer.Q("typeSpecificContainer");
            if (container == null) return;

            // Clear existing fields
            container.Clear();

            // Add appropriate fields based on the selected condition type
            switch (_newConditionType)
            {
                case ConditionDataType.Boolean:
                    // Parameter name field
                    TextField boolParamField = new("Parameter Name");
                    container.Add(boolParamField);

                    // Comparison type dropdown
                    EnumField boolComparisonField = new("Comparison", ComparisonType.IsTrue);
                    container.Add(boolComparisonField);
                    break;

                case ConditionDataType.Float:
                    // Parameter name field
                    TextField floatParamField = new("Parameter Name");
                    container.Add(floatParamField);

                    // Comparison type dropdown
                    EnumField floatComparisonField = new("Comparison", ComparisonType.Equals);
                    container.Add(floatComparisonField);

                    // Value field
                    FloatField floatValueField = new("Value");
                    container.Add(floatValueField);
                    break;

                case ConditionDataType.Integer:
                    // Parameter name field
                    TextField intParamField = new("Parameter Name");
                    container.Add(intParamField);

                    // Comparison type dropdown
                    EnumField intComparisonField = new("Comparison", ComparisonType.Equals);
                    container.Add(intComparisonField);

                    // Value field
                    IntegerField intValueField = new("Value");
                    container.Add(intValueField);
                    break;

                case ConditionDataType.String:
                    // Parameter name field
                    TextField stringParamField = new("Parameter Name");
                    container.Add(stringParamField);

                    // Comparison type dropdown
                    EnumField stringComparisonField = new("Comparison", ComparisonType.Equals);
                    container.Add(stringComparisonField);

                    // Value field
                    TextField stringValueField = new("Value");
                    container.Add(stringValueField);
                    break;

                case ConditionDataType.Composite:
                    // Composite type field (AND/OR)
                    Label compositeTypeLabel = new("Group Type");
                    container.Add(compositeTypeLabel);

                    VisualElement radioGroup = new();
                    radioGroup.style.flexDirection = FlexDirection.Row;

                    Toggle andToggle = new("AND");
                    andToggle.value = true;
                    andToggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue) _newCompositeType = "And";
                    });

                    radioGroup.Add(andToggle);

                    Toggle orToggle = new("OR");
                    orToggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue) _newCompositeType = "Or";
                        andToggle.SetValueWithoutNotify(!evt.newValue);
                    });

                    radioGroup.Add(orToggle);

                    container.Add(radioGroup);
                    break;
            }
        }

        private void AddNewCondition()
        {
            ConditionData condition = new()
            {
                DataType = _newConditionType
            };

            // Get values from the UI fields
            VisualElement container = _contentContainer.Q("typeSpecificContainer");
            if (container != null)
            {
                switch (_newConditionType)
                {
                    case ConditionDataType.Boolean:
                        TextField boolParamField = container.Q<TextField>();
                        if (boolParamField != null) condition.ParameterName = boolParamField.value;

                        EnumField boolComparisonField = container.Q<EnumField>();
                        if (boolComparisonField != null)
                            condition.ComparisonType = (ComparisonType)boolComparisonField.value;

                        break;

                    case ConditionDataType.Float:
                        TextField floatParamField = container.Q<TextField>();
                        if (floatParamField != null) condition.ParameterName = floatParamField.value;

                        EnumField floatComparisonField = container.Q<EnumField>();
                        if (floatComparisonField != null)
                            condition.ComparisonType = (ComparisonType)floatComparisonField.value;

                        FloatField floatValueField = container.Q<FloatField>();
                        if (floatValueField != null) condition.FloatValue = floatValueField.value;
                        break;

                    case ConditionDataType.Integer:
                        TextField intParamField = container.Q<TextField>();
                        if (intParamField != null) condition.ParameterName = intParamField.value;

                        EnumField intComparisonField = container.Q<EnumField>();
                        if (intComparisonField != null)
                            condition.ComparisonType = (ComparisonType)intComparisonField.value;

                        IntegerField intValueField = container.Q<IntegerField>();
                        if (intValueField != null) condition.IntValue = intValueField.value;
                        break;

                    case ConditionDataType.String:
                        TextField stringParamField = container.Q<TextField>();
                        if (stringParamField != null) condition.ParameterName = stringParamField.value;

                        EnumField stringComparisonField = container.Q<EnumField>();
                        if (stringComparisonField != null)
                            condition.ComparisonType = (ComparisonType)stringComparisonField.value;

                        TextField stringValueField = container.Q<TextField>();
                        if (stringValueField != null) condition.StringValue = stringValueField.value;
                        break;

                    case ConditionDataType.Composite:
                        // Store the composite type in StringValue
                        condition.StringValue = _newCompositeType;
                        break;
                }
            }

            _conditions.Add(condition);
            SaveConditions();
            RefreshConditionsList();
        }

        private void AddCompositeCondition(string type)
        {
            ConditionData condition = new()
            {
                DataType = ConditionDataType.Composite,
                StringValue = type // Store AND/OR in StringValue
            };

            _conditions.Add(condition);
            SaveConditions();
            RefreshConditionsList();
        }

        private void ToggleCompositeType(ConditionData condition)
        {
            if (condition.DataType != ConditionDataType.Composite) return;

            // Toggle between And and Or
            condition.StringValue = condition.StringValue == "And" ? "Or" : "And";
            SaveConditions();
            RefreshConditionsList();
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

        private void RemoveCondition(ConditionData condition)
        {
            // Check if it's a composite and find all child conditions
            if (condition.DataType == ConditionDataType.Composite)
            {
                // Find child conditions that have this as parent
                var childConditions = _conditions.Where(c => c.ParentGroupId == condition.UniqueId).ToList();

                // Update their parent group to the parent of the composite being removed
                foreach (ConditionData child in childConditions)
                {
                    child.ParentGroupId = condition.ParentGroupId;
                    child.NestingLevel = Math.Max(0, condition.NestingLevel); // Keep same nesting level or 0
                }
            }

            // Remove the condition
            _conditions.Remove(condition);

            // Update indices of remaining conditions
            UpdateConditionIndices();

            SaveConditions();
            RefreshConditionsList();
        }

        private void UpdateConditionIndices()
        {
            // Group conditions by parent group
            var groupedConditions = _conditions.GroupBy(c => c.ParentGroupId);

            // For each group, update the indices
            foreach (var group in groupedConditions)
            {
                int index = 0;
                foreach (ConditionData condition in group.OrderBy(c => c.GroupIndex))
                {
                    condition.GroupIndex = index++;
                }
            }
        }

        private void MakeElementDraggable(VisualElement element)
        {
            // MouseDown - start drag
            element.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0 && !_isDraggingCondition) // Left button
                {
                    _draggedElement = element;
                    _draggedCondition = element.userData as ConditionData;
                    _draggedStartIndex = _conditions.IndexOf(_draggedCondition);
                    _isDraggingCondition = true;

                    // Visual feedback
                    element.style.opacity = 0.6f;
                    evt.StopPropagation();
                }
            });

            // MouseMove - update drop indicator
            element.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (_isDraggingCondition && _draggedElement != null)
                {
                    evt.StopPropagation();
                }
            });

            // MouseUp - drop
            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (_isDraggingCondition && _draggedElement != null)
                {
                    _isDraggingCondition = false;
                    _draggedElement.style.opacity = 1.0f;
                    _draggedElement = null;
                    _dropIndicator.style.display = DisplayStyle.None;
                    evt.StopPropagation();
                }
            });

            // DragEnter - show drop indicator
            element.RegisterCallback<MouseOverEvent>(evt =>
            {
                if (_isDraggingCondition && _draggedElement != null && _draggedElement != element)
                {
                    // Get target condition
                    ConditionData targetCondition = element.userData as ConditionData;
                    if (targetCondition == null) return;

                    // Show drop indicator
                    _dropIndicator.style.display = DisplayStyle.Flex;

                    // Position drop indicator
                    int targetIndex = _conditionsContainer.IndexOf(element);
                    _conditionsContainer.Remove(_dropIndicator);
                    _conditionsContainer.Insert(targetIndex, _dropIndicator);

                    // Handle drop logic
                    HandleDropOperation(targetCondition, evt.mousePosition);

                    evt.StopPropagation();
                }
            });
        }

        private void HandleDropOperation(ConditionData targetCondition, Vector2 mousePosition)
        {
            if (_draggedCondition == null || targetCondition == null) return;

            // Don't allow dropping a parent onto its child
            if (IsParentOf(_draggedCondition, targetCondition)) return;

            // Get the Y position within the target element to determine if dropping above, inside, or below
            VisualElement targetElement = _conditionElements[targetCondition.UniqueId];
            float relativeY = mousePosition.y - targetElement.worldBound.y;
            float elementHeight = targetElement.worldBound.height;

            if (targetCondition.DataType == ConditionDataType.Composite)
            {
                // For composites, determine if dropping inside or above/below
                if (relativeY > elementHeight * 0.3f && relativeY < elementHeight * 0.7f)
                {
                    // Dropping inside the composite
                    MoveConditionToComposite(_draggedCondition, targetCondition);
                }
                else if (relativeY <= elementHeight * 0.3f)
                {
                    // Dropping above the composite
                    MoveConditionAbove(_draggedCondition, targetCondition);
                }
                else
                {
                    // Dropping below the composite
                    MoveConditionBelow(_draggedCondition, targetCondition);
                }
            }
            else
            {
                // For regular conditions, determine if dropping above or below
                if (relativeY <= elementHeight * 0.5f)
                {
                    // Dropping above
                    MoveConditionAbove(_draggedCondition, targetCondition);
                }
                else
                {
                    // Dropping below
                    MoveConditionBelow(_draggedCondition, targetCondition);
                }
            }

            // Save changes
            SaveConditions();
            RefreshConditionsList();
        }

        private bool IsParentOf(ConditionData potentialParent, ConditionData potentialChild)
        {
            if (potentialParent.DataType != ConditionDataType.Composite) return false;

            // Check if the child has this parent directly
            if (potentialChild.ParentGroupId == potentialParent.UniqueId) return true;

            // Check ancestors recursively
            foreach (ConditionData condition in _conditions)
            {
                if (condition.ParentGroupId == potentialParent.UniqueId)
                {
                    if (IsParentOf(condition, potentialChild)) return true;
                }
            }

            return false;
        }

        private void MoveConditionToComposite(ConditionData conditionToMove, ConditionData compositeCondition)
        {
            if (compositeCondition.DataType != ConditionDataType.Composite) return;

            // Find all conditions in the target composite
            var childrenInComposite = _conditions.Where(c => c.ParentGroupId == compositeCondition.UniqueId).ToList();

            // Set new parent and update nesting level
            conditionToMove.ParentGroupId = compositeCondition.UniqueId;
            conditionToMove.NestingLevel = compositeCondition.NestingLevel + 1;
            conditionToMove.GroupIndex =
                childrenInComposite.Count > 0 ? childrenInComposite.Max(c => c.GroupIndex) + 1 : 0;
        }

        private void MoveConditionAbove(ConditionData conditionToMove, ConditionData targetCondition)
        {
            // Set same parent group as the target
            conditionToMove.ParentGroupId = targetCondition.ParentGroupId;
            conditionToMove.NestingLevel = targetCondition.NestingLevel;

            // Get all conditions with the same parent
            var siblingsConditions = _conditions
                .Where(c => c.ParentGroupId == targetCondition.ParentGroupId)
                .OrderBy(c => c.GroupIndex)
                .ToList();

            // Shift indices to make room
            foreach (ConditionData sibling in siblingsConditions)
            {
                if (sibling.GroupIndex >= targetCondition.GroupIndex && sibling != conditionToMove)
                {
                    sibling.GroupIndex++;
                }
            }

            // Set the index of the moved condition
            conditionToMove.GroupIndex = targetCondition.GroupIndex;
        }

        private void MoveConditionBelow(ConditionData conditionToMove, ConditionData targetCondition)
        {
            // Set same parent group as the target
            conditionToMove.ParentGroupId = targetCondition.ParentGroupId;
            conditionToMove.NestingLevel = targetCondition.NestingLevel;

            // Get all conditions with the same parent
            var siblingsConditions = _conditions
                .Where(c => c.ParentGroupId == targetCondition.ParentGroupId)
                .OrderBy(c => c.GroupIndex)
                .ToList();

            // Shift indices to make room
            foreach (ConditionData sibling in siblingsConditions)
            {
                if (sibling.GroupIndex > targetCondition.GroupIndex && sibling != conditionToMove)
                {
                    sibling.GroupIndex++;
                }
            }

            // Set the index of the moved condition
            conditionToMove.GroupIndex = targetCondition.GroupIndex + 1;
        }

        private void SaveConditions()
        {
            if (!string.IsNullOrEmpty(_edgeId))
            {
                EdgeConditionManager.Instance.SetConditions(_edgeId, _conditions);

                // Mark the asset dirty so changes are saved
                AnimationFlowEditorWindow editorWindow = EditorWindow.GetWindow<AnimationFlowEditorWindow>();
                if (editorWindow != null)
                {
                    EditorUtility.SetDirty(editorWindow);
                }
            }
        }
    }


}
