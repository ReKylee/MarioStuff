using System;
using UnityEditor;
using UnityEngine.UIElements;

public class DragHandler<T> where T : class
{

    #region Constructor

    public DragHandler(VisualElement container, string dataKey, string dragHandleClass)
    {
        _container = container;
        _dataKey = dataKey;
        _dragHandleClass = dragHandleClass;
        RegisterEvents();
    }

    #endregion

    #region Fields

    private readonly VisualElement _container;
    private readonly string _dataKey;
    private readonly string _dragHandleClass;
    private VisualElement _draggedElement;
    private T _draggedData;
    public event Action OnItemsReordered;

    #endregion

    #region Event Registration

    private void RegisterEvents()
    {
        _container.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        _container.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        _container.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button != 0) return;

        VisualElement target = evt.target as VisualElement;
        while (target != null && !target.ClassListContains(_dragHandleClass))
        {
            target = target.parent;
        }

        if (target?.parent?.userData is T data)
        {
            _draggedElement = target.parent;
            _draggedData = data;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(_dataKey, data);
            DragAndDrop.StartDrag(data.ToString());

            evt.StopPropagation();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (_draggedElement == null) return;
        _draggedElement.style.opacity = 0.6f;
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (_draggedElement != null)
        {
            _draggedElement.style.opacity = 1.0f;
            _draggedElement = null;
            _draggedData = default;

            OnItemsReordered?.Invoke();
        }
    }

    #endregion

}
