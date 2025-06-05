using System;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Core.Types;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels.Conditions
{

    /// <summary>
    ///     Visual element for a composite condition (AND/OR group)
    /// </summary>
    public class CompositeConditionView : VisualElement
    {

        #region Constructor

        public CompositeConditionView(FlowCondition condition, ConditionListPanel panel)
        {
            _condition = condition;
            _panel = panel;
            userData = condition;

            AddToClassList("condition-element");
            AddToClassList("composite-condition");
            style.marginLeft = condition.nestingLevel * 20;

            // Determine composite type (stored in StringValue)
            CompositeType compositeType = Enum.TryParse(_condition.StringValue, out CompositeType result)
                ? result
                : CompositeType.All;

            // Add appropriate class for styling
            AddToClassList(compositeType == CompositeType.All ? "composite-and" : "composite-or");

            CreateUI(compositeType);

            // Make element droppable
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        #endregion

        #region Event Handling

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            // Ignore if clicking inside a field or button
            if (evt.target is TextField || evt.target is Button) return;

            // Start drag only when clicking on drag handle or the composite header
            if (evt.target is VisualElement target && (target == this || target.ClassListContains("drag-handle")))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("FlowCondition", _condition);
                DragAndDrop.StartDrag("Composite Condition");
                evt.StopPropagation();
            }
        }

        #endregion

        #region Fields

        private readonly FlowCondition _condition;
        private readonly ConditionListPanel _panel;
        private Button _compositeTypeButton;
        private Button _notToggleButton;
        private bool _isNot;
        public VisualElement ChildContainer { get; private set; }

        #endregion

        #region UI Creation

        private void CreateUI(CompositeType compositeType)
        {
            // Create header section
            VisualElement headerSection = new();
            headerSection.AddToClassList("composite-header");
            Add(headerSection);

            // Create a container for header content (excluding the remove button)
            VisualElement headerContent = new();
            headerContent.AddToClassList("composite-header-content");
            headerSection.Add(headerContent);

            // Drag handle
            Label dragHandle = new("≡");
            dragHandle.AddToClassList("drag-handle");
            headerContent.Add(dragHandle);

            // Create NOT toggle button (before the composite type)
            _notToggleButton = new Button(ToggleNot)
            {
                text = "NOT"
            };

            // Add styling class (now shares base styles with composite-type-button in USS)
            _notToggleButton.AddToClassList("not-toggle");

            // Check if the condition has NOT prefix
            _isNot = _condition.BoolValue;
            if (_isNot)
            {
                AddToClassList("composite-not");
            }
            else
            {
                _notToggleButton.AddToClassList("disabled");
            }

            headerContent.Add(_notToggleButton);

            // Composite type button - clicking cycles through available types
            _compositeTypeButton = new Button(CycleCompositeType)
            {
                text = compositeType.ToString()
            };

            _compositeTypeButton.AddToClassList("composite-type-button");

            // Set a fixed width to make it shorter
            _compositeTypeButton.style.width = 70;
            _compositeTypeButton.style.minWidth = 70;

            // Add color class based on type
            _compositeTypeButton.EnableInClassList("and-type", compositeType == CompositeType.All);
            _compositeTypeButton.EnableInClassList("or-type", compositeType == CompositeType.Any);

            headerContent.Add(_compositeTypeButton);

            // Remove button
            Button removeButton = new(() => _panel.RemoveCondition(_condition)) { text = "×" };
            removeButton.AddToClassList("remove-button");
            headerSection.Add(removeButton);

            // Container for child conditions
            ChildContainer = new VisualElement();
            ChildContainer.AddToClassList("group-conditions-container");
            Add(ChildContainer);

            // Empty message for when container has no children
            Label emptyMessage = new("Drag conditions here");
            emptyMessage.AddToClassList("empty-group-message");
            ChildContainer.Add(emptyMessage);
        }

        private void CycleCompositeType()
        {
            // Parse current type from condition's StringValue
            CompositeType currentType = Enum.TryParse(_condition.StringValue, out CompositeType result)
                ? result
                : CompositeType.All;

            // Toggle between All and Any
            CompositeType newType = currentType == CompositeType.All ? CompositeType.Any : CompositeType.All;

            // Update condition
            _condition.StringValue = newType.ToString();
            _panel.UpdateCondition(_condition);

            // Update button text (consider the NOT state)
            _compositeTypeButton.text = newType.ToString();

            // Update CSS classes
            RemoveFromClassList("composite-and");
            RemoveFromClassList("composite-or");
            AddToClassList(newType == CompositeType.All ? "composite-and" : "composite-or");

            _compositeTypeButton.EnableInClassList("and-type", newType == CompositeType.All);
            _compositeTypeButton.EnableInClassList("or-type", newType == CompositeType.Any);
        }

        private void ToggleNot()
        {
            _isNot = !_isNot;

            // Store the NOT state in the BoolValue field of the condition
            _condition.BoolValue = _isNot;
            _panel.UpdateCondition(_condition);

            // Update visual appearance
            if (_isNot)
            {
                AddToClassList("composite-not");
                _notToggleButton.RemoveFromClassList("disabled");

                // Parse current type and update the composite type button text to show the NOT prefix
                CompositeType currentType = Enum.TryParse(_condition.StringValue, out CompositeType result)
                    ? result
                    : CompositeType.All;

                _compositeTypeButton.text = currentType.ToString();
            }
            else
            {
                RemoveFromClassList("composite-not");
                _notToggleButton.AddToClassList("disabled");

                // Update button text to show normal type (without NOT)
                CompositeType currentType = Enum.TryParse(_condition.StringValue, out CompositeType result)
                    ? result
                    : CompositeType.All;

                _compositeTypeButton.text = currentType.ToString();
            }
        }

        #endregion

    }
}
