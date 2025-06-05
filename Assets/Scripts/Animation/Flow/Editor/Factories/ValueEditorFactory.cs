using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Editor.ValueEditors;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Factories
{
    /// <summary>
    ///     Factory for creating value editors
    /// </summary>
    public static class ValueEditorFactory
    {
        private static readonly Dictionary<Type, object> Editors = new()
        {
            { typeof(bool), new BooleanValueEditor() },
            { typeof(float), new FloatValueEditor() },
            { typeof(int), new IntegerValueEditor() },
            { typeof(string), new StringValueEditor() }
        };

        public static VisualElement CreateEditor<T>(T value, Action<T> onChanged)
        {
            if (Editors.TryGetValue(typeof(T), out object editor) && editor is IValueEditor<T> typedEditor)
                return typedEditor.CreateEditor(value, onChanged);

            return new Label($"No editor for {typeof(T).Name}");
        }

        /// <summary>
        ///     Creates an appropriate value editor for a condition
        /// </summary>
        /// <param name="condition">The condition data to create an editor for</param>
        /// <param name="onChanged">Callback when the condition is updated</param>
        /// <returns>A visual element containing the appropriate editor</returns>
        public static VisualElement CreateEditor(FlowCondition condition, Action<FlowCondition> onChanged)
        {
            // Use pattern matching to determine the condition type
            if (condition is BoolCondition)
            {
                return CreateEditor(condition.BoolValue, value =>
                {
                    condition.BoolValue = value;
                    onChanged?.Invoke(condition);
                });
            }

            if (condition is IntCondition)
            {
                return CreateEditor(condition.IntValue, value =>
                {
                    condition.IntValue = value;
                    onChanged?.Invoke(condition);
                });
            }

            if (condition is FloatCondition)
            {
                return CreateEditor(condition.FloatValue, value =>
                {
                    condition.FloatValue = value;
                    onChanged?.Invoke(condition);
                });
            }

            if (condition is StringCondition)
            {
                VisualElement editor = CreateEditor(condition.StringValue ?? string.Empty, value =>
                {
                    condition.StringValue = value;
                    onChanged?.Invoke(condition);
                });

                // For string operations that need ignore case option
                if (condition.ComparisonType is ComparisonType.Contains or ComparisonType.StartsWith
                    or ComparisonType.EndsWith)
                {
                    VisualElement container = new();
                    container.AddToClassList("string-value-container");
                    container.style.flexDirection = FlexDirection.Column;
                    container.Add(editor);

                    Toggle caseToggle = new("Ignore Case");
                    caseToggle.value = condition.BoolValue; // Reuse BoolValue for ignore case flag
                    caseToggle.RegisterValueChangedCallback(evt =>
                    {
                        condition.BoolValue = evt.newValue;
                        onChanged?.Invoke(condition);
                    });

                    container.Add(caseToggle);
                    return container;
                }

                return editor;
            }

            // Default fallback for unknown condition types
            return new Label("Value not editable");
        }

        /// <summary>
        ///     Creates a basic value editor based on the condition's parameter type
        /// </summary>
        /// <param name="condition">The condition to create an editor for</param>
        /// <returns>A visual element containing the appropriate editor</returns>
        public static VisualElement CreateBasicEditor(FlowCondition condition) =>
            CreateEditor(condition, null); // No callback for basic editor

        /// <summary>
        ///     Determines if a condition type supports advanced string operations
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <returns>True if the condition supports ignore case options</returns>
        public static bool SupportsIgnoreCase(FlowCondition condition) =>
            condition is StringCondition &&
            (condition.ComparisonType == ComparisonType.Contains ||
             condition.ComparisonType == ComparisonType.StartsWith ||
             condition.ComparisonType == ComparisonType.EndsWith);

        /// <summary>
        ///     Gets the appropriate default value for a condition type
        /// </summary>
        /// <param name="condition">The condition to get default value for</param>
        /// <returns>The default value as an object</returns>
        public static object GetDefaultValue(FlowCondition condition)
        {
            return condition switch
            {
                BoolCondition => false,
                IntCondition => 0,
                FloatCondition => 0.0f,
                StringCondition => string.Empty,
                _ => null
            };
        }
    }
}
