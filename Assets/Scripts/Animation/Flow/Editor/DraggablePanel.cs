using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Base class for draggable and resizable panels
    /// </summary>
    public abstract class DraggablePanel<TContent> : VisualElement where TContent : class
    {

        #region Constructor

        protected DraggablePanel(VisualElement parentContainer, string title, Vector2 defaultPosition)
        {
            _parentContainer = parentContainer;
            _position = defaultPosition;
            _size = _minSize;

            AddToClassList("draggable-panel");
            style.position = Position.Absolute;
            style.left = _position.x;
            style.top = _position.y;

            CreateTitleBar(title);
            CreateContentArea();
            CreateResizeHandle();
            RegisterEvents();
        }

        #endregion

        #region Fields

        private readonly VisualElement _parentContainer;
        protected VisualElement ContentContainer;
        protected TContent Content;
        private bool _isDragging;
        private bool _isResizing;
        private bool _isInteracting;
        private Vector2 _dragStartPosition;
        private Vector2 _resizeStartPosition;
        private Vector2 _resizeStartSize;
        private Vector2 _position;
        private Vector2 _size;
        private readonly Vector2 _minSize = new(200, 250);

        #endregion

        #region Abstract Methods

        protected abstract TContent CreateContent();
        protected abstract void OnContentCreated(TContent content);

        #endregion

        #region UI Creation

        private void CreateTitleBar(string title)
        {
            VisualElement titleBar = new();
            titleBar.AddToClassList("panel-title-bar");
            titleBar.RegisterCallback<MouseDownEvent>(OnTitleBarMouseDown);

            Label titleLabel = new(title);
            titleLabel.AddToClassList("panel-title-text");
            titleBar.Add(titleLabel);

            Add(titleBar);
        }

        private void CreateContentArea()
        {
            ContentContainer = new VisualElement();
            ContentContainer.AddToClassList("panel-content");
            ContentContainer.style.flexGrow = 1;
            Add(ContentContainer);

            Content = CreateContent();
            OnContentCreated(Content);
        }

        private void CreateResizeHandle()
        {
            VisualElement resizeHandle = new();
            resizeHandle.AddToClassList("panel-resize-handle");
            resizeHandle.RegisterCallback<MouseDownEvent>(OnResizeHandleMouseDown);
            Add(resizeHandle);
        }

        #endregion

        #region Event Handling

        private void RegisterEvents()
        {
            RegisterCallback<MouseDownEvent>(evt => _isInteracting = true);
            RegisterCallback<MouseUpEvent>(evt => _isInteracting = false);
            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!_isDragging && !_isResizing) _isInteracting = false;
            });

            _parentContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _parentContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnTitleBarMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isDragging = true;
                _dragStartPosition = evt.mousePosition - _position;
                evt.StopPropagation();
            }
        }

        private void OnResizeHandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isResizing = true;
                _resizeStartPosition = evt.mousePosition;
                _resizeStartSize = _size;
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                Vector2 newPosition = evt.mousePosition - _dragStartPosition;
                newPosition.x = Mathf.Clamp(newPosition.x, 0, _parentContainer.worldBound.width - _size.x);
                newPosition.y = Mathf.Clamp(newPosition.y, 0, _parentContainer.worldBound.height - _size.y);

                _position = newPosition;
                style.left = _position.x;
                style.top = _position.y;
            }
            else if (_isResizing)
            {
                Vector2 delta = evt.mousePosition - _resizeStartPosition;
                Vector2 newSize = _resizeStartSize + delta;
                newSize = Vector2.Max(newSize, _minSize);

                _size = newSize;
                style.width = _size.x;
                style.height = _size.y;
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _isDragging = false;
            _isResizing = false;
        }

        #endregion

        #region Public Methods

        public virtual void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            style.display = DisplayStyle.None;
            _isInteracting = false;
        }

        public bool IsBeingInteracted() => style.display == DisplayStyle.Flex && _isInteracting;

        #endregion

    }
}
