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

            if (styleSheet != null)
                styleSheets.Add(styleSheet);

            // Set up node creation
            SetupNodeCreation();

            // Register keyboard shortcuts
            RegisterKeyboardShortcuts();

            // Initialize with default animations
            _availableAnimations = new List<string>
            {
                "Idle", "Walk", "Run", "Jump", "Fall"
            };

            Debug.Log("[AnimationFlowGraphView] Initialized with default animations");
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
            if (_copiedElements == null || _copiedElements.Count == 0)
                return;

            ClearSelection();

            // Create a dictionary to track new nodes created from original nodes
            var originalToNew = new Dictionary<string, AnimationStateNode>();

            // First pass: create new nodes
            foreach (ISelectable element in _copiedElements)
            {
                if (element is AnimationStateNode sourceNode)
                {
                    // Get the position of the original node and offset it
                    Rect sourceRect = sourceNode.GetPosition();
                    Vector2 newPos = new(
                        sourceRect.x + offset,
                        sourceRect.y + offset
                    );

                    // Create a new node with same properties but a new ID
                    AnimationStateNode newNode = CreateStateNode(
                        sourceNode.StateType,
                        sourceNode.AnimationName + " (Copy)",
                        new Rect(newPos, sourceRect.size),
                        null, // Generate a new ID
                        false, // Not initial state
                        sourceNode.FrameToHold
                    );

                    // Add to selection
                    AddToSelection(newNode);

                    // Track the relationship between original and new
                    originalToNew[sourceNode.ID] = newNode;
                }
            }

            // Second pass: recreate connections between copied nodes
            foreach (ISelectable element in _copiedElements)
            {
                if (element is AnimationStateNode sourceNode)
                {
                    // Find all edges connected to this source node
                    var connectedEdges = edges.ToList()
                        .Where(e => e.output.node == sourceNode || e.input.node == sourceNode);

                    foreach (Edge edge in connectedEdges)
                    {
                        // Only create edges between copied nodes
                        if (edge.output.node is AnimationStateNode outputNode &&
                            edge.input.node is AnimationStateNode inputNode)
                        {
                            if (_copiedElements.Contains(outputNode) &&
                                _copiedElements.Contains(inputNode))
                            {
                                // Get the corresponding new nodes
                                AnimationStateNode newOutputNode = originalToNew[outputNode.ID];
                                AnimationStateNode newInputNode = originalToNew[inputNode.ID];

                                // Find the correct ports
                                Port outputPort = newOutputNode.outputContainer.Q<Port>();
                                Port inputPort = newInputNode.inputContainer.Q<Port>();

                                // Connect them
                                if (outputPort != null && inputPort != null)
                                {
                                    ConnectPorts(outputPort, inputPort);
                                }
                            }
                        }
                    }
                }
            }
        }

        public AnimationStateNode CreateStateNode(string stateType, string animationName, Rect position,
            string id = null, bool isInitialState = false, int frameToHold = 0)
        {
            AnimationStateNode node = new(stateType, animationName);
            node.SetPosition(position);

            // Set provided ID if one is passed, otherwise the constructor will create a new GUID
            if (!string.IsNullOrEmpty(id))
            {
                node.ID = id;
            }

            // Set initial state if specified
            node.IsInitialState = isInitialState;

            // Set frame to hold if specified
            node.FrameToHold = frameToHold;

            // Create standard input port using the default Edge type
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(bool));

            inputPort.portName = "In";

            // Create standard output port using the default Edge type
            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output,
                Port.Capacity.Multi, typeof(bool));

            outputPort.portName = "Out";

            // Add ports to node
            node.inputContainer.Add(inputPort);
            node.outputContainer.Add(outputPort);

            // Add node to graph
            AddElement(node);

            // Force immediate layout update to ensure ports are visible
            node.RefreshAnimationList(_availableAnimations);
            node.RefreshExpandedState();
            node.RefreshPorts();

            if (nodes.ToList().Count == 1) // It's 1 because we just added this node
            {
                node.IsInitialState = true;
                node.RefreshInitialStateToggle();
            }

            return node;
        }

        /// <summary>
        ///     Called when the target GameObject changes
        /// </summary>
        public void OnTargetGameObjectChanged(GameObject targetGameObject, IAnimator targetAnimator)
        {
            _targetGameObject = targetGameObject;
            _targetAnimator = targetAnimator;

            // Update available animations
            if (_targetAnimator is not null)
            {
                _availableAnimations = AnimationNameProvider.GetAnimationNames(_targetAnimator);
            }
            else if (_targetGameObject)
            {
                _availableAnimations = AnimationNameProvider.GetAnimationNamesFromGameObject(_targetGameObject);
            }
            else
            {
                _availableAnimations = AnimationNameProvider.GetAnimationNamesFromSelection();
            }

            // Refresh animation dropdowns in existing nodes
            RefreshNodeAnimationLists();
        }

        /// <summary>
        ///     Refresh animation lists in all existing nodes
        /// </summary>
        private void RefreshNodeAnimationLists()
        {
            foreach (Node node in nodes.ToList())
            {
                if (node is AnimationStateNode animNode)
                {
                    animNode.RefreshAnimationList(_availableAnimations);
                }
            }
        }

        /// <summary>
        ///     Get the current list of available animations
        /// </summary>
        public List<string> GetAvailableAnimations() => new(_availableAnimations);

        /// <summary>
        ///     Clears all nodes and edges from the graph view
        /// </summary>
        public void ClearGraph()
        {
            // Remove all edges
            foreach (Edge edge in edges.ToList())
            {
                RemoveElement(edge);
            }

            // Remove all nodes
            foreach (Node node in nodes.ToList())
            {
                RemoveElement(node);
            }
        }
    }
}
