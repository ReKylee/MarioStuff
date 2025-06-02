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

            // Add edge selection handler
            _ = new EdgeSelectionHandler(this);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Handle edge creation
            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    // Get the edge ID from our condition manager
                    string edgeId = EdgeConditionManager.Instance.GetEdgeId(edge);

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
                { "Hold Frame State", typeof(HoldFrameState) },
                { "One Time State", typeof(OneTimeState) },
                { "Looping State", typeof(LoopingState) }
            };

            // Add right-click context menu for node creation
            this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
            {
                // Convert screen position to graph position
                Vector2 localMousePosition = contentViewContainer.WorldToLocal(menuEvent.mousePosition);

                // Add each node type from our dictionary without the disabled header
                foreach (var nodeType in nodeTypes)
                {
                    menuEvent.menu.AppendAction(nodeType.Key,
                        action => CreateStateNode(nodeType.Value.Name.Replace("State", ""), "NewAnimation",
                            new Rect(localMousePosition, new Vector2(150, 200))));
                }
            }));

            // Set up the "+" button functionality
            nodeCreationRequest = context =>
            {
                // Convert screen position to graph position
                Vector2 graphPosition = contentViewContainer.WorldToLocal(context.screenMousePosition);

                // Create context menu
                GenericMenu menu = new();

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
            node.RefreshExpandedState();
            node.RefreshPorts();

            // Auto-set initial state if this is the first node
            if (nodes.ToList().Count == 1) // It's 1 because we just added this node
            {
                node.IsInitialState = true;
                node.RefreshInitialStateToggle();
            }

            return node;
        }

        public Edge ConnectPorts(Port output, Port input)
        {
            // Create a new standard edge
            Edge edge = new()
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

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                // Don't connect to self
                if (startPort.node == port.node)
                    return;

                // Only connect input to output
                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        // Fixed method to avoid infinite recursion
        public new void FrameAll()
        {
            if (nodes.Any())
            {
                // Call the correct base method - don't call ourselves recursively
                base.FrameAll();
            }
        }
    }

    public class AnimationStateNode : Node
    {
        private TextField _animationNameField;
        private VisualElement _contentContainer;
        private IntegerField _frameToHoldField;
        private Toggle _initialStateToggle;
        private bool _isCollapsed;

        public AnimationStateNode(string stateType, string animationName)
        {
            StateType = stateType;
            AnimationName = animationName;
            ID = Guid.NewGuid().ToString();

            // Set title to animation name
            title = animationName;

            // Build node UI
            BuildNodeUI();
        }

        public string StateType { get; }
        public string AnimationName { get; set; }
        public bool IsInitialState { get; set; }
        public int FrameToHold { get; set; }
        public string ID { get; set; }

        public void RefreshInitialStateToggle()
        {
            if (_initialStateToggle != null)
                _initialStateToggle.value = IsInitialState;

            // Update visual appearance
            titleContainer.style.backgroundColor = GetStateColor();
        }

        public void RefreshFrameToHoldField()
        {
            if (_frameToHoldField != null)
                _frameToHoldField.value = FrameToHold;
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
            if (titleLabel != null)
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
            VisualElement nameContainer = new();
            nameContainer.style.flexDirection = FlexDirection.Row;
            nameContainer.style.marginLeft = 8;
            nameContainer.style.marginRight = 8;
            nameContainer.style.marginTop = 4;

            Label nameLabel = new("Animation:");
            nameLabel.style.minWidth = 70;
            nameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            nameContainer.Add(nameLabel);

            _animationNameField = new TextField();
            _animationNameField.value = AnimationName;
            _animationNameField.RegisterValueChangedCallback(evt =>
            {
                AnimationName = evt.newValue;
                title = evt.newValue; // Update node title when animation name changes
            });

            _animationNameField.style.flexGrow = 1;
            nameContainer.Add(_animationNameField);

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

            Label typeValueLabel = new(StateType);
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

                if (!wasInitial && IsInitialState)
                {
                    // Clear other initial states
                    if (parent is GraphView graphView)
                    {
                        var nodes = graphView.nodes.ToList().Cast<AnimationStateNode>();
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

            // Add type-specific UI
            switch (StateType)
            {
                case "HoldFrame":
                    VisualElement frameContainer = new();
                    frameContainer.style.flexDirection = FlexDirection.Row;
                    frameContainer.style.marginLeft = 8;
                    frameContainer.style.marginRight = 8;
                    frameContainer.style.marginTop = 8;
                    frameContainer.style.marginBottom = 8;

                    Label frameLabel = new("Hold Frame:");
                    frameLabel.style.minWidth = 70;
                    frameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
                    frameContainer.Add(frameLabel);

                    _frameToHoldField = new IntegerField();
                    _frameToHoldField.value = FrameToHold;
                    _frameToHoldField.RegisterValueChangedCallback(evt => FrameToHold = evt.newValue);
                    _frameToHoldField.style.flexGrow = 1;
                    frameContainer.Add(_frameToHoldField);

                    _contentContainer.Add(frameContainer);
                    break;
                default:
                    // Add some padding at the bottom for other node types
                    VisualElement spacer = new();
                    spacer.style.height = 8;
                    _contentContainer.Add(spacer);
                    break;
            }

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
                case "HoldFrame":
                    return new Color(0.2f, 0.2f, 0.6f); // Blue for hold frame
                case "OneTime":
                    return new Color(0.6f, 0.3f, 0.1f); // Orange for one-time
                case "Looping":
                    return new Color(0.4f, 0.1f, 0.5f); // Purple for looping
                default:
                    return new Color(0.3f, 0.3f, 0.3f); // Gray for unknown
            }
        }
    }

    public class AnimationFlowEdge : Edge
    {

        public AnimationFlowEdge()
        {
            // Style the edge for better visual appearance
            AddToClassList("flow-edge");

            // Make the edge capable of being selected/dragged
            capabilities |= Capabilities.Selectable | Capabilities.Deletable;

            // Add visual arrow to show direction
            edgeControl.Add(new FlowArrow());

            // Make sure the edge is visible
            edgeControl.style.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            edgeControl.style.minWidth = 2;
        }
        public List<ConditionData> Conditions { get; set; } = new();

        public override void OnSelected()
        {
            base.OnSelected();
            // Show transition editing when selected
            ShowConditionEditor();
        }

        public void ShowConditionEditor()
        {
            TransitionEditorWindow.ShowWindow(this);
        }

        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Edit Transition", _ =>
            {
                // Use our edge selection utility to show the condition editor
                // This works with standard Edge objects
                TransitionEditorWindow.ShowWindow(this);
            });

        }
    }

    // Custom arrow control for edges
    public class FlowArrow : VisualElement
    {
        public FlowArrow()
        {
            // Set up the arrow visual element
            style.position = Position.Absolute;
            style.width = 10;
            style.height = 10;
            style.left = 0;
            style.top = 0;

            // Draw the arrow in GenerateVisualContent
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            // Get the painter for drawing
            Painter2D painter = mgc.painter2D;

            // Define arrow shape
            Vector2[] arrowPoints =
            {
                new(0, 0), // Tip
                new(-10, -5), // Left wing
                new(-7, 0), // Inset
                new(-10, 5) // Right wing
            };

            // Draw filled arrow
            painter.fillColor = new Color(0.8f, 0.8f, 0.8f);
            painter.BeginPath();
            painter.MoveTo(arrowPoints[0]);

            for (int i = 1; i < arrowPoints.Length; i++)
            {
                painter.LineTo(arrowPoints[i]);
            }

            painter.ClosePath();
            painter.Fill();
        }
    }

    public class TransitionEditorWindow : EditorWindow
    {
        private bool _boolValue = true;
        private string _compareType = "Equals";
        private string _conditionType = "Bool";
        private AnimationFlowEdge _currentEdge;
        private string _floatValue = "0";
        private string _parameterName = "";
        private Vector2 _scrollPosition;

        private void OnGUI()
        {
            if (_currentEdge == null)
            {
                EditorGUILayout.HelpBox("No transition selected.", MessageType.Warning);
                return;
            }

            AnimationStateNode sourceNode = _currentEdge.output.node as AnimationStateNode;
            AnimationStateNode targetNode = _currentEdge.input.node as AnimationStateNode;

            if (sourceNode == null || targetNode == null)
            {
                EditorGUILayout.HelpBox("Invalid transition.", MessageType.Error);
                return;
            }

            GUILayout.Label($"Transition: {sourceNode.AnimationName} → {targetNode.AnimationName}",
                EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // Display existing conditions
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

            if (_currentEdge.Conditions.Count == 0)
            {
                EditorGUILayout.HelpBox("No conditions. This transition will always occur.", MessageType.Info);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

                for (int i = 0; i < _currentEdge.Conditions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    ConditionData condition = _currentEdge.Conditions[i];
                    string conditionDesc = GetConditionDescription(condition);

                    EditorGUILayout.LabelField(conditionDesc);

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        _currentEdge.Conditions.RemoveAt(i);
                        i--;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            // Add new condition
            EditorGUILayout.LabelField("Add Condition", EditorStyles.boldLabel);

            _parameterName = EditorGUILayout.TextField("Parameter Name", _parameterName);

            _conditionType = _conditionType == "Bool" ? "Bool" : "Float";

            if (_conditionType == "Bool")
            {
                _boolValue = EditorGUILayout.Toggle("Value", _boolValue);
            }
            else
            {
                _compareType = EditorGUILayout.Popup("Comparison",
                        _compareType == "Equals" ? 0 : _compareType == "Less Than" ? 1 : 2,
                        new[] { "Equals", "Less Than", "Greater Than" }) == 0 ? "Equals" :
                    _compareType == "Less Than" ? "Less Than" : "Greater Than";

                _floatValue = EditorGUILayout.TextField("Value", _floatValue);
            }

            if (GUILayout.Button("Add Condition"))
            {
                if (!string.IsNullOrEmpty(_parameterName))
                {
                    AddCondition();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Condition", "Parameter name cannot be empty.", "OK");
                }
            }

            EditorGUILayout.Space();

            // Special conditions
            if (GUILayout.Button("Add Animation Complete Condition"))
            {
                ConditionData condition = new()
                {
                    Type = "AnimationComplete",
                    ParameterName = ""
                };

                _currentEdge.Conditions.Add(condition);
            }

            if (GUILayout.Button("Add Time Elapsed Condition"))
            {
                ConditionData condition = new()
                {
                    Type = "TimeElapsed",
                    ParameterName = "StateTime",
                    FloatValue = 0.5f
                };

                _currentEdge.Conditions.Add(condition);

                // Create a temporary editing UI for the newly added condition
                _conditionType = "Float";
                _parameterName = "StateTime";
                _floatValue = "0.5";
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        public static void ShowWindow(AnimationFlowEdge edge)
        {
            TransitionEditorWindow window = GetWindow<TransitionEditorWindow>(true, "Edit Transition");
            window.minSize = new Vector2(300, 400);
            window._currentEdge = edge;
            window.Show();
        }

        private void AddCondition()
        {
            ConditionData condition = new()
            {
                ParameterName = _parameterName
            };

            if (_conditionType == "Bool")
            {
                condition.Type = "Bool";
                condition.BoolValue = _boolValue;
            }
            else
            {
                condition.Type = "Float" + _compareType.Replace(" ", "");
                if (float.TryParse(_floatValue, out float value))
                {
                    condition.FloatValue = value;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Value", "Please enter a valid float value.", "OK");
                    return;
                }
            }

            _currentEdge.Conditions.Add(condition);

            // Clear input fields
            _parameterName = "";
        }

        private string GetConditionDescription(ConditionData condition)
        {
            // Dictionary mapping condition types to human-readable descriptions
            var conditionDescriptions = new Dictionary<string, Func<ConditionData, string>>
            {
                // Boolean conditions
                { "Bool", c => $"{c.ParameterName} {(c.BoolValue ? "is true" : "is false")}" },

                // Float comparison conditions
                { "FloatEquals", c => $"{c.ParameterName} = {c.FloatValue}" },
                { "FloatLessThan", c => $"{c.ParameterName} < {c.FloatValue}" },
                { "FloatGreaterThan", c => $"{c.ParameterName} > {c.FloatValue}" },

                // Special conditions
                { "AnimationComplete", _ => "Animation is complete" },
                { "TimeElapsed", c => $"Time in state > {c.FloatValue}s" },
                { "AnyCondition", _ => "Any condition is met" }
            };

            // If we have a description for this condition type, use it
            return conditionDescriptions.TryGetValue(condition.Type, out var formatter)
                ? formatter(condition)
                : $"Unknown condition: {condition.Type}";

        }
    }
}
