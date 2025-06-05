using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Conditions;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Core.Types;
using Animation.Flow.Editor.Factories;
using Animation.Flow.Parameters;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels.Conditions
{
    /// <summary>
    ///     Panel showing the list of conditions
    /// </summary>
    public class ConditionListPanel : DraggablePanel<ScrollView>
    {

        #region Constructor

        public ConditionListPanel(VisualElement parentContainer)
        {
            // Set resize handle position before base constructor
            ResizeHandlePos = ResizeHandlePosition.BottomLeft;

            // Call base constructor which will use our handle position
            Initialize(parentContainer, "Conditions", new Vector2(320, 10));
            // Set resize handle position before creating UI
            ResizeHandlePos = ResizeHandlePosition.BottomLeft;
            // Force refresh resize handle
            VisualElement oldHandle = this.Q(null, "panel-resize-handle-bottom-right");
            if (oldHandle != null) oldHandle.RemoveFromHierarchy();
            CreateResizeHandle();
            // Set resize handle to bottom left
            ResizeHandlePos = ResizeHandlePosition.BottomLeft;

            // Set resize handle to bottom left
            ResizeHandlePos = ResizeHandlePosition.BottomLeft;
            // Initialize view factory and drag handler
            _viewFactory = new ConditionViewFactory(this);

            _dragHandler = new DragHandler<FlowCondition>(Content, "FlowCondition", "drag-handle");
            _dragHandler.OnItemsReordered += OnConditionsReordered;

            // Load the condition list panel stylesheet
            StyleSheet conditionListStylesheet = Resources.Load<StyleSheet>("Stylesheets/ConditionListPanelStyles");

            if (conditionListStylesheet)
            {
                styleSheets.Add(conditionListStylesheet);
            }

            // Create drop indicator element
            _dropIndicator = new VisualElement();
            _dropIndicator.AddToClassList("drop-indicator");


            RegisterDropEvents();
        }

        #endregion

        #region Fields

        private readonly Label _titleLabel;
        private readonly ConditionViewFactory _viewFactory;
        private readonly DragHandler<FlowCondition> _dragHandler;
        private List<FlowCondition> _conditions;
        private readonly VisualElement _dropIndicator;
        public event Action<List<FlowCondition>> OnConditionsChanged;

        #endregion

        #region Overrides

        protected override ScrollView CreateContent()
        {
            ScrollView scrollView = new();
            scrollView.AddToClassList("conditions-scroll-view");

            // Add name for CSS debugging
            scrollView.name = "ConditionScrollView";

            // Configure the content container
            scrollView.contentContainer.style.flexGrow = 1;
            scrollView.contentContainer.style.flexShrink = 1;
            scrollView.contentContainer.style.width = new StyleLength(Length.Percent(100));

            return scrollView;
        }

        protected override void OnContentCreated(ScrollView content)
        {
            ContentContainer.Add(content);
            // All styles are now in DraggablePanel.uss stylesheet
        }

        #endregion

        #region Public Methods

        public void Show(List<FlowCondition> conditions, string title)
        {
            base.Show();
            _conditions = conditions;
            if (_titleLabel != null)
                _titleLabel.text = title;

            RefreshConditionsList();
        }

        public void PrepareForParameterDrop(FlowParameter parameter)
        {
            // Only prepare for drop if the panel is visible
            if (style.display == DisplayStyle.None)
                return;

            // Make sure we're visible and in the DOM
            Show();

            // Store the parameter being dragged for later use in OnDragPerform
            DragAndDrop.SetGenericData("FlowParameter", parameter);

            // Make sure the drop indicator is initialized
            if (_dropIndicator != null)
            {
                _dropIndicator.style.display = DisplayStyle.None;
            }
        }

        public void RemoveCondition(FlowCondition condition)
        {
            if (condition.ConditionType == ConditionType.Composite)
            {
                var orphans = _conditions.Where(c => c.parentGroupId == condition.uniqueId).ToList();
                foreach (FlowCondition orphan in orphans)
                {
                    orphan.parentGroupId = condition.parentGroupId;
                    orphan.nestingLevel = Math.Max(0, condition.nestingLevel);
                }
            }

            _conditions.Remove(condition);
            OnConditionsChanged?.Invoke(_conditions);
            RefreshConditionsList();
        }

        public void UpdateCondition(FlowCondition condition)
        {
            OnConditionsChanged?.Invoke(_conditions);
        }

        public void MoveConditionToComposite(FlowCondition condition, FlowCondition composite)
        {
            condition.parentGroupId = composite.uniqueId;
            condition.nestingLevel = composite.nestingLevel + 1;
            OnConditionsChanged?.Invoke(_conditions);
            RefreshConditionsList();
        }

        public void MoveCompositeToComposite(FlowCondition sourceComposite, FlowCondition targetComposite)
        {
            if (sourceComposite.ConditionType != ConditionType.Composite ||
                targetComposite.ConditionType != ConditionType.Composite)
                return;

            // Prevent circular references
            if (sourceComposite.uniqueId == targetComposite.uniqueId ||
                IsConditionInsideComposite(targetComposite, sourceComposite))
                return;

            // Get all children of the source composite
            var childConditions = GetAllChildrenOfComposite(sourceComposite);

            // Update the source composite's parent and nesting
            sourceComposite.parentGroupId = targetComposite.uniqueId;
            sourceComposite.nestingLevel = targetComposite.nestingLevel + 1;

            // Update all children's nesting level (parent ID stays the same)
            foreach (FlowCondition child in childConditions)
            {
                child.nestingLevel = sourceComposite.nestingLevel + 1;
            }

            OnConditionsChanged?.Invoke(_conditions);
            RefreshConditionsList();
        }

        /// <summary>
        ///     Get all conditions that are children of the specified composite
        /// </summary>
        private List<FlowCondition> GetAllChildrenOfComposite(FlowCondition composite)
        {
            var result = new List<FlowCondition>();

            // First, get direct children
            var directChildren = _conditions.Where(c => c.parentGroupId == composite.uniqueId).ToList();
            result.AddRange(directChildren);

            // Then, recursively get children of any composite children
            foreach (FlowCondition child in directChildren)
            {
                if (child.ConditionType == ConditionType.Composite)
                {
                    result.AddRange(GetAllChildrenOfComposite(child));
                }
            }

            return result;
        }

        /// <summary>
        ///     Check if a condition is inside a specific composite (directly or nested)
        /// </summary>
        private bool IsConditionInsideComposite(FlowCondition condition, FlowCondition composite)
        {
            if (condition.parentGroupId == composite.uniqueId)
                return true;

            // If it's not a direct child, check if it's inside any of the composite's children
            FlowCondition conditionParent = _conditions.FirstOrDefault(c => c.uniqueId == condition.parentGroupId);
            if (conditionParent == null)
                return false;

            return IsConditionInsideComposite(conditionParent, composite);
        }

        #endregion

        #region Drop Handling

        private void RegisterDropEvents()
        {
            Content.RegisterCallback<DragEnterEvent>(OnDragEnter);
            Content.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            Content.RegisterCallback<DragPerformEvent>(OnDragPerform);
            Content.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            Content.RegisterCallback<DragExitedEvent>(OnDragLeave); // Additional handler for when drag exits

            // Register to parent container as well to ensure we catch drag events
            ParentContainer.RegisterCallback<DragExitedEvent>(OnDragLeave);
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.GetGenericData("FlowParameter") is FlowParameter ||
                DragAndDrop.GetGenericData("FlowCondition") is FlowCondition)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.StopPropagation();
            }
        }

        private float _lastDragUpdateTime;
        private Vector2 _lastDragPosition;

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            // Throttle updates to prevent flickering
            float currentTime = (float)EditorApplication.timeSinceStartup;
            if (currentTime - _lastDragUpdateTime > 0.05f ||
                Vector2.Distance(_lastDragPosition, evt.localMousePosition) > 5f)
            {
                _lastDragUpdateTime = currentTime;
                _lastDragPosition = evt.localMousePosition;
                UpdateDropIndicator(evt.localMousePosition);
            }

            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            // Handle parameter drop to create a new condition
            if (DragAndDrop.GetGenericData("FlowParameter") is FlowParameter parameter)
            {
                FlowCondition newCondition = CreateConditionFromParameter(parameter);
                int dropIndex = GetDropIndex(evt.localMousePosition);

                // Validate drop index to prevent out of range exceptions
                dropIndex = Mathf.Clamp(dropIndex, 0, _conditions.Count);

                _conditions.Insert(dropIndex, newCondition);
                OnConditionsChanged?.Invoke(_conditions);
                RefreshConditionsList();
            }
            // Handle condition drop to create a composite condition
            else if (DragAndDrop.GetGenericData("FlowCondition") is FlowCondition draggedCondition)
            {
                // Find the drop target condition - this is for dropping onto a specific item
                VisualElement targetElement = evt.target as VisualElement;
                while (targetElement is { userData: not FlowCondition })
                {
                    targetElement = targetElement.parent;
                }

                if (targetElement?.userData is FlowCondition targetCondition)
                {
                    // Don't allow dropping onto itself
                    if (targetCondition == draggedCondition)
                    {
                        evt.StopPropagation();
                        _dropIndicator.style.display = DisplayStyle.None;
                        return;
                    }

                    // Check if the target is inside the dragged composite (prevent circular nesting)
                    if (draggedCondition.ConditionType == ConditionType.Composite &&
                        IsConditionInsideComposite(targetCondition, draggedCondition))
                    {
                        evt.StopPropagation();
                        _dropIndicator.style.display = DisplayStyle.None;
                        return;
                    }

                    // Check if the target is already a composite - if so, just move the dragged condition inside it
                    if (targetCondition.ConditionType == ConditionType.Composite)
                    {
                        // If we're dragging a composite, move all its children too
                        if (draggedCondition.ConditionType == ConditionType.Composite)
                        {
                            MoveCompositeToComposite(draggedCondition, targetCondition);
                        }
                        else
                        {
                            MoveConditionToComposite(draggedCondition, targetCondition);
                        }
                    }
                    // If we're dragging a composite onto a condition, add the condition to the composite
                    else if (draggedCondition.ConditionType == ConditionType.Composite)
                    {
                        // Check for circular references - don't allow a composite to be added to itself
                        if (draggedCondition.parentGroupId == targetCondition.uniqueId ||
                            IsConditionInsideComposite(draggedCondition, targetCondition))
                        {
                            evt.StopPropagation();
                            _dropIndicator.style.display = DisplayStyle.None;
                            return;
                        }

                        // Move the condition into the dragged composite
                        targetCondition.parentGroupId = draggedCondition.uniqueId;
                        targetCondition.nestingLevel = draggedCondition.nestingLevel + 1;

                        OnConditionsChanged?.Invoke(_conditions);
                        RefreshConditionsList();
                    }
                    else
                    {
                        // Create a new composite condition to hold both
                        FlowCondition composite = CreateCompositeCondition();

                        // Inherit parent and nesting from the target condition
                        composite.parentGroupId = targetCondition.parentGroupId;
                        composite.nestingLevel = targetCondition.nestingLevel;

                        // Insert the composite in place of the target
                        int targetIndex = _conditions.IndexOf(targetCondition);
                        if (targetIndex >= 0)
                        {
                            _conditions.Insert(targetIndex, composite);

                            // Move target condition into the composite
                            targetCondition.parentGroupId = composite.uniqueId;
                            targetCondition.nestingLevel = composite.nestingLevel + 1;

                            // If dragging a composite, move it and all its children
                            if (draggedCondition.ConditionType == ConditionType.Composite)
                            {
                                if (_conditions.Remove(draggedCondition)) // If we're moving from the same list
                                {
                                    // Get all children before removing them
                                    var childConditions = GetAllChildrenOfComposite(draggedCondition);

                                    // Remove all children from their current location
                                    foreach (FlowCondition child in childConditions)
                                    {
                                        _conditions.Remove(child);
                                    }

                                    // Insert the composite at the right position
                                    draggedCondition.parentGroupId = composite.uniqueId;
                                    draggedCondition.nestingLevel = composite.nestingLevel + 1;

                                    // Re-evaluate target index after removal
                                    targetIndex = _conditions.IndexOf(composite);
                                    if (targetIndex >= 0) // Make sure our composite is still in the list
                                        _conditions.Insert(targetIndex + 1, draggedCondition);
                                    else
                                        _conditions.Add(draggedCondition); // Fallback if composite not found

                                    // Reinsert all children with updated parent references
                                    foreach (FlowCondition child in childConditions)
                                    {
                                        // Don't change the parent ID, as it still points to the dragged composite
                                        child.nestingLevel = draggedCondition.nestingLevel + 1;
                                        _conditions.Add(
                                            child); // Add at the end, order will be fixed in RefreshConditionsList
                                    }
                                }
                            }
                            else if (_conditions.Remove(draggedCondition)) // If we're moving a regular condition
                            {
                                draggedCondition.parentGroupId = composite.uniqueId;
                                draggedCondition.nestingLevel = composite.nestingLevel + 1;

                                // Re-evaluate target index after removal
                                targetIndex = _conditions.IndexOf(composite);
                                if (targetIndex >= 0) // Make sure our composite is still in the list
                                    _conditions.Insert(targetIndex + 1, draggedCondition);
                                else
                                    _conditions.Add(draggedCondition); // Fallback if composite not found
                            }

                            OnConditionsChanged?.Invoke(_conditions);
                            RefreshConditionsList();
                        }
                    }
                }
                else
                {
                    // Handle dropping into empty space or non-condition areas
                    int dropIndex = GetDropIndex(evt.localMousePosition);

                    // If dragging a composite, handle it and all its children
                    if (draggedCondition.ConditionType == ConditionType.Composite &&
                        _conditions.Contains(draggedCondition))
                    {
                        // Get all children before removing them
                        var childConditions = GetAllChildrenOfComposite(draggedCondition);

                        // Remove the composite and all its children
                        _conditions.Remove(draggedCondition);
                        foreach (FlowCondition child in childConditions)
                        {
                            _conditions.Remove(child);
                        }

                        // Reset parent for the composite when dropping into empty space
                        draggedCondition.parentGroupId = string.Empty;
                        draggedCondition.nestingLevel = 0;

                        // Revalidate drop index after removal
                        dropIndex = Mathf.Clamp(dropIndex, 0, _conditions.Count);
                        _conditions.Insert(dropIndex, draggedCondition);

                        // Reinsert all children (their parent ID still points to the composite)
                        foreach (FlowCondition child in childConditions)
                        {
                            child.nestingLevel = draggedCondition.nestingLevel + 1;
                            _conditions.Add(child); // Add at the end, order will be fixed in RefreshConditionsList
                        }

                        OnConditionsChanged?.Invoke(_conditions);
                        RefreshConditionsList();
                    }
                    else if (_conditions.Remove(draggedCondition)) // For regular conditions
                    {
                        // Reset parent group id when dropping into empty space
                        draggedCondition.parentGroupId = string.Empty;
                        draggedCondition.nestingLevel = 0;

                        // Revalidate drop index after removal
                        dropIndex = Mathf.Clamp(dropIndex, 0, _conditions.Count);
                        _conditions.Insert(dropIndex, draggedCondition);
                        OnConditionsChanged?.Invoke(_conditions);
                        RefreshConditionsList();
                    }
                }
            }

            _dropIndicator.style.display = DisplayStyle.None;
            evt.StopPropagation();
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            if (_dropIndicator != null)
            {
                // First remove it from hierarchy if it's there
                if (_dropIndicator.parent != null)
                    _dropIndicator.parent.Remove(_dropIndicator);

                _dropIndicator.style.display = DisplayStyle.None;
            }
        }

        // Handle DragExitedEvent the same way as DragLeaveEvent
        private void OnDragLeave(DragExitedEvent evt)
        {
            if (_dropIndicator != null)
            {
                // First remove it from hierarchy if it's there
                if (_dropIndicator.parent != null)
                    _dropIndicator.parent.Remove(_dropIndicator);

                _dropIndicator.style.display = DisplayStyle.None;
            }
        }

        private void UpdateDropIndicator(Vector2 localPosition)
        {
            // First remove the indicator if it's already in the hierarchy
            if (_dropIndicator.parent != null)
                _dropIndicator.parent.Remove(_dropIndicator);

            int dropIndex = GetDropIndex(localPosition);
            _dropIndicator.style.display = DisplayStyle.Flex;

            var contentChildren = Content.Children().ToList();
            int actualChildCount = contentChildren.Count;

            // Insert at the appropriate position
            if (dropIndex >= 0 && dropIndex < actualChildCount)
                Content.Insert(dropIndex, _dropIndicator);
            else
                Content.Add(_dropIndicator);
        }

        private int GetDropIndex(Vector2 localPosition)
        {
            var children = Content.Children().Where(c => c != _dropIndicator).ToList();

            // If list is empty, return 0
            if (children.Count == 0)
                return 0;

            // Convert local position to world position for comparison with worldBound
            Vector2 worldPosition = Content.LocalToWorld(localPosition);

            // First check if we're above the first element
            if (worldPosition.y < children[0].worldBound.yMin + 5)
                return 0;

            // Then check if we're below the last element
            if (worldPosition.y > children[children.Count - 1].worldBound.yMax - 5)
                return children.Count;

            // Then check between elements
            for (int i = 0; i < children.Count - 1; i++)
            {
                Rect currentBounds = children[i].worldBound;
                Rect nextBounds = children[i + 1].worldBound;

                // Check if mouse is between these two elements
                float midpoint = (currentBounds.yMax + nextBounds.yMin) / 2;
                if (worldPosition.y < midpoint)
                    return i + 1;
            }

            // Default to end of list if nothing else matched
            return children.Count;
        }

        #endregion

        #region Condition Management

        private FlowCondition CreateConditionFromParameter(FlowParameter parameter)
        {
            // Create appropriate condition type based on parameter type
            if (parameter.ParameterType == typeof(bool))
            {
                return new BoolCondition(parameter.Name)
                {
                    uniqueId = Guid.NewGuid().ToString(),
                    ParameterName = parameter.Name,
                    BoolValue = true,
                    nestingLevel = 0,
                    parentGroupId = string.Empty
                };
            }

            if (parameter.ParameterType == typeof(int))
            {
                return new IntCondition(parameter.Name, 0)
                {
                    uniqueId = Guid.NewGuid().ToString(),
                    ParameterName = parameter.Name,
                    IntValue = 0,
                    nestingLevel = 0,
                    parentGroupId = string.Empty
                };
            }

            if (parameter.ParameterType == typeof(float))
            {
                return new FloatCondition(parameter.Name, 0f)
                {
                    uniqueId = Guid.NewGuid().ToString(),
                    ParameterName = parameter.Name,
                    FloatValue = 0f,
                    nestingLevel = 0,
                    parentGroupId = string.Empty
                };
            }

            if (parameter.ParameterType == typeof(string))
            {
                return new StringCondition(parameter.Name, "")
                {
                    uniqueId = Guid.NewGuid().ToString(),
                    ParameterName = parameter.Name,
                    StringValue = "",
                    nestingLevel = 0,
                    parentGroupId = string.Empty
                };
            }

            Debug.LogWarning($"Unsupported parameter type: {parameter.ParameterType}");
            return null;
        }


        /// <summary>
        ///     Creates a new composite condition (AND/OR group)
        /// </summary>
        private FlowCondition CreateCompositeCondition() => new CompositeCondition
        {
            uniqueId = Guid.NewGuid().ToString(),
            StringValue = nameof(CompositeType.All)
        };

        private void RefreshConditionsList()
        {
            Content.Clear();

            if (_conditions == null || _conditions.Count == 0)
            {
                Label emptyLabel = new("No conditions. Drag parameters here.");
                emptyLabel.AddToClassList("empty-state-label");
                Content.Add(emptyLabel);
                return;
            }

            RenderConditions(_conditions.Where(c => string.IsNullOrEmpty(c.parentGroupId)), Content);
        }

        private void RenderConditions(IEnumerable<FlowCondition> conditions, VisualElement parentElement)
        {
            foreach (FlowCondition condition in conditions)
            {
                VisualElement view = _viewFactory.CreateView(condition);
                parentElement.Add(view);

                if (condition.ConditionType == ConditionType.Composite && view is CompositeConditionView compositeView)
                {
                    var childConditions = _conditions.Where(c => c.parentGroupId == condition.uniqueId);
                    RenderConditions(childConditions, compositeView.ChildContainer);
                }
            }
        }

        private void OnConditionsReordered()
        {
            OnConditionsChanged?.Invoke(_conditions);
        }

        #endregion

    }
}
