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
            _size = new Vector2(300, 400);

            // IMPORTANT: Set up the panel styling first
            AddToClassList("draggable-panel");
            style.position = Position.Absolute;
            style.left = _position.x;
            style.top = _position.y;
            style.width = _size.x;
            style.height = _size.y;

            // Ensure the panel has a background color
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.borderRightColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.borderTopColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;

            CreateTitleBar(title);
            CreateContentArea();
            CreateResizeHandle();
            RegisterEvents();
        }

        #endregion


        #region Fields

        protected readonly VisualElement _parentContainer;
        protected VisualElement _contentContainer;
        protected VisualElement _titleBar;
        protected Label _titleLabel;
        protected TContent _content;
        protected bool _isDragging;
        protected bool _isResizing;
        protected bool _isInteracting;
        protected Vector2 _dragStartPosition;
        protected Vector2 _resizeStartPosition;
        protected Vector2 _resizeStartSize;
        protected Vector2 _position;
        protected Vector2 _size;
        protected readonly Vector2 _minSize = new(200, 250);

        #endregion

        #region Abstract Methods

        protected abstract TContent CreateContent();
        protected abstract void OnContentCreated(TContent content);

        #endregion

        #region UI Creation

        private void CreateTitleBar(string title)
        {
            _titleBar = new VisualElement();
            _titleBar.AddToClassList("panel-title-bar");
            _titleBar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _titleBar.style.height = 30;
            _titleBar.style.flexDirection = FlexDirection.Row;
            _titleBar.style.justifyContent = Justify.SpaceBetween;
            _titleBar.style.alignItems = Align.Center;
            _titleBar.style.paddingLeft = 8;
            _titleBar.style.paddingRight = 8;

            _titleLabel = new Label(title);
            _titleLabel.AddToClassList("panel-title-text");
            _titleBar.Add(_titleLabel);

            Button closeButton = new(Hide) { text = "×" };
            closeButton.AddToClassList("panel-close-button");
            _titleBar.Add(closeButton);

            Add(_titleBar);
        }

        private void CreateContentArea()
        {
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("panel-content");
            _contentContainer.style.flexGrow = 1;
            Add(_contentContainer);

            _content = CreateContent();
            OnContentCreated(_content);
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
            // Track when we're interacting with the panel
            RegisterCallback<MouseDownEvent>(_ => _isInteracting = true);
            RegisterCallback<MouseUpEvent>(evt =>
            {
                _isInteracting = false;
                OnMouseUp(evt); // Make sure mouse up is always processed
            });

            // Handle mouse leave for interaction state
            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!_isDragging && !_isResizing)
                {
                    _isInteracting = false;
                }
            });

            // Register for mouse move events directly on this panel
            RegisterCallback<MouseMoveEvent>(OnMouseMove);

            // Also register on parent container for global capture
            _parentContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _parentContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);

            // Handle detachment from DOM
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (_parentContainer.HasMouseCapture())
                {
                    _parentContainer.ReleaseMouse();
                }

                _isDragging = false;
                _isResizing = false;
            });
        }

        private void OnTitleBarMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isDragging = true;

                // Store the mouse position in screen space
                _dragStartPosition = evt.mousePosition;

                // Store the current panel position
                _position = new Vector2(style.left.value.value, style.top.value.value);

                // Make this panel appear on top
                BringToFront();

                // Ensure we get all events
                this.CaptureMouse();
                evt.StopPropagation();

            }
        }

        private void OnResizeHandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isResizing = true;
                _resizeStartPosition = evt.mousePosition;
                _resizeStartSize = new Vector2(style.width.value.value, style.height.value.value);

                // Ensure we get all events
                this.CaptureMouse();
                evt.StopPropagation();

                // Make this panel appear on top
                BringToFront();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                // Calculate the delta mouse movement since drag started
                Vector2 delta = evt.mousePosition - _dragStartPosition;

                // Apply the delta to the original position
                Vector2 newPosition = new(
                    _position.x + delta.x,
                    _position.y + delta.y
                );

                // Clamp to container bounds
                float maxX = _parentContainer.layout.width - _size.x;
                float maxY = _parentContainer.layout.height - _size.y;
                newPosition.x = Mathf.Clamp(newPosition.x, 0, maxX > 0 ? maxX : 1000);
                newPosition.y = Mathf.Clamp(newPosition.y, 0, maxY > 0 ? maxY : 1000);

                // Update style properties directly
                style.left = newPosition.x;
                style.top = newPosition.y;

                // Store new position
                _position = newPosition;

                // Force layout update
                MarkDirtyRepaint();

                // Update drag start position for next delta calculation
                _dragStartPosition = evt.mousePosition;
            }
            else if (_isResizing)
            {
                Vector2 delta = evt.mousePosition - _resizeStartPosition;
                Vector2 newSize = _resizeStartSize + delta;
                newSize = Vector2.Max(newSize, _minSize);

                _size = newSize;
                style.width = _size.x;
                style.height = _size.y;

                // Force layout update
                MarkDirtyRepaint();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (_isDragging || _isResizing)
            {
                // End dragging/resizing state
                _isDragging = false;
                _isResizing = false;

                // Always release mouse capture
                if (this.HasMouseCapture())
                {
                    this.ReleaseMouse();
                }

                evt.StopPropagation();
            }
        }

        #endregion

        #region Public Methods

        public virtual void Show()
        {
            // Ensure proper visibility
            style.display = DisplayStyle.Flex;
            style.opacity = 1;

            // Update position values to handle any layout changes
            style.left = _position.x;
            style.top = _position.y;
            style.width = _size.x;
            style.height = _size.y;

            // Make sure it's the topmost panel
            BringToFront();

            // Force a repaint
            MarkDirtyRepaint();
        }

        public virtual void Hide()
        {
            style.display = DisplayStyle.None;
            _isInteracting = false;
        }

        public bool IsBeingInteracted() => style.display == DisplayStyle.Flex && _isInteracting;

        public void UpdateContainerBounds(float containerWidth, float containerHeight)
        {
            // Ensure panel stays within updated container bounds
            if (containerWidth > 0 && containerHeight > 0)
            {
                // If panel is now outside container, move it back in
                if (_position.x > containerWidth - _minSize.x)
                {
                    _position.x = Mathf.Max(0, containerWidth - _size.x);
                    style.left = _position.x;
                }

                if (_position.y > containerHeight - _minSize.y)
                {
                    _position.y = Mathf.Max(0, containerHeight - _size.y);
                    style.top = _position.y;
                }

                // Force layout update
                MarkDirtyRepaint();
            }
        }

        protected new void BringToFront()
        {
            if (_parentContainer != null)
            {
                _parentContainer.Remove(this);
                _parentContainer.Add(this);
            }
        }

        #endregion

    }
}
