using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Conditions.ParameterConditions;
using Animation.Flow.Editor.ValueEditors;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
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
        /// Creates an appropriate value editor for a condition
        /// </summary>
        /// <param name="condition">The condition data to create an editor for</param>
        /// <param name="onChanged">Callback when the condition is updated</param>
        /// <returns>A visual element containing the appropriate editor</returns>
        public static VisualElement CreateEditor(ConditionData condition, Action<ConditionData> onChanged)
        {
            switch (condition.ParameterValueType)
            {
                case ParameterValueType.Bool:
                    return CreateEditor(condition.BoolValue, value => {
                        condition.BoolValue = value;
                        onChanged?.Invoke(condition);
                    });

                case ParameterValueType.Int:
                    return CreateEditor(condition.IntValue, value => {
                        condition.IntValue = value;
                        onChanged?.Invoke(condition);
                    });

                case ParameterValueType.Float:
                    return CreateEditor(condition.FloatValue, value => {
                        condition.FloatValue = value;
                        onChanged?.Invoke(condition);
                    });

                case ParameterValueType.String:
                    var editor = CreateEditor(condition.StringValue ?? string.Empty, value => {
                        condition.StringValue = value;
                        onChanged?.Invoke(condition);
                    });

                    // For string operations that need ignore case option
                    if (condition.ComparisonType == ComparisonType.Contains || 
                        condition.ComparisonType == ComparisonType.StartsWith || 
                        condition.ComparisonType == ComparisonType.EndsWith)
                    {
                        var container = new VisualElement();
                        container.AddToClassList("string-value-container");
                        container.Add(editor);

                        var caseToggle = new Toggle("Ignore Case");
                        caseToggle.value = condition.BoolValue;
                        caseToggle.RegisterValueChangedCallback(evt => {
                            condition.BoolValue = evt.newValue;
                            onChanged?.Invoke(condition);
                        });

                        container.Add(caseToggle);
                        return container;
                    }

                    return editor;

                default:
                    return new Label("Value not editable");
            }
        }
    }
}
