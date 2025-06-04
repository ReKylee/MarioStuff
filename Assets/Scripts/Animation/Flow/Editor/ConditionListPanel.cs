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

            // Create drop indicator element
            _dropIndicator = new VisualElement();
            _dropIndicator.AddToClassList("drop-indicator");


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
            return scrollView;
        }

        protected override void OnContentCreated(ScrollView content)
        {
            ContentContainer.Add(content);
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

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            UpdateDropIndicator(evt.localMousePosition);
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

                _conditions.Insert(dropIndex, newCondition);
                OnConditionsChanged?.Invoke(_conditions);
                RefreshConditionsList();
            }
            // Handle condition drop to create a composite condition
            else if (DragAndDrop.GetGenericData("ConditionData") is ConditionData draggedCondition)
            {
                // Find the drop target condition - this is for dropping onto a specific item
                VisualElement targetElement = evt.target as VisualElement;
                while (targetElement != null && targetElement.userData is not ConditionData)
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
                                _conditions.Insert(targetIndex + 1, draggedCondition);
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
            _dropIndicator.style.display = DisplayStyle.None;
        }

        private void UpdateDropIndicator(Vector2 localPosition)
        {
            int dropIndex = GetDropIndex(localPosition);
            _dropIndicator.style.display = DisplayStyle.Flex;

            if (dropIndex >= Content.childCount)
                Content.Add(_dropIndicator);
            else
                Content.Insert(dropIndex, _dropIndicator);
        }

        private int GetDropIndex(Vector2 localPosition)
        {
            var children = Content.Children().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == _dropIndicator) continue;

                Rect bounds = children[i].worldBound;
                if (localPosition.y < bounds.center.y)
                    return i;
            }

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
                ConditionDataType.Boolean => ComparisonType.IsTrue,
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
            StringValue = CompositeType.And.ToString() // Default to AND logic
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
