using System;
using System.Collections.Generic;
using System.IO;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Core;
using Animation.Flow.Editor.Managers;
using Animation.Flow.Interfaces;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Animation.Flow.Editor
{
    // Add EditorOnly tag to ensure code stripping in builds
    [InitializeOnLoad]
    public class AnimationFlowEditorWindow : EditorWindow
    {
        // Static field to remember the last opened asset between domain reloads
        // Now marked with [SerializeField] to persist between editor sessions
        private static string _lastOpenedAssetPath;

        // Auto-save feature
        [SerializeField] private bool autoSaveEnabled;
        private Button _autoSaveButton;
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

            // No timer needed for auto-save as it now happens on graph changes
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
            if (_hasUnsavedChanges && _currentAsset)
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
            if (_currentAsset && _graphView != null)
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

            // Also listen for changes to node values, not just structure
            _graphView.RegisterCallback<GeometryChangedEvent>(_ => OnGraphElementChanged());
            _graphView.nodeCreationRequest += _ => OnGraphElementChanged();
            parent.Add(_graphView);

            // Mark graph as modified when changes occur
            _graphView.graphViewChanged += change =>
            {
                if (change.edgesToCreate is { Count: > 0 } ||
                    change.movedElements is { Count: > 0 } ||
                    change.elementsToRemove is { Count: > 0 })
                {
                    _hasUnsavedChanges = true;

                    // Auto-save immediately if enabled
                    if (autoSaveEnabled && _currentAsset != null)
                    {
                        // Wait a frame to ensure graph changes are applied
                        EditorApplication.delayCall += () =>
                        {
                            SaveToAsset(_currentAsset);
                            ShowNotification(new GUIContent("Auto-saved"));
                            Debug.Log(
                                $"[Auto-Save] Graph change detected - Saved {_currentAsset.name} at {DateTime.Now}");
                        };
                    }
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

            // Add spacer element
            VisualElement spacer = new();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            // Auto-save toggle button
            Button autoSaveButton = new(ToggleAutoSave)
            {
                text = autoSaveEnabled ? "Auto-Save: ON" : "Auto-Save: OFF",
                name = "AutoSaveButton" // Set a name for the button for easier querying if needed
            };

            // Style the auto-save button
            autoSaveButton.style.marginRight = 10;
            if (autoSaveEnabled)
            {
                autoSaveButton.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            }
            else
            {
                autoSaveButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
            }

            // Add a manual save button
            Button manualSaveButton = new(() =>
            {
                if (_currentAsset != null)
                {
                    SaveToAsset(_currentAsset);
                    // We can do a full save here since it's explicitly requested
                    AssetDatabase.SaveAssetIfDirty(_currentAsset);
                    ShowNotification(new GUIContent("Saved"));
                }
            })
            {
                text = "Save Now"
            };

            manualSaveButton.style.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 0.6f);

            toolbar.Add(autoSaveButton);

            // Add manual save button to toolbar
            toolbar.Add(manualSaveButton);

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
            bool newAsset = !flowAsset;
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
            if (!flowAsset || _graphView is null) return;

            // Record the asset for undo operations
            Undo.RecordObject(flowAsset, "Save Animation Flow");

            // Clear existing data if overwriting
            flowAsset.states.Clear();
            flowAsset.transitions.Clear();

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
                        Position = stateNode.GetPosition().position
                    };

                    flowAsset.states.Add(stateData);
                }
            }

            // Populate Transitions
            foreach (Edge edge in _graphView.edges.ToList())
            {
                if (edge.output.node is AnimationStateNode outputNode &&
                    edge.input.node is AnimationStateNode inputNode)
                {
                    string edgeId = EdgeConditionManager.GetEdgeId(edge);
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

                    FlowTransition flowTransition = new()
                    {
                        FromStateId = outputNode.ID,
                        ToStateId = inputNode.ID,
                        Conditions = conditions
                    };

                    flowAsset.transitions.Add(flowTransition);
                }
            }

            // Mark the asset as dirty but don't force an immediate save
            EditorUtility.SetDirty(flowAsset);

            // Use a less aggressive save method that doesn't cause domain reloads
            AssetDatabase.SaveAssetIfDirty(flowAsset);

            // For auto-save, ensure the asset is actually written to disk
            if (autoSaveEnabled)
            {
                EditorUtility.SetDirty(_currentAsset);
                AssetDatabase.Refresh();
            }

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
                Debug.LogError("[Animation Flow Editor] GraphView is not available.");
                return;
            }

            if (!flowAsset)
            {
                Debug.LogError("[Animation Flow Editor] Invalid Animation Flow Asset");
                return;
            }

            Debug.Log($"[Animation Flow Editor] Loading asset: {flowAsset.name}");
            _currentAsset = flowAsset;

            // Remember this asset for domain reload
            string assetPath = AssetDatabase.GetAssetPath(flowAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                _lastOpenedAssetPath = assetPath;
            }

            // Find objects using this asset and update target animator
            Debug.Log("[Animation Flow Editor] Attempting to find objects using this asset...");
            bool foundController = FindObjectsUsingAsset(flowAsset);
            Debug.Log($"[Animation Flow Editor] Found controller: {foundController}");

            if (!foundController)
            {
                // Fallback to current selection if no controller is found
                Debug.Log("[Animation Flow Editor] No controller found, falling back to selection...");
                UpdateTargetGameObject();
            }

            // Log animation sources
            if (_targetAnimator != null)
            {
                Debug.Log($"[Animation Flow Editor] Target animator found: {_targetAnimator.GetType().Name}");
                var animations = _targetAnimator.GetAvailableAnimations();
                Debug.Log($"[Animation Flow Editor] Available animations from target: {string.Join(", ", animations)}");
            }
            else
            {
                Debug.LogWarning("[Animation Flow Editor] No target animator found!");
            }

            // Clear current graph and condition manager
            _graphView.ClearGraph();
            EdgeConditionManager.Instance.Clear();

            // Create a dictionary to map loaded node IDs to graph nodes for connecting edges
            var graphNodes = new Dictionary<string, AnimationStateNode>();

            // Load States
            foreach (AnimationStateData stateData in flowAsset.states)
            {
                AnimationStateNode node = _graphView.CreateStateNode(
                    stateData.StateType,
                    stateData.AnimationName,
                    new Rect(stateData.Position, new Vector2(150, 200)), // Update size to match node creation
                    stateData.Id,
                    stateData.IsInitialState
                );

                graphNodes[stateData.Id] = node;


                // Set initial state
                if (stateData.IsInitialState)
                {
                    node.IsInitialState = true;
                    node.RefreshInitialStateToggle();
                }
            }

            // Load Transitions
            foreach (FlowTransition transitionData in flowAsset.transitions)
            {
                if (!graphNodes.TryGetValue(transitionData.FromStateId, out AnimationStateNode fromNode) ||
                    !graphNodes.TryGetValue(transitionData.ToStateId, out AnimationStateNode toNode))
                    continue;

                // Get output port from the source node
                Port outputPort = (Port)fromNode.outputContainer[0];

                // Get input port from the target node
                Port inputPort = (Port)toNode.inputContainer[0];

                // Create edge
                Edge edge = _graphView.ConnectPorts(outputPort, inputPort);

                // Store conditions for this edge
                string edgeId = EdgeConditionManager.GetEdgeId(edge);
                if (!string.IsNullOrEmpty(edgeId) && transitionData.Conditions != null)
                {
                    EdgeConditionManager.Instance.SetConditions(edgeId, transitionData.Conditions);
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
            // First priority: try to find objects using current asset
            if (_currentAsset && TryGetAnimatorFromAsset(_currentAsset))
            {
                return; // Successfully found an object using the asset
            }

            // Second priority: try to use the selected object
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject && TryGetAnimatorFromGameObject(selectedObject))
            {
            }
        }

        /// <summary>
        ///     Tries to get an animator from a GameObject, updating the target references if successful
        /// </summary>
        private bool TryGetAnimatorFromGameObject(GameObject gameObject)
        {
            if (!gameObject)
                return false;

            _targetGameObject = gameObject;

            // First check for a flow controller
            AnimationFlowController flowController = gameObject.GetComponent<AnimationFlowController>();
            if (flowController)
            {
                _targetAnimator = flowController.GetAnimator();
                if (_targetAnimator != null)
                {
                    // If we have a graph view, notify it about the target change
                    _graphView?.OnTargetGameObjectChanged(_targetGameObject, _targetAnimator);
                    return true;
                }
            }

            // No valid animator found on this object
            return false;
        }

        /// <summary>
        ///     Tries to get an animator from the controllers using this asset
        /// </summary>
        private bool TryGetAnimatorFromAsset(AnimationFlowAsset asset)
        {
            if (!asset)
                return false;

            // Get the first controller directly from the asset
            AnimationFlowController controller = asset.GetController();
            if (!controller)
                return false;

            _targetAnimator = controller.GetAnimator();
            if (_targetAnimator != null)
            {
                // If we have a graph view, notify it about the target change
                _graphView?.OnTargetGameObjectChanged(_targetGameObject, _targetAnimator);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Legacy method, redirects to TryGetAnimatorFromAsset for backward compatibility
        /// </summary>
        private bool FindObjectsUsingAsset(AnimationFlowAsset asset) => TryGetAnimatorFromAsset(asset);

        // Getter methods for target info
        public GameObject GetTargetGameObject() => _targetGameObject;
        public IAnimator GetTargetAnimator() => _targetAnimator;


        private void ToggleAutoSave()
        {
            autoSaveEnabled = !autoSaveEnabled;

            // Update button visual state
            Button autoSaveButton =
                rootVisualElement.Q<Toolbar>().Q<Button>("AutoSaveButton");

            if (autoSaveButton != null)
            {
                autoSaveButton.text = autoSaveEnabled ? "Auto-Save: ON" : "Auto-Save: OFF";

                if (autoSaveEnabled)
                {
                    autoSaveButton.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                    // Initialize last save time when turning on
                }
                else
                {
                    autoSaveButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
                }
            }

            // Show notification
            ShowNotification(new GUIContent(autoSaveEnabled ? "Auto-Save Enabled" : "Auto-Save Disabled"));

            // If enabling, do an immediate save if there are unsaved changes
            if (autoSaveEnabled && _currentAsset && _hasUnsavedChanges)
            {
                // Save to asset without forcing database operations
                SaveToAsset(_currentAsset);

                // Just mark the asset as dirty without full reimport
                EditorUtility.SetDirty(_currentAsset);

                _hasUnsavedChanges = false;

                // Update window title to remove asterisk if it was there
                UpdateWindowTitle();
            }
        }

        // Method to handle when graph elements change (including property edits)
        private void OnGraphElementChanged()
        {
            if (_hasUnsavedChanges)
                return;

            _hasUnsavedChanges = true;

            // Auto-save if enabled
            if (autoSaveEnabled && _currentAsset)
            {
                // Slight delay to ensure all changes are processed
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        // Use more gentle save approach
                        SaveToAsset(_currentAsset);

                        // Don't force an asset database refresh
                        EditorUtility.SetDirty(_currentAsset);

                        // Only mark as saved after successful save
                        _hasUnsavedChanges = false;

                        // Only show notification for significant changes
                        ShowNotification(new GUIContent("Auto-saved"));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error during auto-save: {ex.Message}");
                    }
                };
            }
        }
    }
}
