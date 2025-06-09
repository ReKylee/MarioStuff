using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PrefabSpawnerWindow : EditorWindow
    {
        private static bool _isSpawningEnabled = true;

        private readonly string[] _dropDownOptions =
        {
            "Pre_Flower",
            "Pre_Coin",
            "Pre_1UP",
            "Pre_Door",
            "Pre_Key",
            "Pre_Transformation",
            "Pre_PickableAxe",
            "Pre_Goom",
            "Pre_BuffMario",
            "MovingPlatform",
            "DisappearingPlatform"
        };

        private GUIStyle _labelStyle;
        private Dictionary<string, GameObject> _prefabDictionary;
        private int _selectedIndex;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            _labelStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.white
                }
            };

            if (_prefabDictionary == null || _prefabDictionary.Count == 0)
                LoadPrefabs();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            _selectedIndex = EditorGUILayout.Popup("Select Option", _selectedIndex, _dropDownOptions);
            EditorGUILayout.Space();

            if (GUILayout.Button("Toggle Prefab Spawning"))
                TogglePrefabSpawning();

            GUILayout.Label(
                "Prefab Spawning Status: " +
                (_isSpawningEnabled ? "<color=yellow>Enabled</color>" : "<color=red>Disabled</color>"), _labelStyle);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Instructions:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Right-click to place the selected prefab");
            EditorGUILayout.LabelField("• Hold Shift + Right-click to delete a prefab");
        }
        [MenuItem("Tools/Prefab Spawner")]
        public static void ShowWindow()
        {
            PrefabSpawnerWindow window = GetWindow<PrefabSpawnerWindow>();
            window.titleContent = new GUIContent("Prefab Spawner");
            window.Show();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_isSpawningEnabled || _prefabDictionary == null) return;

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 1)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);

                if (current.shift)
                {
                    DeletePrefabAtMousePosition(ray);
                    current.Use();
                }
                else if (_prefabDictionary.ContainsKey(_dropDownOptions[_selectedIndex]))
                {
                    Vector3 mouseWorldPos = ray.origin;
                    Vector3 mouseWorldPosRounded =
                        new(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y), 0);

                    Instantiate(_prefabDictionary[_dropDownOptions[_selectedIndex]],
                        mouseWorldPosRounded, Quaternion.identity);

                    Debug.Log("Prefab created at: " + mouseWorldPosRounded);
                    current.Use();
                }
            }
        }

        private void DeletePrefabAtMousePosition(Ray ray)
        {
            // Perform a raycast to find objects at the mouse position
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider)
            {
                GameObject hitObject = hit.collider.gameObject;


                // Get the root game object in case the collider is on a child
                GameObject rootObject = hitObject.transform.root.gameObject;

                // Check if this object's name starts with any of our prefab names
                bool isPrefabFromDictionary =
                    _dropDownOptions.Any(prefabName => rootObject.name.StartsWith(prefabName));

                if (isPrefabFromDictionary)
                {
                    Undo.RecordObject(rootObject, "Delete Prefab");
                    Debug.Log("Deleted prefab: " + rootObject.name);
                    DestroyImmediate(rootObject);
                }
                else
                {
                    Debug.Log("Object is not a spawnable prefab: " + rootObject.name);
                }
            }
            else
            {
                Debug.Log("No prefab found at this position");
            }
        }

        private static void TogglePrefabSpawning()
        {
            _isSpawningEnabled = !_isSpawningEnabled;
        }

        private void LoadPrefabs()
        {
            _prefabDictionary = new Dictionary<string, GameObject>();
            foreach (string n in _dropDownOptions)
            {
                _prefabDictionary.Add(n, Resources.Load<GameObject>("Prefabs/" + n));
            }
        }
    }
}
