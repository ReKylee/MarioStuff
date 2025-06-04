using Animation.Flow.Conditions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Visual element for a single condition
    /// </summary>
    public class ConditionElementView : VisualElement
    {

        #region UI Creation

        private void CreateUI()
        {
            // Drag handle
            Label dragHandle = new("≡");
            dragHandle.AddToClassList("drag-handle");
            Add(dragHandle);

            // Parameter name
            TextField paramField = new();
            paramField.AddToClassList("parameter-field");
            paramField.value = _condition.ParameterName;
            paramField.RegisterValueChangedCallback(evt =>
            {
                _condition.ParameterName = evt.newValue;
                _panel.UpdateCondition(_condition);
            });

            Add(paramField);

            // Comparison type dropdown
            var comparisonDropdown = _comparisonSelector.CreateDropdown(
                _condition.ComparisonType,
                newValue =>
                {
                    _condition.ComparisonType = newValue;
                    _panel.UpdateCondition(_condition);
                }
            );

            Add(comparisonDropdown);

            // Value field
            VisualElement valueField =
                ValueEditorFactory.CreateEditor(_condition, () => _panel.UpdateCondition(_condition));

            Add(valueField);

            // Remove button
            Button removeButton = new(() => _panel.RemoveCondition(_condition)) { text = "×" };
            removeButton.AddToClassList("remove-button");
            Add(removeButton);
        }

        #endregion

        #region Constructor

        public ConditionElementView(ConditionData condition, ConditionListPanel panel)
        {
            _condition = condition;
            _panel = panel;
            userData = condition;

            AddToClassList("condition-element");
            style.flexDirection = FlexDirection.Row;
            style.marginLeft = condition.NestingLevel * 20;

            _comparisonSelector = new ComparisonTypeSelector(condition.DataType);
            CreateUI();

            // Make element droppable
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            // Ignore if clicking inside a field
            if (evt.target is TextField || evt.target is Button) return;

            // Start drag only when clicking on drag handle or the condition itself
            if (evt.target is VisualElement target && (target == this || target.ClassListContains("drag-handle")))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("ConditionData", _condition);
                DragAndDrop.StartDrag(_condition.ParameterName);
                evt.StopPropagation();
            }
        }

        #endregion

        #region Fields

        private readonly ConditionData _condition;
        private readonly ConditionListPanel _panel;
        private readonly ComparisonTypeSelector _comparisonSelector;

        #endregion

    }
}
