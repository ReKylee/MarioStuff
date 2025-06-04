using System;
using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Visual element for a composite condition
    /// </summary>
    public class CompositeConditionView : VisualElement
    {

        #region Constructor

        public CompositeConditionView(ConditionData composite, ConditionListPanel panel)
        {
            _composite = composite;
            _panel = panel;
            userData = composite;

            AddToClassList("composite-condition");
            style.marginLeft = composite.NestingLevel * 20;

            CreateUI();
            UpdateStyle();
        }

        #endregion

        #region Fields

        private readonly ConditionData _composite;
        private readonly ConditionListPanel _panel;
        private Button _typeButton;
        public VisualElement ChildContainer { get; private set; }

        #endregion

        #region UI Creation

        private void CreateUI()
        {
            // Header
            VisualElement header = new();
            header.AddToClassList("composite-header");
            header.style.flexDirection = FlexDirection.Row;
            Add(header);

            // Drag handle
            Label dragHandle = new("≡");
            dragHandle.AddToClassList("drag-handle");
            header.Add(dragHandle);

            // Type button
            _typeButton = new Button(ToggleType);
            _typeButton.AddToClassList("composite-type-button");
            header.Add(_typeButton);

            // Remove button
            Button removeButton = new(() => _panel.RemoveCondition(_composite)) { text = "×" };
            removeButton.AddToClassList("remove-button");
            header.Add(removeButton);

            // Child container
            ChildContainer = new VisualElement();
            ChildContainer.AddToClassList("composite-children");
            Add(ChildContainer);

            // Register drop events
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void ToggleType()
        {
            CompositeType currentType = Enum.Parse<CompositeType>(_composite.StringValue);
            CompositeType newType = currentType == CompositeType.And ? CompositeType.Or : CompositeType.And;
            _composite.StringValue = newType.ToString();

            UpdateStyle();
            _panel.UpdateCondition(_composite);
        }

        private void UpdateStyle()
        {
            string type = _composite.StringValue ?? "And";
            _typeButton.text = type.ToUpper();

            RemoveFromClassList("composite-and");
            RemoveFromClassList("composite-or");
            AddToClassList($"composite-{type.ToLower()}");
        }

        #endregion

        #region Drop Handling

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (DragAndDrop.GetGenericData("ConditionData") is ConditionData condition)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                AddToClassList("drop-target");
                evt.StopPropagation();
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (DragAndDrop.GetGenericData("ConditionData") is ConditionData condition)
            {
                DragAndDrop.AcceptDrag();
                _panel.MoveConditionToComposite(condition, _composite);
                evt.StopPropagation();
            }

            RemoveFromClassList("drop-target");
        }

        #endregion

    }
}
