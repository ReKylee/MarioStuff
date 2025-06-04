using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class IntegerValueEditor : IValueEditor<int>
    {
        public VisualElement CreateEditor(int initialValue, Action<int> onValueChanged)
        {
            IntegerField field = new();
            field.value = initialValue;
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return field;
        }
    }
}
