using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.ValueEditors
{
    /// <summary>
    ///     Interface for value editors
    /// </summary>
    public interface IValueEditor<T>
    {
        VisualElement CreateEditor(T initialValue, Action<T> onValueChanged);
    }
}
