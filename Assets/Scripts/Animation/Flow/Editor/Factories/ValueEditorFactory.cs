using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
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

        public static VisualElement CreateEditor(ConditionData condition, Action onChanged)
        {
            return condition.DataType switch
            {
                ConditionDataType.Boolean => CreateEditor(condition.BoolValue, val =>
                {
                    condition.BoolValue = val;
                    onChanged();
                }),
                ConditionDataType.Float => CreateEditor(condition.FloatValue, val =>
                {
                    condition.FloatValue = val;
                    onChanged();
                }),
                ConditionDataType.Integer => CreateEditor(condition.IntValue, val =>
                {
                    condition.IntValue = val;
                    onChanged();
                }),
                ConditionDataType.String => CreateEditor(condition.StringValue, val =>
                {
                    condition.StringValue = val;
                    onChanged();
                }),
                _ => new Label("N/A")
            };
        }
    }
}
