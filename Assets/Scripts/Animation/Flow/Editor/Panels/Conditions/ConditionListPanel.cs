using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Conditions;
using Animation.Flow.Editor.Factories;
using Animation.Flow.Editor.Panels.Parameters;
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
            : base(parentContainer, "Conditions", new Vector2(320, 10))
        {
            // Initialize view factory and drag handler
            _viewFactory = new ConditionViewFactory(this);

            _dragHandler = new DragHandler<ConditionData>(Content, "ConditionData", "drag-handle");
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
            _dropIndicator.style.height = 4;
            _dropIndicator.style.backgroundColor =
                new StyleColor(new Color(0.3f, 0.7f, 1f, 0.8f)); // Make it more visible


            RegisterDropEvents();
        }

        #endregion

        #region Fields

        private readonly Label _titleLabel;
        private readonly ConditionViewFactory _viewFactory;
        private readonly DragHandler<ConditionData> _dragHandler;
        private List<ConditionData> _conditions;
        private readonly VisualElement _dropIndicator;
        public event Action<List<ConditionData>> OnConditionsChanged;

        #endregion

        #region Overrides

        protected override ScrollView CreateContent()
        {
            ScrollView scrollView = new();
            scrollView.AddToClassList("conditions-scroll-view");

            // Add name for CSS debugging
            scrollView.name = "ConditionScrollView";

            // Configure the scrollview to auto-resize based on content
            scrollView.style.flexGrow = 1;
            scrollView.style.width = new StyleLength(StyleKeyword.Auto);
            scrollView.contentContainer.style.flexGrow = 1;
            scrollView.contentContainer.style.width = new StyleLength(StyleKeyword.Auto);

            // Apply explicit background color programmatically
            scrollView.style.backgroundColor = new Color(0.176f, 0.176f, 0.176f, 1f);

            return scrollView;
        }

        protected override void OnContentCreated(ScrollView content)
        {
            ContentContainer.Add(content);

            ContentContainer.style.flexGrow = 1;
        }

        #endregion

        #region Public Methods

        public void Show(List<ConditionData> conditions, string title)
        {
            base.Show();
            _conditions = conditions;
            if (_titleLabel != null)
                _titleLabel.text = title;

            RefreshConditionsList();
        }

        public void PrepareForParameterDrop(ParameterData parameter)
        {
            // Only prepare for drop if the panel is visible
            if (style.display == DisplayStyle.None)
                return;

            // Make sure we're visible and in the DOM
            Show();

            // Store the parameter being dragged for later use in OnDragPerform
            DragAndDrop.SetGenericData("ParameterData", parameter);

            // Make sure the drop indicator is initialized
            if (_dropIndicator != null)
            {
                _dropIndicator.style.display = DisplayStyle.None;
            }
        }

        public void RemoveCondition(ConditionData condition)
        {
            if (condition.DataType == ConditionDataType.Composite)
            {
                var orphans = _conditions.Where(c => c.ParentGroupId == condition.UniqueId).ToList();
                foreach (ConditionData orphan in orphans)
                {
                    orphan.ParentGroupId = condition.ParentGroupId;
                    orphan.NestingLevel = Math.Max(0, condition.NestingLevel);
                }
            }

            _conditions.Remove(condition);
            OnConditionsChanged?.Invoke(_conditions);
            RefreshConditionsList();
        }

        public void UpdateCondition(ConditionData condition)
        {
            OnConditionsChanged?.Invoke(_conditions);
        }

        public void MoveConditionToComposite(ConditionData condition, ConditionData composite)
        {
            condition.ParentGroupId = composite.UniqueId;
            condition.NestingLevel = composite.NestingLevel + 1;
            OnConditionsChanged?.Invoke(_conditions);
            RefreshConditionsList();
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
            if (DragAndDrop.GetGenericData("ParameterData") is ParameterData ||
                DragAndDrop.GetGenericData("ConditionData") is ConditionData)
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
            if (DragAndDrop.GetGenericData("ParameterData") is ParameterData parameter)
            {
                ConditionData newCondition = CreateConditionFromParameter(parameter);
                int dropIndex = GetDropIndex(evt.localMousePosition);

                // Validate drop index to prevent out of range exceptions
                dropIndex = Mathf.Clamp(dropIndex, 0, _conditions.Count);

                _conditions.Insert(dropIndex, newCondition);
                OnConditionsChanged?.Invoke(_conditions);
                RefreshConditionsList();
            }
            // Handle condition drop to create a composite condition
            else if (DragAndDrop.GetGenericData("ConditionData") is ConditionData draggedCondition)
            {
                // Find the drop target condition - this is for dropping onto a specific item
                VisualElement targetElement = evt.target as VisualElement;
                while (targetElement is { userData: not ConditionData })
                {
                    targetElement = targetElement.parent;
                }

                if (targetElement?.userData is ConditionData targetCondition)
                {
                    // Don't allow dropping onto itself
                    if (targetCondition == draggedCondition)
                    {
                        evt.StopPropagation();
                        _dropIndicator.style.display = DisplayStyle.None;
                        return;
                    }

                    // Check if the target is already a composite - if so, just move the dragged condition inside it
                    if (targetCondition.DataType == ConditionDataType.Composite)
                    {
                        MoveConditionToComposite(draggedCondition, targetCondition);
                    }
                    else
                    {
                        // Create a new composite condition to hold both
                        ConditionData composite = CreateCompositeCondition();

                        // Inherit parent and nesting from the target condition
                        composite.ParentGroupId = targetCondition.ParentGroupId;
                        composite.NestingLevel = targetCondition.NestingLevel;

                        // Insert the composite in place of the target
                        int targetIndex = _conditions.IndexOf(targetCondition);
                        if (targetIndex >= 0)
                        {
                            _conditions.Insert(targetIndex, composite);

                            // Move both conditions into the composite
                            targetCondition.ParentGroupId = composite.UniqueId;
                            targetCondition.NestingLevel = composite.NestingLevel + 1;

                            if (_conditions.Remove(draggedCondition)) // If we're moving from the same list
                            {
                                draggedCondition.ParentGroupId = composite.UniqueId;
                                draggedCondition.NestingLevel = composite.NestingLevel + 1;

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

                    if (_conditions.Remove(draggedCondition)) // Only if it exists in our list already
                    {
                        // Reset parent group id when dropping into empty space
                        draggedCondition.ParentGroupId = string.Empty;
                        draggedCondition.NestingLevel = 0;

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

        private ConditionData CreateConditionFromParameter(ParameterData parameter) =>
            new()
            {
                UniqueId = Guid.NewGuid().ToString(),
                DataType = parameter.Type,
                ParameterName = parameter.Name,
                ComparisonType = GetDefaultComparisonType(parameter.Type)
            };

        private ComparisonType GetDefaultComparisonType(ConditionDataType type)
        {
            return type switch
            {
                ConditionDataType.Float => ComparisonType.GreaterThan,
                _ => ComparisonType.Equals
            };
        }

        /// <summary>
        ///     Creates a new composite condition (AND/OR group)
        /// </summary>
        private ConditionData CreateCompositeCondition() => new()
        {
            UniqueId = Guid.NewGuid().ToString(),
            DataType = ConditionDataType.Composite,
            StringValue = nameof(CompositeType.And)
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

            RenderConditions(_conditions.Where(c => string.IsNullOrEmpty(c.ParentGroupId)), Content);
        }

        private void RenderConditions(IEnumerable<ConditionData> conditions, VisualElement parentElement)
        {
            foreach (ConditionData condition in conditions)
            {
                VisualElement view = _viewFactory.CreateView(condition);
                parentElement.Add(view);

                if (condition.DataType == ConditionDataType.Composite && view is CompositeConditionView compositeView)
                {
                    var childConditions = _conditions.Where(c => c.ParentGroupId == condition.UniqueId);
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
