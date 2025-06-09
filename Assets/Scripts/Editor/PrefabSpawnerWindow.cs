using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabSpawnerWindow : EditorWindow
{
    private static bool _isSpawningEnabled = true;

    private readonly string[] _dropDownOptions =
    {
        "Prefab_Flower",
        "Prefab_Star",
        "Prefab_Mario",
        "Prefab_Floor",
        "Prefab_Coin",
        "Prefab_Spikes"
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
        if (_isSpawningEnabled && _prefabDictionary != null &&
            _prefabDictionary.ContainsKey(_dropDownOptions[_selectedIndex]))
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 1)
            {
                Debug.Log("_selectedIndex " + _selectedIndex);
                Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                Vector3 mouseWorldPos = ray.origin;
                Vector3 mouseWorldPosRounded =
                    new(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y), 0);

                Instantiate(_prefabDictionary[_dropDownOptions[_selectedIndex]],
                    mouseWorldPosRounded, Quaternion.identity);

                Debug.Log("Mouse Position in Scene " + mouseWorldPosRounded);
            }
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
            _prefabDictionary.Add(n, Resources.Load<GameObject>("Tiles/" + n));
        }
    }
}
