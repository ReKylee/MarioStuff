using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Interfaces;
using Animation.Flow.States;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class AnimationStateNode : Node
    {
        private PopupField<string> _animationDropdown;
        private VisualElement _contentContainer;
        private IntegerField _frameToHoldField;
        private Toggle _initialStateToggle;
        private bool _isCollapsed;

        public AnimationStateNode(FlowStateType stateType, string animationName)
        {
            StateType = stateType;
            AnimationName = animationName;
            ID = Guid.NewGuid().ToString();

            // Set title to animation name
            title = animationName;

            // Build node UI
            BuildNodeUI();
        }
        public int FrameToHold => _frameToHoldField?.value ?? 0;
        public FlowStateType StateType { get; }
        public string AnimationName { get; set; }
        public bool IsInitialState { get; set; }
        public string ID { get; set; }

        /// <summary>
        ///     Refresh the animation dropdown with a new list of animations
        /// </summary>
        public void RefreshAnimationList(List<string> availableAnimations)
        {
            if (_animationDropdown == null || availableAnimations == null)
                return;

            // Save current selection
            string currentSelection = _animationDropdown.value;

            // Update dropdown choices
            _animationDropdown.choices = availableAnimations;

            // Try to restore selection
            if (availableAnimations.Contains(currentSelection))
            {
                _animationDropdown.value = currentSelection;
            }
            else if (availableAnimations.Count > 0)
            {
                _animationDropdown.value = availableAnimations[0];
                AnimationName = availableAnimations[0];
                title = AnimationName;
            }
        }

        public void RefreshInitialStateToggle()
        {
            if (_initialStateToggle is not null)
                _initialStateToggle.value = IsInitialState;

            // Update visual appearance
            titleContainer.style.backgroundColor = GetStateColor();
        }


        private void BuildNodeUI()
        {
            // Apply custom styles for better appearance
            AddToClassList("animation-state-node");

            // Remove any extra foldout sections that might be created by default
            extensionContainer.style.display = DisplayStyle.None;

            // Configure main container for better layout
            mainContainer.style.borderTopLeftRadius = 5;
            mainContainer.style.borderTopRightRadius = 5;
            mainContainer.style.borderBottomLeftRadius = 5;
            mainContainer.style.borderBottomRightRadius = 5;
            mainContainer.style.overflow = Overflow.Hidden;

            // Style the title container - clean and prominent
            titleContainer.style.backgroundColor = GetStateColor();
            titleContainer.style.paddingLeft = 12;
            titleContainer.style.paddingRight = 30; // Leave space for collapse indicator
            titleContainer.style.paddingTop = 8;
            titleContainer.style.paddingBottom = 8;
            titleContainer.style.height = 30;

            // Make title text more readable
            Label titleLabel = titleContainer.Q<Label>();
            if (titleLabel is not null)
            {
                titleLabel.style.fontSize = 14;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.color = Color.white;
            }

            // Make title container clickable for collapse/expand
            titleContainer.RegisterCallback<ClickEvent>(evt =>
            {
                // Don't trigger if clicking on child elements
                if (evt.target == titleContainer || evt.target == titleLabel)
                    ToggleCollapsed();
            });

            // Add visual indicator for collapsible state
            Button collapseIndicator = new(ToggleCollapsed) { text = "▼" };
            collapseIndicator.style.width = 20;
            collapseIndicator.style.height = 20;
            collapseIndicator.style.position = Position.Absolute;
            collapseIndicator.style.right = 5;
            collapseIndicator.style.top = 5;
            collapseIndicator.style.backgroundColor = new Color(0, 0, 0, 0);
            collapseIndicator.style.borderBottomWidth = 0;
            collapseIndicator.style.borderTopWidth = 0;
            collapseIndicator.style.borderLeftWidth = 0;
            collapseIndicator.style.borderRightWidth = 0;
            titleContainer.Add(collapseIndicator);

            // Container for all content below the title
            _contentContainer = new VisualElement();
            _contentContainer.style.display = DisplayStyle.Flex;
            _contentContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            _contentContainer.style.paddingTop = 6;
            _contentContainer.style.paddingBottom = 6;
            mainContainer.Add(_contentContainer);

            // Disable any default elements that might create empty space
            if (titleButtonContainer != null)
                titleButtonContainer.style.display = DisplayStyle.None;

            if (topContainer != null)
                topContainer.style.minHeight = 0;

            // Create animation name field
            VisualElement nameContainer = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginLeft = 8,
                    marginRight = 8,
                    marginTop = 4
                }
            };

            Label nameLabel = new("Animation:")
            {
                style =
                {
                    minWidth = 70,
                    color = new Color(0.9f, 0.9f, 0.9f)
                }
            };

            nameContainer.Add(nameLabel);

            // Get animation names from the parent graph view if possible
            List<string> availableAnimations;
            if (parent is AnimationFlowGraphView graphView)
            {
                availableAnimations = graphView.GetAvailableAnimations();
            }
            else
            {
                // Fallback to selection-based animations if not in a graph view yet
                availableAnimations = AnimationNameProvider.GetAnimationNamesFromSelection();
            }

            // Create a dropdown for animation selection
            _animationDropdown = new PopupField<string>();
            _animationDropdown.formatListItemCallback = animName => animName;
            _animationDropdown.formatSelectedValueCallback = animName => animName;

            // Populate the dropdown with animation names
            _animationDropdown.choices = availableAnimations;

            // Set the current value if it exists in the list, otherwise use the first item
            if (availableAnimations.Contains(AnimationName))
            {
                _animationDropdown.value = AnimationName;
            }
            else if (availableAnimations.Count > 0)
            {
                _animationDropdown.value = availableAnimations[0];
                AnimationName = availableAnimations[0];
                title = AnimationName;
            }

            // Register callback for when selection changes
            _animationDropdown.RegisterValueChangedCallback(evt =>
            {
                AnimationName = evt.newValue;
                title = evt.newValue; // Update node title when animation name changes
            });

            _animationDropdown.style.flexGrow = 1;
            nameContainer.Add(_animationDropdown);

            // Add refresh button next to dropdown
            Button refreshButton = new(() =>
            {
                if (parent is not AnimationFlowGraphView)
                    return;

                // Get animator from editor window if possible
                AnimationFlowEditorWindow editorWindow = EditorWindow.GetWindow<AnimationFlowEditorWindow>();
                IAnimator animator = editorWindow?.GetTargetAnimator();

                // Update animation list based on the animator
                List<string> updatedAnimations;
                updatedAnimations = animator is not null
                    ? AnimationNameProvider.GetAnimationNames(animator)
                    : AnimationNameProvider.GetAnimationNamesFromSelection();

                RefreshAnimationList(updatedAnimations);
            })
            {
                text = "⟳"
            };

            refreshButton.style.width = 24;
            refreshButton.tooltip = "Refresh animation list";
            nameContainer.Add(refreshButton);

            _contentContainer.Add(nameContainer);

            // Add a label for state type (not editable, just informational)
            VisualElement typeContainer = new();
            typeContainer.style.flexDirection = FlexDirection.Row;
            typeContainer.style.marginLeft = 8;
            typeContainer.style.marginRight = 8;
            typeContainer.style.marginTop = 4;
            typeContainer.style.marginBottom = 4;

            Label typeLabel = new("Type:");
            typeLabel.style.minWidth = 70;
            typeLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            typeContainer.Add(typeLabel);

            Label typeValueLabel = new(StateType.ToString());
            typeValueLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            typeContainer.Add(typeValueLabel);

            _contentContainer.Add(typeContainer);

            // Create initial state toggle
            VisualElement toggleContainer = new();
            toggleContainer.style.marginLeft = 8;
            toggleContainer.style.marginRight = 8;
            toggleContainer.style.marginTop = 8;

            _initialStateToggle = new Toggle("Initial State");
            _initialStateToggle.value = IsInitialState;
            _initialStateToggle.RegisterValueChangedCallback(evt =>
            {
                bool wasInitial = IsInitialState;
                IsInitialState = evt.newValue;

                // Update node visual style
                titleContainer.style.backgroundColor = GetStateColor();

                // Only enforce single initial state if we're turning this ON
                if (!wasInitial && IsInitialState)
                {
                    // Clear other initial states
                    if (parent is GraphView view)
                    {
                        var nodes = view.nodes.ToList().Cast<AnimationStateNode>();
                        foreach (AnimationStateNode node in nodes)
                        {
                            if (node != this && node._initialStateToggle != null)
                            {
                                node.IsInitialState = false;
                                node._initialStateToggle.value = false;
                                node.titleContainer.style.backgroundColor = node.GetStateColor();
                            }
                        }
                    }
                }
            });

            toggleContainer.Add(_initialStateToggle);
            _contentContainer.Add(toggleContainer);


            // Add some padding at the bottom for other node types
            VisualElement spacer = new();
            spacer.style.height = 8;
            _contentContainer.Add(spacer);

            // Create and style ports differently, using arrow-like visuals
            StylePortContainers();

            // Initial refresh
            RefreshExpandedState();
        }

        private void StylePortContainers()
        {
            // Use standard default port container styling without absolute positioning
            // This will allow Unity's built-in layout system to place ports properly
            inputContainer.style.backgroundColor = new Color(0, 0, 0, 0);
            outputContainer.style.backgroundColor = new Color(0, 0, 0, 0);
        }

        private void ToggleCollapsed()
        {
            _isCollapsed = !_isCollapsed;

            // Update visual indicator
            Button collapseButton = titleContainer.Q<Button>();
            if (collapseButton != null)
                collapseButton.text = _isCollapsed ? "▶" : "▼";

            // Show/hide content
            _contentContainer.style.display = _isCollapsed ? DisplayStyle.None : DisplayStyle.Flex;

            // Force the node to update its size
            RefreshExpandedState();
        }

        // Get a color based on the state type and whether it's initial
        private Color GetStateColor()
        {
            if (IsInitialState)
                return new Color(0.2f, 0.6f, 0.2f); // Green for initial state

            switch (StateType)
            {
                case FlowStateType.HoldFrame:
                    return new Color(0.2f, 0.2f, 0.6f); // Blue for hold frame
                case FlowStateType.OneTime:
                    return new Color(0.6f, 0.3f, 0.1f); // Orange for one-time
                case FlowStateType.Looping:
                    return new Color(0.4f, 0.1f, 0.5f); // Purple for looping
                default:
                    return new Color(0.3f, 0.3f, 0.3f); // Gray for unknown
            }
        }

        /// <summary>
        ///     Mark this node as having an invalid animation
        /// </summary>
        /// <param name="missingAnimationName">Name of the missing animation</param>
        public void MarkAsInvalid(string missingAnimationName)
        {
            // Add a warning class to the node
            AddToClassList("animation-node-warning");

            // Change the border color to orange to indicate a warning
            mainContainer.style.borderLeftColor = new Color(1f, 0.6f, 0f);
            mainContainer.style.borderRightColor = new Color(1f, 0.6f, 0f);
            mainContainer.style.borderTopColor = new Color(1f, 0.6f, 0f);
            mainContainer.style.borderBottomColor = new Color(1f, 0.6f, 0f);
            mainContainer.style.borderLeftWidth = 2;
            mainContainer.style.borderRightWidth = 2;
            mainContainer.style.borderTopWidth = 2;
            mainContainer.style.borderBottomWidth = 2;

            // Add warning text to the title
            title = $"{AnimationName} ⚠️";

            // Add tooltip
            tooltip = $"Warning: Animation '{missingAnimationName}' does not exist in the current controller.";
        }

        /// <summary>
        ///     Clear the invalid state visual indicators
        /// </summary>
        public void ClearInvalidState()
        {
            // Remove warning class
            RemoveFromClassList("animation-node-warning");

            // Reset border
            mainContainer.style.borderLeftWidth = 1;
            mainContainer.style.borderRightWidth = 1;
            mainContainer.style.borderTopWidth = 1;
            mainContainer.style.borderBottomWidth = 1;
            mainContainer.style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            mainContainer.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            mainContainer.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            mainContainer.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Reset title
            title = AnimationName;

            // Clear tooltip
            tooltip = "";
        }
    }
}
