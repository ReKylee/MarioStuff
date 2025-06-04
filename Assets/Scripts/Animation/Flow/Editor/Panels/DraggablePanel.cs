using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor.Panels
{
    /// <summary>
    ///     Base class for draggable and resizable panels
    /// </summary>
    public abstract class DraggablePanel<TContent> : VisualElement where TContent : class
    {

        #region Constructor

        protected DraggablePanel(VisualElement parentContainer, string title, Vector2 defaultPosition)
        {
            ParentContainer = parentContainer;
            _position = defaultPosition;
            _size = _minSize;

            // Load the dedicated stylesheet for DraggablePanel
            StyleSheet draggablePanelStylesheet = Resources.Load<StyleSheet>("Stylesheets/DraggablePanel");


            if (draggablePanelStylesheet)
            {

                styleSheets.Add(draggablePanelStylesheet);
            }


            AddToClassList("draggable-panel");
            style.position = Position.Absolute;
            style.left = _position.x;
            style.top = _position.y;

            // Set initial size with minimum values
            style.minWidth = _minSize.x;
            style.minHeight = _minSize.y;


            // Ensure the panel stays within bounds of the parent container
            ParentContainer.RegisterCallback<GeometryChangedEvent>(_ => EnsureWithinBounds());

            CreateTitleBar(title);
            CreateContentArea();
            CreateResizeHandle();
            RegisterEvents();
        }

        /// <summary>
        ///     Ensures the panel stays within the bounds of the parent container
        /// </summary>
        protected void EnsureWithinBounds()
        {
            if (ParentContainer == null) return;

            Rect parentRect = ParentContainer.worldBound;
            Rect selfRect = worldBound;

            // Calculate available space
            float maxX = parentRect.width - selfRect.width;
            float maxY = parentRect.height - selfRect.height;

            // Adjust position if needed
            bool changed = false;

            if (_position.x < 0)
            {
                _position.x = 0;
                changed = true;
            }
            else if (_position.x > maxX)
            {
                _position.x = Mathf.Max(0, maxX);
                changed = true;
            }

            if (_position.y < 0)
            {
                _position.y = 0;
                changed = true;
            }
            else if (_position.y > maxY)
            {
                _position.y = Mathf.Max(0, maxY);
                changed = true;
            }

            // Apply changes if needed
            if (changed)
            {
                style.left = _position.x;
                style.top = _position.y;
            }
        }

        #endregion

        #region Fields

        protected readonly VisualElement ParentContainer;
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
            ContentContainer.style.flexDirection = FlexDirection.Column;
            ContentContainer.style.width = new StyleLength(StyleKeyword.Auto);

            // Ensure background color is applied directly
            ContentContainer.style.backgroundColor = new Color(0.176f, 0.176f, 0.176f, 1f); // #2D2D2D

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
            RegisterCallback<MouseDownEvent>(_ => _isInteracting = true);
            RegisterCallback<MouseUpEvent>(_ => _isInteracting = false);
            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!_isDragging && !_isResizing) _isInteracting = false;
            });

            ParentContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            ParentContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
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

                // Calculate parent bounds
                Rect parentBounds = ParentContainer.worldBound;

                // Make sure panel doesn't go outside the graph view's bounds
                newPosition.x = Mathf.Clamp(newPosition.x, 0, Mathf.Max(0, parentBounds.width - _size.x));
                newPosition.y = Mathf.Clamp(newPosition.y, 0, Mathf.Max(0, parentBounds.height - _size.y));

                _position = newPosition;
                style.left = _position.x;
                style.top = _position.y;
            }
            else if (_isResizing)
            {
                Vector2 delta = evt.mousePosition - _resizeStartPosition;
                Vector2 newSize = _resizeStartSize + delta;
                newSize = Vector2.Max(newSize, _minSize);

                // Make sure panel doesn't resize outside the graph view's bounds
                Rect parentBounds = ParentContainer.worldBound;
                float maxWidth = parentBounds.width - _position.x;
                float maxHeight = parentBounds.height - _position.y;
                newSize.x = Mathf.Min(newSize.x, maxWidth);
                newSize.y = Mathf.Min(newSize.y, maxHeight);

                _size = newSize;

                // For manual resizing, set explicit min-width/height instead of fixed width/height
                // This allows content to expand the panel if needed
                style.minWidth = _size.x;
                style.minHeight = _size.y;
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
