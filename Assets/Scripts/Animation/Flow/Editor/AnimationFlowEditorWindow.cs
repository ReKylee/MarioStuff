using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animation.Flow.Editor
{
    // Add EditorOnly tag to ensure code stripping in builds
    [InitializeOnLoad]
    public class AnimationFlowEditorWindow : EditorWindow
    {

        // Static field to remember the last opened asset between domain reloads
        private static string _lastOpenedAssetPath;
        private AnimationFlowAsset _currentAsset;
        private AnimationFlowGraphView _graphView;

        // Static constructor for InitializeOnLoad
        static AnimationFlowEditorWindow()
        {
            // Register for domain reload event handling
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();

            // Try to reopen the last asset if available
            if (!string.IsNullOrEmpty(_lastOpenedAssetPath))
            {
                AnimationFlowAsset asset = AssetDatabase.LoadAssetAtPath<AnimationFlowAsset>(_lastOpenedAssetPath);
                if (asset != null)
                {
                    LoadAsset(asset);
                }
            }
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        // Handle domain reloads cleanly
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Clean up any resources or refresh if needed
                var windows = Resources.FindObjectsOfTypeAll<AnimationFlowEditorWindow>();
                foreach (AnimationFlowEditorWindow window in windows)
                {
                    window.RefreshGraphView();
                }
            }
        }

        private void RefreshGraphView()
        {
            // Refresh only if needed
            if (_currentAsset != null && _graphView != null)
            {
                LoadAsset(_currentAsset);
            }
        }

        [MenuItem("Window/Animation Flow Editor")]
        public static void ShowWindow()
        {
            AnimationFlowEditorWindow window = GetWindow<AnimationFlowEditorWindow>("Animation Flow Editor");
            window.minSize = new Vector2(800, 600);
        }

        private void ConstructGraphView()
        {
            _graphView = new AnimationFlowGraphView
            {
                name = "Animation Flow Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            Toolbar toolbar = new();

            Button saveButton = new(() => SaveGraph())
            {
                text = "Save"
            };

            toolbar.Add(saveButton);

            Button loadButton = new(() => LoadGraph())
            {
                text = "Load"
            };

            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveGraph()
        {
            if (_graphView == null)
            {
                Debug.LogError("GraphView is not available.");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Animation Flow", "New Animation Flow", "asset",
                "Please enter a file name to save the animation flow to.");

            if (string.IsNullOrEmpty(path))
                return;

            AnimationFlowAsset flowAsset = AssetDatabase.LoadAssetAtPath<AnimationFlowAsset>(path);
            bool newAsset = flowAsset == null;
            if (newAsset)
            {
                flowAsset = CreateInstance<AnimationFlowAsset>();
            }
            else
            {
                // Clear existing data if overwriting
                flowAsset.States.Clear();
                flowAsset.Transitions.Clear();
            }

            // Populate States
            foreach (Node viewNode in _graphView.nodes.ToList())
            {
                if (viewNode is AnimationStateNode stateNode)
                {
                    AnimationStateData stateData = new()
                    {
                        Id = stateNode.ID,
                        StateType = stateNode.StateType,
                        AnimationName = stateNode.AnimationName,
                        IsInitialState = stateNode.IsInitialState,
                        Position = stateNode.GetPosition().position,
                        FrameToHold = stateNode.FrameToHold
                    };

                    flowAsset.States.Add(stateData);
                }
            }

            // Populate Transitions
            foreach (Edge edge in _graphView.edges.ToList())
            {
                if (edge.output.node is AnimationStateNode outputNode &&
                    edge.input.node is AnimationStateNode inputNode)
                {
                    string edgeId = EdgeConditionManager.Instance.GetEdgeId(edge);
                    var conditions = new List<ConditionData>();

                    // Get conditions either from EdgeConditionManager or from AnimationFlowEdge
                    if (!string.IsNullOrEmpty(edgeId))
                    {
                        conditions = EdgeConditionManager.Instance.GetConditions(edgeId);
                    }
                    else if (edge is AnimationFlowEdge flowEdge)
                    {
                        conditions = new List<ConditionData>(flowEdge.Conditions);
                    }

                    TransitionData transitionData = new()
                    {
                        FromStateId = outputNode.ID,
                        ToStateId = inputNode.ID,
                        Conditions = conditions
                    };

                    flowAsset.Transitions.Add(transitionData);
                }
            }

            if (newAsset)
            {
                AssetDatabase.CreateAsset(flowAsset, path);
            }
            else
            {
                EditorUtility.SetDirty(flowAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Set current asset reference
            _currentAsset = flowAsset;

            // Remember this asset path for domain reload
            _lastOpenedAssetPath = path;

            // Update window title to show current asset name
            titleContent.text = $"Animation Flow: {Path.GetFileNameWithoutExtension(path)}";

            // Show a brief status message in the status bar instead of dialog
            ShowNotification(new GUIContent($"Saved to {Path.GetFileName(path)}"));
        }

        private void LoadGraph()
        {
            if (_graphView == null)
            {
                EditorUtility.DisplayDialog("Error", "GraphView is not available.", "OK");
                return;
            }

            // Open the Object Picker window instead of the file browser
            EditorGUIUtility.ShowObjectPicker<AnimationFlowAsset>(null, false, "",
                EditorGUIUtility.GetControlID(FocusType.Passive));

            EditorApplication.update += WaitForObjectPicker;
        }

        private void WaitForObjectPicker()
        {
            if (Event.current != null && Event.current.commandName == "ObjectSelectorClosed")
            {
                Object selectedObject = EditorGUIUtility.GetObjectPickerObject();
                if (selectedObject is AnimationFlowAsset flowAsset)
                {
                    LoadAsset(flowAsset);
                }

                EditorApplication.update -= WaitForObjectPicker;
            }
        }

        public void LoadAsset(AnimationFlowAsset flowAsset)
        {
            if (_graphView == null)
            {
                Debug.LogError("GraphView is not available.");
                return;
            }

            if (flowAsset == null)
            {
                Debug.LogError("Invalid Animation Flow Asset");
                return;
            }

            _currentAsset = flowAsset;

            // Remember this asset for domain reload
            string assetPath = AssetDatabase.GetAssetPath(flowAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                _lastOpenedAssetPath = assetPath;
            }

            // Clear current graph and condition manager
            _graphView.ClearGraph();
            EdgeConditionManager.Instance.Clear();

            // Create a dictionary to map loaded node IDs to graph nodes for connecting edges
            var graphNodes = new Dictionary<string, AnimationStateNode>();

            // Load States
            foreach (AnimationStateData stateData in flowAsset.States)
            {
                AnimationStateNode node = _graphView.CreateStateNode(
                    stateData.StateType,
                    stateData.AnimationName,
                    new Rect(stateData.Position, new Vector2(150, 200)), // Update size to match node creation
                    stateData.Id,
                    stateData.IsInitialState,
                    stateData.FrameToHold
                );

                graphNodes[stateData.Id] = node;

                // Set frame to hold for HoldFrame state type
                if (stateData.StateType == "HoldFrame")
                {
                    node.FrameToHold = stateData.FrameToHold;
                    node.RefreshFrameToHoldField();
                }

                // Set initial state
                if (stateData.IsInitialState)
                {
                    node.IsInitialState = true;
                    node.RefreshInitialStateToggle();
                }
            }

            // Load Transitions
            foreach (TransitionData transitionData in flowAsset.Transitions)
            {
                if (graphNodes.TryGetValue(transitionData.FromStateId, out AnimationStateNode fromNode) &&
                    graphNodes.TryGetValue(transitionData.ToStateId, out AnimationStateNode toNode))
                {
                    // Get output port from the source node
                    Port outputPort = (Port)fromNode.outputContainer[0];

                    // Get input port from the target node
                    Port inputPort = (Port)toNode.inputContainer[0];

                    // Create edge
                    Edge edge = _graphView.ConnectPorts(outputPort, inputPort);

                    // Store conditions for this edge
                    string edgeId = EdgeConditionManager.Instance.GetEdgeId(edge);
                    if (!string.IsNullOrEmpty(edgeId) && transitionData.Conditions != null)
                    {
                        EdgeConditionManager.Instance.SetConditions(edgeId, transitionData.Conditions);
                    }
                }
            }

            // Update window title to show current asset name
            titleContent.text = $"Animation Flow: {flowAsset.name}";

            // Show a brief status message in the status bar instead of dialog
            ShowNotification(new GUIContent($"Loaded {flowAsset.name}"));

            // Frame the entire graph to show all nodes
            _graphView.FrameAll();
        }
    }
}
