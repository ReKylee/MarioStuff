using Animation.Flow.Conditions;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Visual element for a single condition
    /// </summary>
    public class ConditionElementView : VisualElement
    {

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
        }

        #endregion

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

        #region Fields

        private readonly ConditionData _condition;
        private readonly ConditionListPanel _panel;
        private readonly ComparisonTypeSelector _comparisonSelector;

        #endregion

    }
}
