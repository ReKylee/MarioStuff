using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Animation.Flow.Adapters;
using GabrielBigardi.SpriteAnimator;
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
        // Now marked with [SerializeField] to persist between editor sessions
        private static string _lastOpenedAssetPath;
        private AnimationFlowAsset _currentAsset;

        private AnimationFlowGraphView _graphView;

        // Flag to track if the graph has unsaved changes
        private bool _hasUnsavedChanges;

        // Store the animator for accessing animation names
        private IAnimator _targetAnimator;

        // Target GameObject reference for getting animations
        private GameObject _targetGameObject;

        // Static constructor for InitializeOnLoad
        static AnimationFlowEditorWindow()
        {
            // Register for domain reload event handling
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnEnable()
        {
            // Create a root container with vertical layout
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // Create a container that will take up all available space
            VisualElement contentContainer = new();
            contentContainer.style.flexGrow = 1;
            rootVisualElement.Add(contentContainer);

            // Construct graph view (will be added to content container)
            ConstructGraphView(contentContainer);

            // Generate toolbar (will be added to root)
            GenerateToolbar();

            // Try to reopen the last asset if available
            if (!string.IsNullOrEmpty(_lastOpenedAssetPath))
            {
                AnimationFlowAsset asset = AssetDatabase.LoadAssetAtPath<AnimationFlowAsset>(_lastOpenedAssetPath);
                if (asset)
                {
                    LoadAsset(asset);
                }
            }

            // Subscribe to editor closing to show save prompt if needed
            EditorApplication.wantsToQuit += WantsToQuit;

            // Listen for selection changes to update target GameObject
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            // Remove _graphView from its parent container instead of directly from rootVisualElement
            if (_graphView is { parent: not null })
            {
                _graphView.parent.Remove(_graphView);
            }

            EditorApplication.wantsToQuit -= WantsToQuit;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private bool WantsToQuit()
        {
            // If there are unsaved changes, prompt to save
            if (_hasUnsavedChanges && _currentAsset != null)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Unsaved Changes",
                    $"The Animation Flow '{_currentAsset.name}' has unsaved changes. Do you want to save them?",
                    "Save", "Don't Save", "Cancel");

                switch (choice)
                {
                    case 0: // Save
                        SaveCurrentAsset();
                        return true;
                    case 1: // Don't Save
                        return true;
                    case 2: // Cancel
                        return false;
                }
            }

            return true;
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
            // Create the window as a tab, docked in the center area by default
            AnimationFlowEditorWindow window = GetWindow<AnimationFlowEditorWindow>(
                "Animation Flow Editor",
                false,
                typeof(SceneView));

            window.minSize = new Vector2(800, 600);
            window.Focus();
        }

        private void ConstructGraphView(VisualElement parent)
        {
            _graphView = new AnimationFlowGraphView
            {
                name = "Animation Flow Graph"
            };

            _graphView.StretchToParentSize();
            parent.Add(_graphView);

            // Mark graph as modified when changes occur
            _graphView.graphViewChanged += change =>
            {
                if (change.edgesToCreate != null && change.edgesToCreate.Count > 0 ||
                    change.movedElements != null && change.movedElements.Count > 0 ||
                    change.elementsToRemove != null && change.elementsToRemove.Count > 0)
                {
                    _hasUnsavedChanges = true;
                }

                return change;
            };
        }

        private void GenerateToolbar()
        {
            Toolbar toolbar = new();

            Button saveButton = new(SaveCurrentAsset)
            {
                text = "Save"
            };

            toolbar.Add(saveButton);

            Button saveAsButton = new(SaveGraphAs)
            {
                text = "Save As..."
            };

            toolbar.Add(saveAsButton);

            Button loadButton = new(LoadGraph)
            {
                text = "Load"
            };

            toolbar.Add(loadButton);

            toolbar.style.height = 30;

            rootVisualElement.Add(toolbar);
        }

        private void SaveCurrentAsset()
        {
            if (_currentAsset == null)
            {
                SaveGraphAs();
                return;
            }

            SaveToAsset(_currentAsset);
        }

        private void SaveGraphAs()
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

            SaveToAsset(flowAsset);

            if (newAsset)
            {
                AssetDatabase.CreateAsset(flowAsset, path);
            }

            // Set current asset reference
            _currentAsset = flowAsset;

            // Remember this asset path for domain reload
            _lastOpenedAssetPath = path;

            // Update window title to show current asset name
            UpdateWindowTitle();

            // Show a brief status message in the status bar instead of dialog
            ShowNotification(new GUIContent($"Saved to {Path.GetFileName(path)}"));
        }

        private void SaveToAsset(AnimationFlowAsset flowAsset)
        {
            if (flowAsset == null || _graphView == null) return;

            // Record the asset for undo operations
            Undo.RecordObject(flowAsset, "Save Animation Flow");

            // Clear existing data if overwriting
            flowAsset.States.Clear();
            flowAsset.Transitions.Clear();

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

            // Mark the asset as dirty but don't force an immediate save
            EditorUtility.SetDirty(flowAsset);

            // Use a less aggressive save method that doesn't cause domain reloads
            AssetDatabase.SaveAssetIfDirty(flowAsset);

            // Reset unsaved changes flag
            _hasUnsavedChanges = false;

            // Show a brief notification instead of dialog
            ShowNotification(new GUIContent($"Saved {flowAsset.name}"));
        }

        private void LoadGraph()
        {
            if (_graphView == null)
            {
                EditorUtility.DisplayDialog("Error", "GraphView is not available.", "OK");
                return;
            }

            // Check for unsaved changes
            if (_hasUnsavedChanges && _currentAsset != null)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Unsaved Changes",
                    $"The Animation Flow '{_currentAsset.name}' has unsaved changes. Do you want to save them?",
                    "Save", "Don't Save", "Cancel");

                switch (choice)
                {
                    case 0: // Save
                        SaveCurrentAsset();
                        break;
                    case 1: // Don't Save
                        break;
                    case 2: // Cancel
                        return;
                }
            }

            // Open the Object Picker window instead of the file browser
            EditorGUIUtility.ShowObjectPicker<AnimationFlowAsset>(null, false, "",
                GUIUtility.GetControlID(FocusType.Passive));

            EditorApplication.update += WaitForObjectPicker;
        }

        private void WaitForObjectPicker()
        {
            if (Event.current is { commandName: "ObjectSelectorClosed" })
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
            if (_graphView is null)
            {
                Debug.LogError("GraphView is not available.");
                return;
            }

            if (!flowAsset)
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
            UpdateWindowTitle();

            // Show a brief status message in the status bar instead of dialog
            ShowNotification(new GUIContent($"Loaded {flowAsset.name}"));

            // Frame the entire graph to show all nodes
            _graphView.FrameAll();

            // Reset unsaved changes flag since we just loaded
            _hasUnsavedChanges = false;
        }

        private void UpdateWindowTitle()
        {
            titleContent.text =
                _currentAsset ? $"Animation Flow: {_currentAsset.name}" : "Animation Flow Editor";
        }

        private void OnSelectionChanged()
        {
            // Update target GameObject when selection changes
            UpdateTargetGameObject();
        }

        private void UpdateTargetGameObject()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject)
            {
                // Check if the selected object has an AnimationFlowController
                AnimationFlowController flowController = selectedObject.GetComponent<AnimationFlowController>();
                if (flowController)
                {
                    _targetGameObject = selectedObject;

                    // Try to get the animator through reflection
                    MethodInfo methodInfo = flowController.GetType().GetMethod("GetAnimatorAdapter",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (methodInfo != null)
                    {
                        _targetAnimator = methodInfo.Invoke(flowController, null) as IAnimator;
                    }

                    // If we have a graph view, notify it about the target change
                    _graphView?.OnTargetGameObjectChanged(_targetGameObject, _targetAnimator);
                }
                else
                {
                    // Try to get a SpriteAnimator component directly
                    SpriteAnimator spriteAnimator = selectedObject.GetComponent<SpriteAnimator>();
                    if (spriteAnimator)
                    {
                        _targetGameObject = selectedObject;
                        _targetAnimator = new SpriteAnimatorAdapter(spriteAnimator);

                        // If we have a graph view, notify it about the target change
                        _graphView?.OnTargetGameObjectChanged(_targetGameObject, _targetAnimator);
                    }
                }
            }
        }

        // Getter methods for target info
        public GameObject GetTargetGameObject() => _targetGameObject;
        public IAnimator GetTargetAnimator() => _targetAnimator;
    }
}
