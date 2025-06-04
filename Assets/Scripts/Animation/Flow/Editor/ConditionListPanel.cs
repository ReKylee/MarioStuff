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
            : base(parentContainer, "Conditions", new Vector2(parentContainer.resolvedStyle.width - 320, 100))
        {
            _viewFactory = new ConditionViewFactory(this);

            _dropIndicator = new VisualElement();
            _dropIndicator.AddToClassList("drop-indicator");
            _dropIndicator.style.display = DisplayStyle.None;

            _dragHandler = new DragHandler<ConditionData>(_content, "ConditionData", "drag-handle");
            _dragHandler.OnItemsReordered += OnConditionsReordered;

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
            _contentContainer.Add(content);
        }

        #endregion

        #region Public Methods

        public void Show(List<ConditionData> conditions, string title)
        {
            base.Show();
            _conditions = conditions ?? new List<ConditionData>();
            if (_titleLabel != null)
                _titleLabel.text = title;

            RefreshConditionsList();
        }

        public void PrepareForParameterDrop(ParameterData parameter)
        {
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
            _content.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            _content.RegisterCallback<DragPerformEvent>(OnDragPerform);
            _content.RegisterCallback<DragLeaveEvent>(OnDragLeave);
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

            if (DragAndDrop.GetGenericData("ParameterData") is ParameterData parameter)
            {
                ConditionData newCondition = CreateConditionFromParameter(parameter);
                int dropIndex = GetDropIndex(evt.localMousePosition);

                _conditions.Insert(dropIndex, newCondition);
                OnConditionsChanged?.Invoke(_conditions);
                RefreshConditionsList();
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

            if (dropIndex >= _content.childCount)
                _content.Add(_dropIndicator);
            else
                _content.Insert(dropIndex, _dropIndicator);
        }

        private int GetDropIndex(Vector2 localPosition)
        {
            var children = _content.Children().ToList();
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

        private void RefreshConditionsList()
        {
            _content.Clear();

            if (_conditions == null || _conditions.Count == 0)
            {
                Label emptyLabel = new("No conditions. Drag parameters here.");
                emptyLabel.AddToClassList("empty-state-label");
                _content.Add(emptyLabel);
                return;
            }

            RenderConditions(_conditions.Where(c => string.IsNullOrEmpty(c.ParentGroupId)), _content);
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
