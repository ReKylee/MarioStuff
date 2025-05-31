#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kirby.Abilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kirby.Editor
{
    [CustomEditor(typeof(CopyAbilityData))]
    public class CopyAbilityDataEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, bool> _abilityFoldouts = new();
        private readonly Dictionary<Object, UnityEditor.Editor> _cachedEditors = new(); // Cache for embedded editors
        private SerializedProperty _abilitiesListProperty;
        private string[] _availableAbilityTypeNames;
        private List<Type> _availableAbilityTypes;
        private ReorderableModifierList _modifierList;
        private int _selectedAbilityTypeIndex;
        private SerializedProperty _statModifiersProperty;

        private void OnEnable()
        {
            _abilitiesListProperty = serializedObject.FindProperty("Abilities");
            _statModifiersProperty = serializedObject.FindProperty("statModifiers");
            _modifierList = new ReorderableModifierList(serializedObject, _statModifiersProperty);

            RefreshAvailableAbilityTypes();
        }

        private void OnDisable() // Clean up cached editors
        {
            foreach (UnityEditor.Editor editorEntry in _cachedEditors.Values)
            {
                if (editorEntry != null)
                {
                    DestroyImmediate(editorEntry);
                }
            }

            _cachedEditors.Clear();
        }

        private void RefreshAvailableAbilityTypes()
        {
            _availableAbilityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(AbilityModuleBase)) && !type.IsAbstract && type.IsPublic)
                .ToList();

            _availableAbilityTypeNames = _availableAbilityTypes.Select(type => type.Name).ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CopyAbilityData copyAbilityData = (CopyAbilityData)target;

            DrawPropertiesExcluding(serializedObject, "m_Script", "statModifiers", "Abilities");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Stat Modifiers", EditorStyles.boldLabel);

            // Use our modern reorderable list instead of the old StatModifierEditorUtility
            _modifierList.DoLayoutList();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Contained Abilities", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box); // Box for the abilities list
            EditorGUILayout.Space(5);

            var currentAbilityObjects = new List<Object>();
            for (int i = 0; i < _abilitiesListProperty.arraySize; i++)
            {
                SerializedProperty abilityRefProperty = _abilitiesListProperty.GetArrayElementAtIndex(i);
                currentAbilityObjects.Add(abilityRefProperty.objectReferenceValue);
                AbilityModuleBase abilityModuleInstance = abilityRefProperty.objectReferenceValue as AbilityModuleBase;

                EditorGUILayout.BeginHorizontal();

                if (abilityModuleInstance != null)
                {
                    string foldoutKey = $"Ability_{abilityModuleInstance.GetInstanceID()}";
                    _abilityFoldouts.TryGetValue(foldoutKey, out bool isFoldout);

                    bool newFoldoutState = EditorGUILayout.Foldout(isFoldout,
                        $"{abilityModuleInstance.name} ({abilityModuleInstance.GetType().Name})", // Display name and type
                        true, EditorStyles.foldoutHeader);

                    if (newFoldoutState != isFoldout)
                    {
                        _abilityFoldouts[foldoutKey] = newFoldoutState;
                    }

                    // Remove Button
                    if (GUILayout.Button(new GUIContent("X", "Remove Ability"), GUILayout.Width(30),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        Object objToRemove = abilityRefProperty.objectReferenceValue;
                        if (objToRemove != null &&
                            _cachedEditors.TryGetValue(objToRemove, out UnityEditor.Editor cachedEditor))
                        {
                            DestroyImmediate(cachedEditor);
                            _cachedEditors.Remove(objToRemove);
                        }

                        abilityRefProperty.objectReferenceValue = null;
                        _abilitiesListProperty.DeleteArrayElementAtIndex(i);
                        EditorUtility.SetDirty(copyAbilityData);
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        GUIUtility.ExitGUI(); // Exit GUI to prevent errors after list modification
                    }

                    EditorGUILayout.EndHorizontal(); // End horizontal for foldout and remove button

                    if (newFoldoutState)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box); // Box for embedded editor
                        EditorGUI.indentLevel++;
                        if (!_cachedEditors.TryGetValue(abilityModuleInstance, out UnityEditor.Editor abilityEditor) ||
                            !abilityEditor)
                        {
                            abilityEditor = CreateEditor(abilityModuleInstance);
                            _cachedEditors[abilityModuleInstance] = abilityEditor;
                        }

                        abilityEditor.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                }
                else // abilityInstance is null, slot is empty
                {
                    // Show the property field to allow assigning a new ability asset
                    EditorGUILayout.PropertyField(abilityRefProperty, GUIContent.none, GUILayout.ExpandWidth(true));

                    // Remove button for the empty slot
                    if (GUILayout.Button(new GUIContent("X", "Remove Slot"), GUILayout.Width(30),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        // If there was an object (e.g. user just cleared it but hasn't removed slot)
                        Object objToRemove = abilityRefProperty.objectReferenceValue;
                        if (objToRemove != null &&
                            _cachedEditors.TryGetValue(objToRemove, out UnityEditor.Editor cachedEditor))
                        {
                            DestroyImmediate(cachedEditor);
                            _cachedEditors.Remove(objToRemove);
                        }

                        abilityRefProperty.objectReferenceValue = null;
                        _abilitiesListProperty.DeleteArrayElementAtIndex(i);
                        EditorUtility.SetDirty(copyAbilityData);
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        GUIUtility.ExitGUI(); // Exit GUI to prevent errors after list modification
                    }

                    EditorGUILayout.EndHorizontal(); // End horizontal for property field and remove button
                }

                if (i < _abilitiesListProperty.arraySize - 1) EditorGUILayout.Separator();
                EditorGUILayout.Space(2);
            }

            // Clean up cached editors for abilities that are no longer in the list
            var keysToRemove = _cachedEditors.Keys.Where(k => !currentAbilityObjects.Contains(k)).ToList();
            foreach (Object key in keysToRemove)
            {
                if (_cachedEditors.TryGetValue(key, out UnityEditor.Editor cachedEditor))
                {
                    DestroyImmediate(cachedEditor);
                }

                _cachedEditors.Remove(key);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical(); // End box for abilities list
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Add New Ability", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box); // Box for add new ability section
            EditorGUILayout.Space(5);
            if (_availableAbilityTypes.Count > 0)
            {
                _selectedAbilityTypeIndex = EditorGUILayout.Popup("Ability Type", _selectedAbilityTypeIndex,
                    _availableAbilityTypeNames);

                if (GUILayout.Button("Create and Add New Ability"))
                {
                    CreateAndAddAbility(copyAbilityData, _availableAbilityTypes[_selectedAbilityTypeIndex]);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No AbilityBase subclasses found in the project.", MessageType.Info);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical(); // End box for add new ability section

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateAndAddAbility(CopyAbilityData owner, Type abilityType)
        {
            // Method implementation unchanged
            AbilityModuleBase
                newAbilityModuleInstance =
                    (AbilityModuleBase)CreateInstance(abilityType);

            newAbilityModuleInstance.name =
                string.Format("{0}_For_{1}", abilityType.Name, owner.name.Replace(" ", ""));

            string ownerPath = AssetDatabase.GetAssetPath(owner);
            string directory;
            if (string.IsNullOrEmpty(ownerPath))
            {
                directory = "Assets/Abilities_Generated";
                // Ensure this specific directory is created if it's the one being used.
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            else
            {
                string abilitiesFolderName = string.Format("{0}_Abilities", owner.name.Replace(" ", ""));
                directory = Path.Combine(Path.GetDirectoryName(ownerPath) ?? string.Empty, abilitiesFolderName);
            }

            // General directory creation for the final path, handles the case where Path.Combine might create a new root if base is empty.
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string abilityAssetName = string.Format("{0}.asset", newAbilityModuleInstance.name);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, abilityAssetName));

            AssetDatabase.CreateAsset(newAbilityModuleInstance, assetPath);
            AssetDatabase.SaveAssets();

            _abilitiesListProperty.InsertArrayElementAtIndex(_abilitiesListProperty.arraySize);
            SerializedProperty newElement =
                _abilitiesListProperty.GetArrayElementAtIndex(_abilitiesListProperty.arraySize - 1);

            newElement.objectReferenceValue = newAbilityModuleInstance;

            EditorUtility.SetDirty(owner);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
