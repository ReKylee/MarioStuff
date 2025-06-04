using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.ValueEditors
{
    public class BooleanValueEditor : IValueEditor<bool>
    {
        public VisualElement CreateEditor(bool initialValue, Action<bool> onValueChanged)
        {
            Toggle toggle = new();
            toggle.value = initialValue;
            toggle.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            return toggle;
        }
    }
}
