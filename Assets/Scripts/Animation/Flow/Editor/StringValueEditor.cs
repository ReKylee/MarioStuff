using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class StringValueEditor : IValueEditor<string>
    {
        public VisualElement CreateEditor(string initialValue, Action<string> onValueChanged)
        {
            TextField field = new();
            field.value = initialValue;
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return field;
        }
    }
}
