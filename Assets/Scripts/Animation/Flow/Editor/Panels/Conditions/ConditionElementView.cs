using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Editor.Factories;
using Animation.Flow.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels.Conditions
{
    /// <summary>
    ///     Visual element for a single condition
    /// </summary>
    public class ConditionElementView : VisualElement
    {

        #region UI Creation

        private void CreateUI()
        {
            // Main layout with drag handle and remove button at the ends
            style.justifyContent = Justify.SpaceBetween;

            // Drag handle
            Label dragHandle = new("≡");
            dragHandle.AddToClassList("drag-handle");
            Add(dragHandle);

            // Create a middle container to hold the parameter name, comparison button, and value field
            VisualElement middleContainer = new();
            middleContainer.AddToClassList("middle-container");
            Add(middleContainer);

            // Parameter name - show as label instead of editable field
            Label paramLabel = new(_condition.ParameterName);
            paramLabel.AddToClassList("parameter-name-label");
            middleContainer.Add(paramLabel);

            // Comparison type dropdown as a button that opens a menu
            Button comparisonButton = new();
            comparisonButton.text = ComparisonSymbols.GetSymbol(_condition.ComparisonType);
            comparisonButton.AddToClassList("comparison-button");
            comparisonButton.tooltip = _condition.ComparisonType.ToString();

            // Get available comparison types
            var availableCompTypes = _comparisonSelector.GetAvailableComparisonTypes();

            // If there's only one comparison type, make the button non-interactive
            if (availableCompTypes.Count <= 1)
            {
                comparisonButton.AddToClassList("non-interactive");
            }
            else
            {
                comparisonButton.clicked += () =>
                {
                    GenericMenu menu = new();
                    foreach (ComparisonType compType in availableCompTypes)
                    {
                        menu.AddItem(new GUIContent(ComparisonSymbols.GetDescription(compType)),
                            _condition.ComparisonType == compType,
                            () =>
                            {
                                _condition.ComparisonType = compType;
                                comparisonButton.text = ComparisonSymbols.GetSymbol(compType);
                                comparisonButton.tooltip = compType.ToString();
                                _panel.UpdateCondition(_condition);
                            });
                    }

                    menu.DropDown(comparisonButton.worldBound);
                };
            }

            middleContainer.Add(comparisonButton);

            // Value field based on parameter type
            VisualElement valueField = ValueEditorFactory.CreateEditor(_condition, condition =>
            {
                // When the value changes, update the condition in the panel
                _panel.UpdateCondition(condition);
            });

            middleContainer.Add(valueField);

            // Remove button
            Button removeButton = new(() => _panel.RemoveCondition(_condition)) { text = "×" };
            removeButton.AddToClassList("remove-button");
            Add(removeButton);
        }

        #endregion

        #region Constructor

        public ConditionElementView(FlowCondition condition, ConditionListPanel panel)
        {
            _condition = condition;
            _panel = panel;
            userData = condition;

            AddToClassList("condition-element");
            style.marginLeft = condition.nestingLevel * 20; // Keep this dynamic based on nesting level

            _comparisonSelector = new ComparisonTypeSelector(condition);
            CreateUI();

            // Make element droppable
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            // Ignore if clicking inside a field
            if (evt.target is TextField or Button) return;

            // Start drag only when clicking on drag handle or the condition itself
            if (evt.target is VisualElement target && (target == this || target.ClassListContains("drag-handle")))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("FlowCondition", _condition);
                DragAndDrop.StartDrag(_condition.ParameterName);
                evt.StopPropagation();
            }
        }

        #endregion

        #region Fields

        private readonly FlowCondition _condition;
        private readonly ConditionListPanel _panel;
        private readonly ComparisonTypeSelector _comparisonSelector;

        #endregion

    }
}
