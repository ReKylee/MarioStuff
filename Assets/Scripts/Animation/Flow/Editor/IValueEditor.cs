using System;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Interface for value editors
    /// </summary>
    public interface IValueEditor<T>
    {
        VisualElement CreateEditor(T initialValue, Action<T> onValueChanged);
    }
}
