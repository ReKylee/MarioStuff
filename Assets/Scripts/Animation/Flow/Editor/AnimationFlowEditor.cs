using System;
using System.Collections.Generic;
using Animation.Flow.Core;
using Animation.Flow.Nodes.Composites;
using Animation.Flow.Nodes.Decorators;
using Animation.Flow.Nodes.Leaves;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Custom editor for the animation flow system
    ///     Provides a visual editor for creating and editing behavior trees
    /// </summary>
    [CustomEditor(typeof(AnimationFlowAsset))]
    public class AnimationFlowEditor : EditorWindow
    {
        // Constants
        private const float NodeWidth = 200f;
        private const float NodeHeight = 100f;
        private const float HeaderHeight = 20f;
        private const float ConnectionWidth = 3f;
        private Vector2 _connectingEnd;
        private FlowNode _connectingNode;
        private Vector2 _connectingStart;

        // Node factory
        private GenericMenu _createNodeMenu;
        private Vector2 _createNodePosition;

        // Current animation flow
        private AnimationFlowAsset _currentFlow;

        // Current state
        private Vector2 _dragOffset;
        private Vector2 _graphOffset;
        private GUIStyle _inPointStyle;
        private bool _isConnecting;
        private bool _isDragging;
        private bool _isSelecting;
        private GUIStyle _nodeHeaderStyle;
        private GUIStyle _nodeLabelStyle;

        // Styles
        private GUIStyle _nodeStyle;
        private GUIStyle _outPointStyle;
        private GUIStyle _rootNodeStyle;
        private FlowNode _selectedNode;
        private GUIStyle _selectedNodeStyle;
        private Rect _selectionRect;
        private Vector2 _selectionStart;

        private void OnEnable()
        {
            InitializeStyles();
            InitializeNodeFactory();
        }

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            if (_currentFlow != null)
            {
                DrawNodes();
                DrawConnections();
            }

            DrawToolbar();

            ProcessEvents(Event.current);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        [MenuItem("Window/Animation/Flow Editor")]
        public static void OpenWindow()
        {
            AnimationFlowEditor window = GetWindow<AnimationFlowEditor>("Animation Flow Editor");
            window.minSize = new Vector2(800, 600);
        }

        #region Initialization

        private void InitializeStyles()
        {
            _nodeStyle = new GUIStyle
            {
                normal = { background = EditorGUIUtility.Load("node0") as Texture2D },
                border = new RectOffset(12, 12, 12, 12),
                padding = new RectOffset(10, 10, 8, 8)
            };

            _rootNodeStyle = new GUIStyle
            {
                normal = { background = EditorGUIUtility.Load("node1") as Texture2D },
                border = new RectOffset(12, 12, 12, 12),
                padding = new RectOffset(10, 10, 8, 8)
            };

            _selectedNodeStyle = new GUIStyle
            {
                normal = { background = EditorGUIUtility.Load("node2") as Texture2D },
                border = new RectOffset(12, 12, 12, 12),
                padding = new RectOffset(10, 10, 8, 8)
            };

            _inPointStyle = new GUIStyle
            {
                normal = { background = EditorGUIUtility.Load("btn left") as Texture2D },
                active = { background = EditorGUIUtility.Load("btn left on") as Texture2D },
                alignment = TextAnchor.MiddleCenter
            };

            _outPointStyle = new GUIStyle
            {
                normal = { background = EditorGUIUtility.Load("btn right") as Texture2D },
                active = { background = EditorGUIUtility.Load("btn right on") as Texture2D },
                alignment = TextAnchor.MiddleCenter
            };

            _nodeLabelStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };

            _nodeHeaderStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        private void InitializeNodeFactory()
        {
            _createNodeMenu = new GenericMenu();

            // Composite nodes
            _createNodeMenu.AddItem(new GUIContent("Composites/Selector"), false, CreateNodeCallback,
                typeof(SelectorNode));

            _createNodeMenu.AddItem(new GUIContent("Composites/Sequence"), false, CreateNodeCallback,
                typeof(SequenceNode));

            _createNodeMenu.AddItem(new GUIContent("Composites/Parallel"), false, CreateNodeCallback,
                typeof(ParallelNode));

            // Decorator nodes
            _createNodeMenu.AddItem(new GUIContent("Decorators/Inverter"), false, CreateNodeCallback,
                typeof(InverterNode));

            _createNodeMenu.AddItem(new GUIContent("Decorators/Repeater"), false, CreateNodeCallback,
                typeof(RepeaterNode));

            // Leaf nodes
            _createNodeMenu.AddItem(new GUIContent("Leaves/Play Animation"), false, CreateNodeCallback,
                typeof(PlayAnimationNode));

            _createNodeMenu.AddItem(new GUIContent("Leaves/Check Parameter"), false, CreateNodeCallback,
                typeof(CheckParameterNode));

            _createNodeMenu.AddItem(new GUIContent("Leaves/Wait"), false, CreateNodeCallback, typeof(WaitNode));
        }

        #endregion

        #region Drawing

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 newOffset = new(_graphOffset.x % gridSpacing, _graphOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(
                    new Vector3(gridSpacing * i + newOffset.x, 0, 0),
                    new Vector3(gridSpacing * i + newOffset.x, position.height, 0)
                );
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(
                    new Vector3(0, gridSpacing * j + newOffset.y, 0),
                    new Vector3(position.width, gridSpacing * j + newOffset.y, 0)
                );
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (_currentFlow == null || _currentFlow.Nodes == null)
                return;

            foreach (FlowNode node in _currentFlow.Nodes)
            {
                if (node == null)
                    continue;

                DrawNode(node);
            }
        }

        private void DrawNode(FlowNode node)
        {
            Vector2 position = node.Position + _graphOffset;
            Rect nodeRect = new(position.x, position.y, NodeWidth, NodeHeight);

            // Choose the style based on the node's state
            GUIStyle style = _nodeStyle;
            if (node == _currentFlow.RootNode)
            {
                style = _rootNodeStyle;
            }
            else if (node == _selectedNode)
            {
                style = _selectedNodeStyle;
            }

            // Draw the node background
            GUI.Box(nodeRect, "", style);

            // Draw the header
            Rect headerRect = new(nodeRect.x, nodeRect.y, nodeRect.width, HeaderHeight);
            GUI.Box(headerRect, node.GetType().Name, _nodeHeaderStyle);

            // Draw the node content
            Rect contentRect = new(nodeRect.x, nodeRect.y + HeaderHeight, nodeRect.width,
                nodeRect.height - HeaderHeight);

            GUI.Label(contentRect, node.Name, _nodeLabelStyle);

            // Draw the connection points
            DrawNodeConnectionPoints(node, nodeRect);
        }

        private void DrawNodeConnectionPoints(FlowNode node, Rect nodeRect)
        {
            // Draw the input connection point (all nodes can have inputs)
            Rect inputRect = new(nodeRect.x - 8, nodeRect.y + nodeRect.height / 2 - 8, 16, 16);
            GUI.Box(inputRect, "", _inPointStyle);

            // Only draw output points for composite and decorator nodes
            if (node is CompositeNode || node is DecoratorNode)
            {
                Rect outputRect = new(nodeRect.x + nodeRect.width - 8, nodeRect.y + nodeRect.height / 2 - 8, 16, 16);
                GUI.Box(outputRect, "", _outPointStyle);
            }
        }

        private void DrawConnections()
        {
            if (_currentFlow == null || _currentFlow.Nodes == null)
                return;

            Handles.BeginGUI();

            foreach (FlowNode node in _currentFlow.Nodes)
            {
                if (node == null)
                    continue;

                DrawNodeConnections(node);
            }

            // Draw the connection being created, if any
            if (_isConnecting)
            {
                Handles.color = Color.white;
                Handles.DrawBezier(
                    _connectingStart,
                    _connectingEnd,
                    _connectingStart + Vector2.right * 50f,
                    _connectingEnd + Vector2.left * 50f,
                    Color.white,
                    null,
                    ConnectionWidth
                );
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodeConnections(FlowNode node)
        {
            Vector2 startPos = node.Position + _graphOffset;
            startPos.x += NodeWidth - 8;
            startPos.y += NodeHeight / 2;

            // Get the node's children
            var children = node.GetChildren();

            // Draw connections to all children
            foreach (FlowNode child in children)
            {
                if (child == null)
                    continue;

                Vector2 endPos = child.Position + _graphOffset;
                endPos.x += 8;
                endPos.y += NodeHeight / 2;

                // Choose color based on node type
                Color connectionColor = Color.white;
                if (node is SelectorNode)
                {
                    connectionColor = Color.yellow;
                }
                else if (node is SequenceNode)
                {
                    connectionColor = Color.green;
                }
                else if (node is ParallelNode)
                {
                    connectionColor = Color.cyan;
                }
                else if (node is DecoratorNode)
                {
                    connectionColor = Color.magenta;
                }

                Handles.DrawBezier(
                    startPos,
                    endPos,
                    startPos + Vector2.right * 50f,
                    endPos + Vector2.left * 50f,
                    connectionColor,
                    null,
                    ConnectionWidth
                );
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Open", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                OpenAnimationFlow();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SaveAnimationFlow();
            }

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateAnimationFlow();
            }

            GUILayout.FlexibleSpace();

            // Display the current flow name
            if (_currentFlow != null)
            {
                GUILayout.Label(_currentFlow.name, EditorStyles.boldLabel);
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Event Processing

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        ProcessLeftMouseDown(e);
                    }
                    else if (e.button == 1)
                    {
                        ProcessRightMouseDown(e);
                    }

                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        ProcessLeftMouseUp(e);
                    }

                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        ProcessLeftMouseDrag(e);
                    }
                    else if (e.button == 2)
                    {
                        ProcessMiddleMouseDrag(e);
                    }

                    break;

                case EventType.MouseMove:
                    ProcessMouseMove(e);
                    break;

                case EventType.KeyDown:
                    ProcessKeyDown(e);
                    break;
            }
        }

        private void ProcessLeftMouseDown(Event e)
        {
            if (_currentFlow == null)
                return;

            // Check if we clicked on a node
            FlowNode clickedNode = GetNodeAtPosition(e.mousePosition);

            if (clickedNode != null)
            {
                // Check if we clicked on a connection point
                if (IsPositionOverInputPoint(clickedNode, e.mousePosition))
                {
                    StartConnectionFromInput(clickedNode, e.mousePosition);
                }
                else if (IsPositionOverOutputPoint(clickedNode, e.mousePosition))
                {
                    StartConnectionFromOutput(clickedNode, e.mousePosition);
                }
                else
                {
                    // We clicked on the node itself
                    _selectedNode = clickedNode;
                    _isDragging = true;
                    _dragOffset = e.mousePosition - clickedNode.Position;
                }

                e.Use();
            }
            else
            {
                // Start selection rectangle
                _isSelecting = true;
                _selectionStart = e.mousePosition;
                _selectionRect = new Rect(e.mousePosition, Vector2.zero);

                // Deselect current node
                _selectedNode = null;

                e.Use();
            }
        }

        private void ProcessRightMouseDown(Event e)
        {
            if (_currentFlow == null)
                return;

            // Check if we right-clicked on a node
            FlowNode clickedNode = GetNodeAtPosition(e.mousePosition);

            if (clickedNode != null)
            {
                // Show context menu for node
                GenericMenu nodeMenu = new();
                nodeMenu.AddItem(new GUIContent("Set as Root"), false, SetNodeAsRoot, clickedNode);
                nodeMenu.AddItem(new GUIContent("Delete"), false, DeleteNode, clickedNode);
                nodeMenu.ShowAsContext();
            }
            else
            {
                // Show context menu for creating nodes
                _createNodePosition = e.mousePosition - _graphOffset;
                _createNodeMenu.ShowAsContext();
            }

            e.Use();
        }

        private void ProcessLeftMouseUp(Event e)
        {
            _isDragging = false;

            if (_isConnecting)
            {
                // Check if we released over a node's input
                FlowNode targetNode = GetNodeAtPosition(e.mousePosition);

                if (targetNode != null && IsPositionOverInputPoint(targetNode, e.mousePosition))
                {
                    ConnectNodes(_connectingNode, targetNode);
                }

                _isConnecting = false;
                e.Use();
            }

            if (_isSelecting)
            {
                _isSelecting = false;

                // Select nodes that are inside the selection rectangle
                _selectionRect = GetNormalizedRect(_selectionStart, e.mousePosition);

                // Find nodes inside the selection rect
                // For now, just select the first one found
                foreach (FlowNode node in _currentFlow.Nodes)
                {
                    if (node == null)
                        continue;

                    Rect nodeRect = new(node.Position + _graphOffset, new Vector2(NodeWidth, NodeHeight));
                    if (_selectionRect.Overlaps(nodeRect))
                    {
                        _selectedNode = node;
                        break;
                    }
                }

                e.Use();
            }
        }

        private void ProcessLeftMouseDrag(Event e)
        {
            if (_isDragging && _selectedNode != null)
            {
                _selectedNode.Position = e.mousePosition - _dragOffset - _graphOffset;
                e.Use();
            }
            else if (_isConnecting)
            {
                _connectingEnd = e.mousePosition;
                e.Use();
            }
            else if (_isSelecting)
            {
                _selectionRect = GetNormalizedRect(_selectionStart, e.mousePosition);
                e.Use();
            }
        }

        private void ProcessMiddleMouseDrag(Event e)
        {
            _graphOffset += e.delta;
            e.Use();
        }

        private void ProcessMouseMove(Event e)
        {
            if (_isConnecting)
            {
                _connectingEnd = e.mousePosition;
                e.Use();
            }
        }

        private void ProcessKeyDown(Event e)
        {
            if (e.keyCode == KeyCode.Delete && _selectedNode != null)
            {
                DeleteNode(_selectedNode);
                e.Use();
            }
        }

        #endregion

        #region Node Operations

        private void CreateNodeCallback(object nodeType)
        {
            if (_currentFlow == null)
                return;

            FlowNode newNode = CreateNodeOfType((Type)nodeType);

            if (newNode != null)
            {
                newNode.Position = _createNodePosition;
                _currentFlow.AddNode(newNode);

                // If this is the first node, set it as root
                if (_currentFlow.RootNode == null)
                {
                    _currentFlow.SetRootNode(newNode);
                }

                EditorUtility.SetDirty(_currentFlow);
            }
        }

        private FlowNode CreateNodeOfType(Type nodeType)
        {
            FlowNode node = CreateInstance(nodeType) as FlowNode;

            if (node != null)
            {
                node.name = nodeType.Name;
                AssetDatabase.AddObjectToAsset(node, _currentFlow);
                AssetDatabase.SaveAssets();
            }

            return node;
        }

        private void DeleteNode(object nodeObj)
        {
            FlowNode node = nodeObj as FlowNode;

            if (_currentFlow == null || node == null)
                return;

            // Remove all connections to this node
            foreach (FlowNode otherNode in _currentFlow.Nodes)
            {
                if (otherNode == null)
                    continue;

                if (otherNode is CompositeNode compositeNode)
                {
                    compositeNode.RemoveChild(node);
                }
                else if (otherNode is DecoratorNode decoratorNode && decoratorNode.Child == node)
                {
                    decoratorNode.Child = null;
                }
            }

            // Remove from the flow asset
            _currentFlow.RemoveNode(node);

            // If this was the selected node, deselect it
            if (_selectedNode == node)
            {
                _selectedNode = null;
            }

            // Destroy the node asset
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(_currentFlow);
        }

        private void SetNodeAsRoot(object nodeObj)
        {
            FlowNode node = nodeObj as FlowNode;

            if (_currentFlow == null || node == null)
                return;

            _currentFlow.SetRootNode(node);
            EditorUtility.SetDirty(_currentFlow);
        }

        private void ConnectNodes(FlowNode parentNode, FlowNode childNode)
        {
            if (parentNode == null || childNode == null)
                return;

            // Prevent connecting a node to itself
            if (parentNode == childNode)
                return;

            // Prevent circular references
            if (WouldCreateCircularReference(parentNode, childNode))
            {
                Debug.LogWarning("Cannot create circular reference in behavior tree.");
                return;
            }

            // Add the child to the parent
            if (parentNode is CompositeNode compositeNode)
            {
                compositeNode.AddChild(childNode);
                EditorUtility.SetDirty(parentNode);
            }
            else if (parentNode is DecoratorNode decoratorNode)
            {
                decoratorNode.Child = childNode;
                EditorUtility.SetDirty(parentNode);
            }
        }

        #endregion

        #region File Operations

        private void CreateAnimationFlow()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Animation Flow Asset",
                "New Animation Flow",
                "asset",
                "Create a new Animation Flow asset"
            );

            if (string.IsNullOrEmpty(path))
                return;

            AnimationFlowAsset flowAsset = CreateInstance<AnimationFlowAsset>();
            AssetDatabase.CreateAsset(flowAsset, path);
            AssetDatabase.SaveAssets();

            _currentFlow = flowAsset;
            _selectedNode = null;
            _graphOffset = Vector2.zero;
        }

        private void OpenAnimationFlow()
        {
            string path = EditorUtility.OpenFilePanel(
                "Open Animation Flow Asset",
                "Assets",
                "asset"
            );

            if (string.IsNullOrEmpty(path))
                return;

            // Convert from absolute path to asset path
            path = path.Substring(path.IndexOf("Assets"));

            AnimationFlowAsset flowAsset = AssetDatabase.LoadAssetAtPath<AnimationFlowAsset>(path);

            if (flowAsset != null)
            {
                _currentFlow = flowAsset;
                _selectedNode = null;
                _graphOffset = Vector2.zero;
            }
        }

        private void SaveAnimationFlow()
        {
            if (_currentFlow == null)
                return;

            EditorUtility.SetDirty(_currentFlow);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Utility Methods

        private FlowNode GetNodeAtPosition(Vector2 position)
        {
            if (_currentFlow == null || _currentFlow.Nodes == null)
                return null;

            // Check nodes in reverse order (so top-most nodes are selected first)
            for (int i = _currentFlow.Nodes.Count - 1; i >= 0; i--)
            {
                FlowNode node = _currentFlow.Nodes[i];
                if (node == null)
                    continue;

                Rect nodeRect = new(node.Position + _graphOffset, new Vector2(NodeWidth, NodeHeight));
                if (nodeRect.Contains(position))
                {
                    return node;
                }
            }

            return null;
        }

        private bool IsPositionOverInputPoint(FlowNode node, Vector2 position)
        {
            Vector2 nodePosition = node.Position + _graphOffset;
            Rect inputRect = new(nodePosition.x - 8, nodePosition.y + NodeHeight / 2 - 8, 16, 16);
            return inputRect.Contains(position);
        }

        private bool IsPositionOverOutputPoint(FlowNode node, Vector2 position)
        {
            // Only composite and decorator nodes have output points
            if (!(node is CompositeNode || node is DecoratorNode))
                return false;

            Vector2 nodePosition = node.Position + _graphOffset;
            Rect outputRect = new(nodePosition.x + NodeWidth - 8, nodePosition.y + NodeHeight / 2 - 8, 16, 16);
            return outputRect.Contains(position);
        }

        private void StartConnectionFromOutput(FlowNode node, Vector2 position)
        {
            _isConnecting = true;
            _connectingNode = node;
            _connectingStart = position;
            _connectingEnd = position;
        }

        private void StartConnectionFromInput(FlowNode node, Vector2 position)
        {
            // For input connections, we need to find who's connected to this node and disconnect them
            // Not implemented in this example
        }

        private bool WouldCreateCircularReference(FlowNode parent, FlowNode child)
        {
            // Check if child is an ancestor of parent
            var nodesToCheck = new Queue<FlowNode>();
            nodesToCheck.Enqueue(child);

            while (nodesToCheck.Count > 0)
            {
                FlowNode currentNode = nodesToCheck.Dequeue();

                if (currentNode == parent)
                {
                    return true; // Circular reference found
                }

                foreach (FlowNode grandchild in currentNode.GetChildren())
                {
                    nodesToCheck.Enqueue(grandchild);
                }
            }

            return false;
        }

        private Rect GetNormalizedRect(Vector2 start, Vector2 end)
        {
            Rect rect = new();

            rect.x = Mathf.Min(start.x, end.x);
            rect.y = Mathf.Min(start.y, end.y);
            rect.width = Mathf.Abs(end.x - start.x);
            rect.height = Mathf.Abs(end.y - start.y);

            return rect;
        }

        #endregion

    }
}
