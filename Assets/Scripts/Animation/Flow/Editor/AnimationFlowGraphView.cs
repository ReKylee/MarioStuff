using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Conditions;
using Animation.Flow.Core;
using Animation.Flow.Editor.Managers;
using Animation.Flow.Editor.Panels;
using Animation.Flow.Interfaces;
using Animation.Flow.States;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class AnimationFlowGraphView : GraphView
    {

        // Transition editor panel (embedded in the graph view)
        private readonly TransitionEditorPanel _transitionEditorPanel;

        // Store the available animation names
        private List<string> _availableAnimations;

        // Clipboard data for copy/paste operations
        private List<ISelectable> _copiedElements = new();

        // Track if we're currently handling a selection change to prevent recursion
        private bool _isHandlingSelection;
        private IAnimator _targetAnimator;

        // Store references to the target GameObject and animator
        private GameObject _targetGameObject;

        public AnimationFlowGraphView()
        {
            // Set up basic behavior
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Add background grid
            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();

            // Register for graph changes
            graphViewChanged += OnGraphViewChanged;

            // Add style sheet
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/Animation/Flow/Editor/AnimationFlowEditor.uss");

            if (styleSheet is not null)
                styleSheets.Add(styleSheet);

            // Add context menu style sheet
            StyleSheet contextMenuStyleSheet = Resources.Load<StyleSheet>("Stylesheets/ContextMenuStyles");

            if (contextMenuStyleSheet is not null)
                styleSheets.Add(contextMenuStyleSheet);


            _transitionEditorPanel = new TransitionEditorPanel(this);

            // Register keyboard shortcuts
            RegisterKeyboardShortcuts();


            // Initialize with default animations
            _availableAnimations = new List<string>
            {
                "Idle", "Walk", "Run", "Jump", "Fall"
            };

            Debug.Log("[AnimationFlowGraphView] Initialized with default animations");
        }

        /// <summary>
        ///     Get current available animations list
        /// </summary>
        public List<string> GetAvailableAnimations() => new(_availableAnimations);

        /// <summary>
        ///     Called when the target GameObject changes - this updates animation lists
        /// </summary>
        public void OnTargetGameObjectChanged(GameObject targetGameObject, IAnimator targetAnimator)
        {
            _targetGameObject = targetGameObject;
            _targetAnimator = targetAnimator;

            Debug.Log(
                $"[AnimationFlowGraphView] Target changed to {(_targetGameObject ? _targetGameObject.name : "null")}");

            // Update available animations from the target
            if (_targetAnimator != null)
            {
                // Get animations from the target animator
                _availableAnimations = _targetAnimator.GetAvailableAnimations();

                if (_availableAnimations != null)
                {
                    Debug.Log($"[AnimationFlowGraphView] Got {_availableAnimations.Count} animations from animator");
                }
                else
                {
                    Debug.LogWarning("[AnimationFlowGraphView] Animator.GetAvailableAnimations() returned null");
                    _availableAnimations = new List<string>();
                }
            }
            else
            {
                // Fall back to default animations
                _availableAnimations = AnimationNameProvider.GetAnimationNamesFromSelection();
                Debug.Log($"[AnimationFlowGraphView] Using {_availableAnimations.Count} fallback animations");
            }

            // Refresh animation dropdowns in existing nodes and validate compatibility
            RefreshNodeAnimationLists();
        }

        /// <summary>
        ///     Refresh animation lists in all existing nodes and validate compatibility
        /// </summary>
        private void RefreshNodeAnimationLists()
        {
            foreach (Node node in nodes.ToList())
            {
                if (node is AnimationStateNode animNode)
                {
                    // Check if the animation exists in the current adapter
                    bool isAnimationValid = _availableAnimations.Contains(animNode.AnimationName);

                    // Update the animation list
                    animNode.RefreshAnimationList(_availableAnimations);

                    // Mark the node as invalid if its animation doesn't exist
                    if (!isAnimationValid)
                    {
                        animNode.MarkAsInvalid(animNode.AnimationName);
                    }
                    else
                    {
                        animNode.ClearInvalidState();
                    }
                }
            }
        }

        /// <summary>
        ///     Create a new animation state node
        /// </summary>
        public AnimationStateNode CreateStateNode(AnimationStateType stateType, string animationName, Rect position,
            string customId = null, bool isInitialState = false)
        {
            // Create a new animation state node
            AnimationStateNode node = new(stateType, animationName);

            // Set custom ID if provided
            if (!string.IsNullOrEmpty(customId))
            {
                node.ID = customId;
            }

            // Set initial state
            if (isInitialState)
            {
                // Clear initial state from other nodes
                foreach (Node existingNode in nodes.ToList())
                {
                    if (existingNode is AnimationStateNode { IsInitialState: true } existingAnimNode)
                    {
                        existingAnimNode.IsInitialState = false;
                        existingAnimNode.RefreshInitialStateToggle();
                    }
                }

                // Set this node as initial
                node.IsInitialState = true;
                node.RefreshInitialStateToggle();
            }


            // Set position
            node.SetPosition(position);

            // Create standard input port using the default Edge type
            Port inputPort = Port.Create<AnimationFlowEdge>(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(bool));

            inputPort.portName = "In";
            node.inputContainer.Add(inputPort);

            // Create standard output port using the default Edge type
            Port outputPort = Port.Create<AnimationFlowEdge>(Orientation.Horizontal, Direction.Output,
                Port.Capacity.Multi, typeof(bool));

            outputPort.portName = "Out";
            node.outputContainer.Add(outputPort);

            // Add node to graph
            AddElement(node);

            // Force port refresh again after adding output port
            node.RefreshExpandedState();
            node.RefreshPorts();


            // Refresh animation list
            node.RefreshAnimationList(_availableAnimations);

            // Validate animation compatibility, but only if the animation name doesn't exist 
            // AND we have animations available (otherwise everything would be marked invalid)
            if (_availableAnimations.Count > 0 && !_availableAnimations.Contains(animationName))
            {
                node.MarkAsInvalid(animationName);
            }
            else
            {
                // Make sure it starts in a valid state
                node.ClearInvalidState();
            }


            return node;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) =>
            ports.Where(port =>
                    startPort != port &&
                    startPort.direction != port.direction &&
                    startPort.node != port.node)
                .ToList();


        /// <summary>
        ///     Clear all elements from the graph
        /// </summary>
        public void ClearGraph()
        {
            // Clear all edges
            edges.ForEach(edge => RemoveElement(edge));

            // Clear all nodes
            nodes.ForEach(node => RemoveElement(node));

            // Clear selection
            ClearSelection();
        }

        /// <summary>
        ///     Frame the view to show all content
        /// </summary>
        public new void FrameAll()
        {
            if (nodes.ToList().Count > 0)
            {
                // The base FrameAll method doesn't take parameters, so call it without arguments
                base.FrameAll();
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Handle edge creation
            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    // Get the edge ID from our condition manager
                    string edgeId = EdgeConditionManager.GetEdgeId(edge);

                    // Initialize with empty conditions list
                    if (!string.IsNullOrEmpty(edgeId))
                    {
                        EdgeConditionManager.Instance.SetConditions(edgeId, new List<ConditionData>());
                    }
                }
            }

            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent menuEvent)
        {
            if (menuEvent?.menu == null) return;


            // Convert screen position to graph position
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(menuEvent.mousePosition);

            // Cache selection for performance
            var selectedElements = selection?.ToList() ?? new List<ISelectable>();
            var selectedNodes = selectedElements.OfType<AnimationStateNode>().ToList();
            var selectedGraphElements = selectedElements.OfType<GraphElement>().ToList();
            bool hasSelection = selectedElements.Count > 0;
            bool hasCopiedElements = _copiedElements?.Count > 0;

            // Add "Create Animation State" as a submenu with all node types
            try
            {
                var stateTypes = StateTypeRegistry.GetRegisteredStateTypes();
                if (stateTypes != null)
                {
                    foreach (AnimationStateType nodeType in stateTypes)
                    {
                        menuEvent.menu.AppendAction($"✨ Create Animation State/{nodeType}",
                            _ => CreateStateNode(nodeType, "NewAnimation",
                                new Rect(localMousePosition, new Vector2(150, 200))));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load state types: {ex.Message}");
            }

            // Add separator between create and edit actions
            menuEvent.menu.AppendSeparator();

            // EDIT SECTION
            menuEvent.menu.AppendAction("✂️ Cut", _ =>
                {
                    if (selectedNodes.Count > 0)
                    {
                        _copiedElements = new List<ISelectable>(selectedNodes);
                        DeleteElements(selectedGraphElements);
                    }
                },
                hasSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            menuEvent.menu.AppendAction("📝 Copy", _ =>
                {
                    if (selectedNodes.Count > 0)
                    {
                        _copiedElements = new List<ISelectable>(selectedNodes);
                    }
                },
                hasSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            menuEvent.menu.AppendAction("📋 Paste", _ => PasteElements(),
                hasCopiedElements ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            menuEvent.menu.AppendAction("🗑️ Delete", _ =>
                {
                    if (selectedGraphElements.Count > 0)
                    {
                        DeleteElements(selectedGraphElements);
                    }
                },
                hasSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            // Add separator before duplicate
            menuEvent.menu.AppendSeparator();

            // Duplicate option
            menuEvent.menu.AppendAction("🔄 Duplicate", _ =>
                {
                    if (selectedNodes.Count > 0)
                    {
                        _copiedElements = new List<ISelectable>(selectedNodes);
                        PasteElements(20);
                    }
                },
                hasSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            // Add separator before view options
            menuEvent.menu.AppendSeparator();

            // VIEW SECTION
            menuEvent.menu.AppendAction("🔍 Frame Selection", _ => FrameSelection(),
                hasSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            menuEvent.menu.AppendAction("🔍 Frame All", _ => FrameAll());

            // Add separator before select all
            menuEvent.menu.AppendSeparator();

            // SELECT SECTION
            menuEvent.menu.AppendAction("📌 Select All Nodes", _ =>
            {
                if (nodes == null)
                    return;

                foreach (Node node in nodes)
                {
                    if (node != null)
                    {
                        AddToSelection(node);
                    }
                }
            });
        }
        private void RegisterKeyboardShortcuts()
        {
            // Delete - Remove selected elements
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace)
                {
                    if (selection.Count > 0)
                    {
                        // Convert ISelectable to GraphElement to match DeleteElements signature
                        var elementsToDelete = selection.OfType<GraphElement>().ToList();
                        DeleteElements(elementsToDelete);
                        evt.StopPropagation();
                    }
                }
            });

            // Ctrl+C - Copy selected elements
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.ctrlKey && evt.keyCode == KeyCode.C)
                {
                    _copiedElements = selection.Where(e => e is AnimationStateNode).ToList();
                    evt.StopPropagation();
                }
            });

            // Ctrl+V - Paste copied elements
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.ctrlKey && evt.keyCode == KeyCode.V)
                {
                    PasteElements();
                    evt.StopPropagation();
                }
            });

            // Ctrl+D - Duplicate selected elements
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.ctrlKey && evt.keyCode == KeyCode.D)
                {
                    _copiedElements = selection.Where(e => e is AnimationStateNode).ToList();
                    PasteElements(20);
                    evt.StopPropagation();
                }
            });

            // F - Frame selected elements
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.F)
                {
                    if (selection.Count > 0)
                    {
                        FrameSelection();
                    }
                    else
                    {
                        FrameAll();
                    }

                    evt.StopPropagation();
                }
            });

            // Ctrl+A - Select all nodes
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.ctrlKey && evt.keyCode == KeyCode.A)
                {
                    foreach (Node node in nodes)
                    {
                        AddToSelection(node);
                    }

                    evt.StopPropagation();
                }
            });
        }

        // Connect two ports with an edge
        public Edge ConnectPorts(Port output, Port input)
        {
            // Create a new edge - Use our custom AnimationFlowEdge which has OnSelected override
            AnimationFlowEdge edge = new()
            {
                output = output,
                input = input
            };

            // Properly connect the edge to both ports
            input.Connect(edge);
            output.Connect(edge);

            // Add edge to graph
            AddElement(edge);

            return edge;
        }

        public new void DeleteElements(IEnumerable<GraphElement> elementsToDelete)
        {
            // First, collect all edges that need to be deleted
            // This includes edges explicitly selected for deletion and edges connected to nodes being deleted
            var edgesToDelete = new HashSet<Edge>();

            // Process all elements that are directly selected for deletion
            foreach (GraphElement element in elementsToDelete)
            {
                // If it's a node, collect its connected edges
                if (element is AnimationStateNode node)
                {

                    // Get all connected edges (both input and output)
                    Port inputPort = node.inputContainer.Q<Port>();
                    Port outputPort = node.outputContainer.Q<Port>();

                    if (inputPort != null)
                    {
                        foreach (Edge edge in inputPort.connections)
                        {
                            edgesToDelete.Add(edge);
                        }
                    }

                    if (outputPort != null)
                    {
                        foreach (Edge edge in outputPort.connections)
                        {
                            edgesToDelete.Add(edge);
                        }
                    }
                }
                // If it's already an edge, add it directly
                else if (element is Edge edge)
                {
                    edgesToDelete.Add(edge);
                }
            }

            // Remove edge conditions from the EdgeConditionManager
            foreach (Edge edge in edgesToDelete)
            {
                string edgeId = EdgeConditionManager.GetEdgeId(edge);
                if (!string.IsNullOrEmpty(edgeId))
                {
                    EdgeConditionManager.Instance.RemoveConditions(edgeId);
                }
            }

            // Delete all edges that need to be removed
            foreach (Edge edge in edgesToDelete)
            {
                // Remove from ports
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);

                // Remove from graph
                RemoveElement(edge);
            }

            // Delete all nodes last
            foreach (GraphElement element in elementsToDelete)
            {
                // Skip edges as we've already handled them
                if (element is Edge) continue;

                RemoveElement(element);
            }

            // Hide transition editor panel if needed
            if (_transitionEditorPanel != null && !_transitionEditorPanel.IsBeingInteracted())
            {
                _transitionEditorPanel.Hide();
            }
        }


        private void PasteElements(float offset = 0)
        {
            if (_copiedElements is null || _copiedElements.Count == 0)
                return;

            ClearSelection();

            // Create a dictionary to track new nodes created from original nodes
            var originalToNew = new Dictionary<string, AnimationStateNode>();

            // First pass: create new nodes
            foreach (ISelectable element in _copiedElements)
            {
                if (element is AnimationStateNode origNode)
                {
                    // Get position with offset
                    Rect originalPos = origNode.GetPosition();
                    Rect newPos = new(
                        originalPos.x + offset,
                        originalPos.y + offset,
                        originalPos.width,
                        originalPos.height);

                    // Clone the node
                    AnimationStateNode newNode = CreateStateNode(
                        origNode.StateType,
                        origNode.AnimationName,
                        newPos);

                    // Keep track of the mapping
                    originalToNew[origNode.ID] = newNode;

                    // Select the new node
                    AddToSelection(newNode);
                }
            }

            // Second pass: recreate connections between pasted nodes
            foreach (ISelectable element in _copiedElements)
            {
                if (element is AnimationStateNode origNode)
                {
                    // Skip if not in our map
                    if (!originalToNew.TryGetValue(origNode.ID, out AnimationStateNode newNode))
                        continue;

                    // Get the output port of this node
                    Port outputPort = (Port)newNode.outputContainer[0];

                    // Check all connections from original node's output port
                    Port origOutputPort = (Port)origNode.outputContainer[0];
                    foreach (Edge origEdge in origOutputPort.connections)
                    {
                        // Only if target is also in our pasted set
                        if (origEdge.input.node is AnimationStateNode origTargetNode &&
                            originalToNew.TryGetValue(origTargetNode.ID, out AnimationStateNode newTargetNode))
                        {
                            // Get input port of target
                            Port inputPort = (Port)newTargetNode.inputContainer[0];

                            // Create new edge
                            ConnectPorts(outputPort, inputPort);
                        }
                    }
                }
            }
        }
    }
}
