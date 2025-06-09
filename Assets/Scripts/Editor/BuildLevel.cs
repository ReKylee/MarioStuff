using System;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;
using UnityEditor;
using UnityEngine;

namespace Editor
{
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

                if (Json.Deserialize(jsonData) is not Dictionary<string, object> gameData)
                    return;

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
                            case "1": CreateGameObject("Pre_Transformation", i, height, width); break;
                            case "2": CreateGameObject("Pre_Coin", i, height, width); break;
                            case "4": CreateGameObject("Pre_Flower", i, height, width); break;
                            case "5": CreateGameObject("Pre_Door", i, height, width); break;
                            case "6": CreateGameObject("Pre_PickableAxe", i, height, width); break;
                            case "7": CreateGameObject("Pre_Key", i, height, width); break;
                            case "8": CreateGameObject("Pre_1UP", i, height, width); break;
                            case "9": CreateGameObject("Pre_Goom", i, height, width); break;
                            case "10": CreateGameObject("Pre_BuffMario", i, height, width); break;
                            case "11": CreateGameObject("MovingPlatform", i, height, width); break;
                            case "12": CreateGameObject("DisappearingPlatform", i, height, width); break;
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
            GameObject temp = Instantiate(Resources.Load("Prefabs/" + prefabName)) as GameObject;
            if (!temp)
            {
                Debug.LogError("Prefab not found: Prefabs/" + prefabName);
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
}
