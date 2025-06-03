using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.States;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    public class AnimationFlowGraphView : GraphView
    {
        // Store the available animation names
        private List<string> _availableAnimations;

        // Clipboard data for copy/paste operations
        private List<ISelectable> _copiedElements = new();

        // Track if we're currently handling a selection change to prevent recursion
        private bool _isHandlingSelection;
        private IAnimator _targetAnimator;

        // Store references to the target GameObject and animator
        private GameObject _targetGameObject;

        // Transition editor panel (embedded in the graph view)
        private TransitionEditorPanel _transitionEditorPanel;

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

            // Set up node creation
            SetupNodeCreation();

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
        public AnimationStateNode CreateStateNode(string stateType, string animationName, Rect position,
            string customId = null, bool isInitialState = false, int frameToHold = 0)
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

        private void SetupNodeCreation()
        {
            // Define node types with their corresponding classes for more type safety
            var nodeTypes = new Dictionary<string, Type>
            {
                { "Looping State", typeof(LoopingState) },
                { "One Time State", typeof(OneTimeState) },
                { "Hold Frame State", typeof(HoldFrameState) }
            };

            // Add right-click context menu for node creation
            this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
            {
                // Convert screen position to graph position
                Vector2 localMousePosition = contentViewContainer.WorldToLocal(menuEvent.mousePosition);

                // Add animation state options at the top level 
                // Add each node type directly to it
                foreach (var nodeType in nodeTypes)
                {
                    menuEvent.menu.AppendAction($"✨ Create Animation State/{nodeType.Key}",
                        _ => CreateStateNode(nodeType.Value.Name.Replace("State", ""), "NewAnimation",
                            new Rect(localMousePosition, new Vector2(150, 200))));
                }

                // Add separator
                menuEvent.menu.AppendSeparator();

                // Add selection and navigation options
                menuEvent.menu.AppendAction("🔍 Frame Selection", _ => FrameSelection(),
                    selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvent.menu.AppendAction("🔍 Frame All", _ => FrameAll());

                // Add separator
                menuEvent.menu.AppendSeparator();

                // Add edit options
                menuEvent.menu.AppendAction("✂️ Cut", _ =>
                {
                    // Store selected nodes for cutting
                    _copiedElements = selection.Where(e => e is AnimationStateNode).ToList();
                    DeleteElements(selection.OfType<GraphElement>().ToList());
                }, selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvent.menu.AppendAction("📝 Copy",
                    _ => { _copiedElements = selection.Where(e => e is AnimationStateNode).ToList(); },
                    selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvent.menu.AppendAction("📋 Paste", _ => PasteElements(),
                    _copiedElements.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvent.menu.AppendAction("🗑️ Delete",
                    _ => { DeleteElements(selection.OfType<GraphElement>().ToList()); },
                    selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                // Add separator and select options
                menuEvent.menu.AppendSeparator();
                menuEvent.menu.AppendAction("📌 Select All Nodes", _ =>
                {
                    foreach (Node node in nodes)
                    {
                        AddToSelection(node);
                    }
                });
            }));

            // Set up the "+" button functionality
            nodeCreationRequest = context =>
            {
                // Convert screen position to graph position
                Vector2 graphPosition = contentViewContainer.WorldToLocal(context.screenMousePosition);

                // Create context menu
                GenericMenu menu = new();

                // Add a heading as the first item
                menu.AddDisabledItem(new GUIContent("✨ Create Animation State"));
                menu.AddSeparator("");

                // Add each node type from our dictionary
                foreach (var nodeType in nodeTypes)
                {
                    menu.AddItem(new GUIContent(nodeType.Key), false,
                        () => CreateStateNode(nodeType.Value.Name.Replace("State", ""), "NewAnimation",
                            new Rect(graphPosition, new Vector2(150, 200))));
                }

                menu.ShowAsContext();
            };
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
