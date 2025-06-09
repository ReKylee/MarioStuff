using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildLevel : EditorWindow
{
    private TextAsset _curLevel;
    private GameObject _world;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Assign Level File:");
        _curLevel = EditorGUILayout.ObjectField(_curLevel, typeof(TextAsset), false) as TextAsset;

        EditorGUILayout.LabelField("Assign Parent Transform");
        _world = EditorGUILayout.ObjectField(_world, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Create Level") && _curLevel)
        {
            CreateLevel();
        }
    }

    [MenuItem("Tools/Level Creator")]
    public static void ShowWindow()
    {
        GetWindow<BuildLevel>("Level Creator");
    }

    private void CreateLevel()
    {
        Debug.Log("Creating level... " + _curLevel.name);
        try
        {
            string jsonData = _curLevel.text;
            var gameData = JsonUtility.FromJson<Dictionary<string, object>>(jsonData);
            int height = int.Parse(gameData["height"].ToString());
            int width = int.Parse(gameData["width"].ToString());

            var layers = (List<object>)gameData["layers"];
            foreach (var levelTiles in layers.Cast<Dictionary<string, object>>()
                         .Where(layerData => layerData.ContainsKey("data"))
                         .Select(layerData => (List<object>)layerData["data"]))
            {
                for (int i = 0; i < levelTiles.Count; i++)
                {
                    switch (levelTiles[i].ToString())
                    {
                        case "1": CreateGameObject("Prefab_Flower", i, height, width); break;
                        case "2": CreateGameObject("Prefab_Star", i, height, width); break;
                        case "3": CreateGameObject("Prefab_Mario", i, height, width); break;
                        case "4": CreateGameObject("Prefab_Floor", i, height, width); break;
                        case "5": CreateGameObject("Prefab_Coin", i, height, width); break;
                        case "6": CreateGameObject("Prefab_Spikes", i, height, width); break;
                    }
                }
            }

            Debug.Log("Height: " + height + ", Width: " + width);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void CreateGameObject(string prefabName, int index, int height, int width)
    {
        GameObject temp = Instantiate(Resources.Load("Tiles/" + prefabName)) as GameObject;
        if (!temp)
        {
            Debug.LogError("Prefab not found: Tiles/" + prefabName);
            return;
        }

        int colCalc = index % width;
        string col = colCalc.ToString();
        if (colCalc < 10)
            col = "0" + colCalc;

        int rowCalc = height - 1 - index / width;
        string row = rowCalc.ToString();
        if (rowCalc < 10)
            row = "0" + rowCalc;

        temp.name = row + col;
        temp.transform.localPosition = new Vector3(colCalc, rowCalc, 0);

        if (_world)
            temp.transform.SetParent(_world.transform);
    }
}
