using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class FloatValueEditor : IValueEditor<float>
    {
        public VisualElement CreateEditor(float initialValue, Action<float> onValueChanged)
        {
            FloatField field = new();
            field.value = initialValue;
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return field;
        }
    }
}
