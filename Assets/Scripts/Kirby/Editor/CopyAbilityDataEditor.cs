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
        private string _abilitySearchString = "";
        private string[] _availableAbilityTypeNames;
        private List<Type> _availableAbilityTypes;
        private ReorderableModifierList _modifierList;
        private Vector2 _scrollPosition;
        private bool _searchResultsNeedRefresh = true;
        private int _selectedAbilityTypeIndex;
        private SerializedProperty _statModifiersProperty;

        private void OnEnable()
        {
            _abilitiesListProperty = serializedObject.FindProperty("abilities");
            _statModifiersProperty = serializedObject.FindProperty("statModifiers");
            _modifierList = new ReorderableModifierList(serializedObject, _statModifiersProperty);

            RefreshAvailableAbilityTypes();
        }

        private void OnDisable() // Clean up cached editors
        {
            foreach (UnityEditor.Editor editorEntry in _cachedEditors.Values)
            {
                if (editorEntry)
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

            // Exclude "abilities" from the default inspector to hide the default serialized list
            DrawPropertiesExcluding(serializedObject, "m_Script", "statModifiers", "abilities");

            EditorGUILayout.Space(10);

            // Create a header for Stat Modifiers (without background)
            Rect statModHeaderRect = EditorGUILayout.GetControlRect(false, 28);

            // Style for header text (without custom color)
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12
            };

            statModHeaderRect.x += 10;
            statModHeaderRect.width -= 20;
            EditorGUI.LabelField(statModHeaderRect, "Stat Modifiers", headerStyle);

            EditorGUILayout.Space(5);

            // Draw the modifiers list
            GUIStyle listContainerStyle = new()
            {
                margin = new RectOffset(8, 8, 0, 8),
                padding = new RectOffset(2, 2, 2, 2)
            };

            EditorGUILayout.BeginVertical(listContainerStyle);

            // Use our modern reorderable list instead of the old StatModifierEditorUtility
            _modifierList.DoLayoutList();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Create a header for Contained Abilities (without background)
            Rect abilitiesHeaderRect = EditorGUILayout.GetControlRect(false, 28);

            abilitiesHeaderRect.x += 10;
            abilitiesHeaderRect.width -= 20;

            // Split the header rect to add the cleanup button on the right side
            Rect labelRect = new(abilitiesHeaderRect.x, abilitiesHeaderRect.y,
                abilitiesHeaderRect.width * 0.6f, abilitiesHeaderRect.height);

            Rect buttonRect = new(
                labelRect.x + labelRect.width + 10,
                abilitiesHeaderRect.y + 2,
                abilitiesHeaderRect.width * 0.4f - 10,
                EditorGUIUtility.singleLineHeight);

            // Draw the header label
            EditorGUI.LabelField(labelRect, "Contained Abilities", headerStyle);

            // Draw the cleanup button
            Color prevCleanupBtnColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.6f, 0.3f, 0.3f, 0.8f) // Darker red for dark theme
                : new Color(1f, 0.85f, 0.85f, 1f); // Light pink for light theme

            GUIContent cleanupButtonContent = new("Clean Up Unused",
                "Find and delete ability module assets that are no longer referenced by this or any other Copy Ability");

            if (GUI.Button(buttonRect, cleanupButtonContent))
            {
                CleanupUnusedAbilityModules((CopyAbilityData)target);
            }

            GUI.backgroundColor = prevCleanupBtnColor;

            EditorGUILayout.Space(5);

            // Clean container style without background for the abilities list
            GUIStyle abilitiesContainerStyle = new()
            {
                margin = new RectOffset(8, 8, 0, 8),
                padding = new RectOffset(5, 5, 5, 5)
            };

            EditorGUILayout.BeginVertical(abilitiesContainerStyle); // Box for the abilities list
            EditorGUILayout.Space(2);

            var currentAbilityObjects = new List<Object>();
            for (int i = 0; i < _abilitiesListProperty.arraySize; i++)
            {
                SerializedProperty abilityRefProperty = _abilitiesListProperty.GetArrayElementAtIndex(i);
                currentAbilityObjects.Add(abilityRefProperty.objectReferenceValue);
                AbilityModuleBase abilityModuleInstance = abilityRefProperty.objectReferenceValue as AbilityModuleBase;

                if (abilityModuleInstance)
                {
                    string foldoutKey = $"Ability_{abilityModuleInstance.GetInstanceID()}";
                    _abilityFoldouts.TryGetValue(foldoutKey, out bool isFoldout);

                    // Create a container that includes the foldout
                    GUIStyle roundedBoxStyle = new(EditorStyles.helpBox)
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(8, 8, 8, 8)
                    };

                    // Start vertical layout for entire foldout section including header
                    EditorGUILayout.BeginVertical(roundedBoxStyle);

                    // Create the main horizontal layout for the ability entry - only vertically centered
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));

                    // Add a small left margin
                    GUILayout.Space(8);

                    // Create a vertical alignment style for the foldout to match object field height
                    GUIStyle foldoutContainerStyle = new()
                    {
                        margin = new RectOffset(0, 0, 1, 0), // Small top margin to align vertically
                        padding = new RectOffset(0, 0, 0, 0)
                    };

                    EditorGUILayout.BeginVertical(foldoutContainerStyle,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    // Create foldout
                    GUIContent foldoutContent =
                        new($"{abilityModuleInstance.GetType().Name}");

                    // Use a custom EditorGUILayout.Foldout with style to make it align properly
                    GUIStyle foldoutStyle = new(EditorStyles.foldout)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(0, 0, 1, 0) // Slight adjustment to center text
                    };

                    bool newFoldoutState = EditorGUILayout.Foldout(isFoldout, foldoutContent, true, foldoutStyle);

                    if (newFoldoutState != isFoldout)
                    {
                        _abilityFoldouts[foldoutKey] = newFoldoutState;
                    }

                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    // Add GameObject field with explicit vertical alignment
                    float objectFieldWidth = EditorGUIUtility.currentViewWidth * 0.4f;
                    EditorGUI.BeginChangeCheck();

                    // Use a style to make object field align with foldout
                    GUIStyle objectFieldStyle = new()
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0)
                    };

                    EditorGUILayout.BeginVertical(objectFieldStyle);
                    Object newReference = EditorGUILayout.ObjectField(
                        abilityModuleInstance,
                        typeof(AbilityModuleBase),
                        false,
                        GUILayout.Width(objectFieldWidth));

                    EditorGUILayout.EndVertical();

                    // Add a small spacing between fields
                    GUILayout.Space(4);

                    if (EditorGUI.EndChangeCheck())
                    {
                        abilityRefProperty.objectReferenceValue = newReference;
                        EditorUtility.SetDirty(copyAbilityData);
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

                    EditorGUILayout.EndHorizontal();

                    // Display the ability's properties when folded out
                    if (newFoldoutState)
                    {

                        EditorGUILayout.Space(8);

                        // Draw the ability module inspector using the helper method
                        DrawAbilityModuleInspector(abilityModuleInstance, headerStyle);

                        // Apply any changes
                        new SerializedObject(abilityModuleInstance).ApplyModifiedProperties();

                    }

                    // End the rounded box vertical layout that wraps the entire foldout section
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
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

            // Create a header for Add New Ability (without background)
            Rect addAbilityHeaderRect = EditorGUILayout.GetControlRect(false, 28);

            addAbilityHeaderRect.x += 10;
            addAbilityHeaderRect.width -= 20;
            EditorGUI.LabelField(addAbilityHeaderRect, "Add New Ability", headerStyle);

            EditorGUILayout.Space(5);

            // Use a consistent container style WITH a background for the add ability section
            GUIStyle addAbilityContainerStyle = new(EditorStyles.helpBox)
            {
                margin = new RectOffset(8, 8, 0, 8),
                padding = new RectOffset(10, 10, 10, 10)
            };

            // Set background color to make the section stand out
            Color prevBgColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.25f, 0.28f, 1f) // Subtle darker blue for dark theme
                : new Color(0.93f, 0.95f, 1f, 1f); // Very light blue for light theme

            EditorGUILayout.BeginVertical(addAbilityContainerStyle);

            // Reset background color
            GUI.backgroundColor = prevBgColor;

            if (_availableAbilityTypes.Count > 0)
            {
                // Search field for filtering abilities
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Find:", GUILayout.Width(40));

                // Initialize the search string if not already done
                if (string.IsNullOrEmpty(_abilitySearchString))
                {
                    _abilitySearchString = "";
                }

                string newSearch = EditorGUILayout.TextField(_abilitySearchString, EditorStyles.toolbarSearchField);
                if (newSearch != _abilitySearchString)
                {
                    _abilitySearchString = newSearch;
                    _searchResultsNeedRefresh = true;
                }

                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)) &&
                    !string.IsNullOrEmpty(_abilitySearchString))
                {
                    _abilitySearchString = "";
                    _searchResultsNeedRefresh = true;
                    GUI.FocusControl(null);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // If there's a search string, filter the ability types
                var displayedIndices = new List<int>();

                if (!string.IsNullOrEmpty(_abilitySearchString) || _searchResultsNeedRefresh)
                {
                    // Filter ability types based on search
                    for (int i = 0; i < _availableAbilityTypeNames.Length; i++)
                    {
                        string displayName = ObjectNames.NicifyVariableName(_availableAbilityTypeNames[i]);
                        if (string.IsNullOrEmpty(_abilitySearchString) ||
                            displayName.IndexOf(_abilitySearchString, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            displayedIndices.Add(i);
                        }
                    }

                    _searchResultsNeedRefresh = false;
                }
                else
                {
                    // Show all if no search
                    for (int i = 0; i < _availableAbilityTypeNames.Length; i++)
                    {
                        displayedIndices.Add(i);
                    }
                }

                // Calculate visible rows/columns based on window width
                float windowWidth = EditorGUIUtility.currentViewWidth - 40; // Account for margins
                int maxButtonsPerRow = Mathf.Max(1, Mathf.FloorToInt(windowWidth / 180f));

                // Create a scrollable area for the ability grid
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition,
                    GUILayout.MinHeight(Mathf.Min(displayedIndices.Count * 32f, 180f)));

                // Create a grid of card-like buttons for abilities
                GUIStyle cardButtonStyle = new(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 32f,
                    padding = new RectOffset(12, 8, 8, 8),
                    margin = new RectOffset(4, 4, 4, 4),
                    richText = true
                };

                // Get categories for grouping (if not searching)
                var abilityIndicesByCategory = new Dictionary<string, List<int>>();

                if (string.IsNullOrEmpty(_abilitySearchString) && displayedIndices.Count > 6)
                {
                    // Group abilities by their category/namespace
                    foreach (int index in displayedIndices)
                    {
                        string typeName = _availableAbilityTypes[index].Name;
                        string category = GetAbilityCategory(_availableAbilityTypes[index]);

                        if (!abilityIndicesByCategory.ContainsKey(category))
                        {
                            abilityIndicesByCategory[category] = new List<int>();
                        }

                        abilityIndicesByCategory[category].Add(index);
                    }

                    // Display abilities grouped by category
                    foreach (var categoryPair in abilityIndicesByCategory.OrderBy(x => x.Key))
                    {
                        // Category header
                        EditorGUILayout.Space(4);
                        EditorGUILayout.LabelField(categoryPair.Key, EditorStyles.boldLabel);

                        // Grid for this category
                        int buttonsInCurrentRow = 0;
                        EditorGUILayout.BeginHorizontal();

                        foreach (int index in categoryPair.Value.OrderBy(i => _availableAbilityTypeNames[i]))
                        {
                            // Start a new row if needed
                            if (buttonsInCurrentRow >= maxButtonsPerRow)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                buttonsInCurrentRow = 0;
                            }

                            // Draw button with icon and name
                            string displayName = ObjectNames.NicifyVariableName(_availableAbilityTypeNames[index]);
                            Texture iconTexture = GetAbilityIcon(_availableAbilityTypes[index]);

                            GUIContent buttonContent;
                            if (iconTexture != null)
                            {
                                buttonContent = new GUIContent($"  {displayName}", iconTexture);
                            }
                            else
                            {
                                buttonContent = new GUIContent($"  {displayName}");
                            }

                            if (GUILayout.Button(buttonContent, cardButtonStyle, GUILayout.MinWidth(160)))
                            {
                                CreateAndAddAbility((CopyAbilityData)target, _availableAbilityTypes[index]);
                            }

                            buttonsInCurrentRow++;
                        }

                        // Complete the row
                        while (buttonsInCurrentRow < maxButtonsPerRow)
                        {
                            GUILayout.FlexibleSpace();
                            buttonsInCurrentRow++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    // Display a simple grid for search results or when there are few ability types
                    int buttonsInCurrentRow = 0;
                    EditorGUILayout.BeginHorizontal();

                    foreach (int index in displayedIndices.OrderBy(i => _availableAbilityTypeNames[i]))
                    {
                        // Start a new row if needed
                        if (buttonsInCurrentRow >= maxButtonsPerRow)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            buttonsInCurrentRow = 0;
                        }

                        // Draw button with icon and name
                        string displayName = ObjectNames.NicifyVariableName(_availableAbilityTypeNames[index]);
                        Texture iconTexture = GetAbilityIcon(_availableAbilityTypes[index]);

                        GUIContent buttonContent;
                        if (iconTexture != null)
                        {
                            buttonContent = new GUIContent($"  {displayName}", iconTexture);
                        }
                        else
                        {
                            buttonContent = new GUIContent($"  {displayName}");
                        }

                        // Highlight matches if searching
                        if (!string.IsNullOrEmpty(_abilitySearchString))
                        {
                            int matchIndex =
                                displayName.IndexOf(_abilitySearchString, StringComparison.OrdinalIgnoreCase);

                            if (matchIndex >= 0)
                            {
                                string beforeMatch = displayName.Substring(0, matchIndex);
                                string match = displayName.Substring(matchIndex, _abilitySearchString.Length);
                                string afterMatch = displayName.Substring(matchIndex + _abilitySearchString.Length);
                                buttonContent.text = $"  {beforeMatch}<color=#FFA500FF>{match}</color>{afterMatch}";
                            }
                        }

                        if (GUILayout.Button(buttonContent, cardButtonStyle, GUILayout.MinWidth(160)))
                        {
                            CreateAndAddAbility((CopyAbilityData)target, _availableAbilityTypes[index]);
                        }

                        buttonsInCurrentRow++;
                    }

                    // Complete the row with flexible space
                    while (buttonsInCurrentRow < maxButtonsPerRow)
                    {
                        GUILayout.FlexibleSpace();
                        buttonsInCurrentRow++;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();

                // No search results message
                if (displayedIndices.Count == 0)
                {
                    EditorGUILayout.HelpBox($"No abilities found matching '{_abilitySearchString}'", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No AbilityBase subclasses found in the project.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

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

        private void CleanupUnusedAbilityModules(CopyAbilityData owner)
        {
            // Start tracking stats
            int totalAbilitiesFound = 0;
            int abilitiesDeleted = 0;

            // Find the abilities folder for this CopyAbilityData
            string ownerPath = AssetDatabase.GetAssetPath(owner);
            if (string.IsNullOrEmpty(ownerPath))
            {
                EditorUtility.DisplayDialog("Cannot Clean Up",
                    "Save the CopyAbilityData asset first before cleaning up.", "OK");

                return;
            }

            string abilitiesFolderName = $"{owner.name.Replace(" ", "")}_Abilities";
            string directory = Path.Combine(Path.GetDirectoryName(ownerPath) ?? string.Empty, abilitiesFolderName);

            if (!Directory.Exists(directory))
            {
                EditorUtility.DisplayDialog("No Abilities Folder",
                    $"No abilities folder found at: {directory}", "OK");

                return;
            }

            // Get all ability assets in the directory
            string[] abilityAssetPaths = Directory.GetFiles(directory, "*.asset");
            totalAbilitiesFound = abilityAssetPaths.Length;

            if (totalAbilitiesFound == 0)
            {
                EditorUtility.DisplayDialog("No Abilities Found",
                    $"No ability assets found in: {directory}", "OK");

                return;
            }

            // First, find all CopyAbilityData assets in the project
            string[] allCopyAbilityGUIDs = AssetDatabase.FindAssets("t:CopyAbilityData");
            var allCopyAbilityData = new List<CopyAbilityData>();

            foreach (string guid in allCopyAbilityGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CopyAbilityData copyAbilityAsset = AssetDatabase.LoadAssetAtPath<CopyAbilityData>(path);
                if (copyAbilityAsset != null)
                {
                    allCopyAbilityData.Add(copyAbilityAsset);
                }
            }

            // Build a set of all referenced abilities across all CopyAbilityData assets
            var allReferencedAbilities = new HashSet<Object>();

            foreach (CopyAbilityData copyAbility in allCopyAbilityData)
            {
                SerializedObject serializedCopyAbility = new(copyAbility);
                SerializedProperty abilitiesProperty = serializedCopyAbility.FindProperty("abilities");

                if (abilitiesProperty != null && abilitiesProperty.isArray)
                {
                    for (int i = 0; i < abilitiesProperty.arraySize; i++)
                    {
                        SerializedProperty abilityRefProperty = abilitiesProperty.GetArrayElementAtIndex(i);
                        if (abilityRefProperty.objectReferenceValue != null)
                        {
                            allReferencedAbilities.Add(abilityRefProperty.objectReferenceValue);
                        }
                    }
                }
            }

            // Identify unreferenced abilities in this directory
            var pathsToDelete = new List<string>();

            foreach (string assetPath in abilityAssetPaths)
            {
                Object abilityAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (abilityAsset != null && !allReferencedAbilities.Contains(abilityAsset))
                {
                    pathsToDelete.Add(assetPath);
                }
            }

            // If no unreferenced abilities, inform the user
            if (pathsToDelete.Count == 0)
            {
                EditorUtility.DisplayDialog("No Unused Abilities",
                    "All ability modules in this folder are currently in use.", "OK");

                return;
            }

            // Ask for confirmation before deleting
            string message =
                $"Found {pathsToDelete.Count} unused ability modules out of {totalAbilitiesFound} total.\n\n";

            // Show first few abilities to be deleted (up to 5)
            int displayCount = Mathf.Min(pathsToDelete.Count, 5);
            for (int i = 0; i < displayCount; i++)
            {
                string name = Path.GetFileNameWithoutExtension(pathsToDelete[i]);
                message += $"• {name}\n";
            }

            if (pathsToDelete.Count > 5)
            {
                message += $"• ...and {pathsToDelete.Count - 5} more\n";
            }

            message += "\nDo you want to delete these unused ability modules?";

            bool userConfirmed = EditorUtility.DisplayDialog(
                "Confirm Cleanup",
                message,
                "Delete Unused Abilities",
                "Cancel");

            if (userConfirmed)
            {
                AssetDatabase.StartAssetEditing();

                try
                {
                    foreach (string path in pathsToDelete)
                    {
                        AssetDatabase.DeleteAsset(path);
                        abilitiesDeleted++;
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                EditorUtility.DisplayDialog(
                    "Cleanup Complete",
                    $"Successfully deleted {abilitiesDeleted} unused ability modules.",
                    "OK");
            }
        }

        // Helper method to create a cached editor without using custom editors
        private UnityEditor.Editor CreateCachedEditorWithoutCustomEditor(Object obj, Type type,
            ref Dictionary<Object, UnityEditor.Editor> cache)
        {
            if (obj == null)
                return null;

            if (cache.TryGetValue(obj, out UnityEditor.Editor editor) && editor != null)
                return editor;

            // Create editor without using custom editors
            editor = CreateEditor(obj);

            // Store in cache
            cache[obj] = editor;

            return editor;
        }

        // Helper method that reuses AbilityModuleBaseEditor for embedded rendering
        private void DrawAbilityModuleInspector(AbilityModuleBase abilityModuleInstance, GUIStyle headerStyle)
        {
            if (!abilityModuleInstance) return;


            // Check if we have a cached editor first
            if (!_cachedEditors.TryGetValue(abilityModuleInstance, out UnityEditor.Editor abilityEditor) ||
                !abilityEditor)
            {
                // Create a new editor (this will use AbilityModuleBaseEditor because of the [CustomEditor] attribute)
                abilityEditor = CreateEditor(abilityModuleInstance);
                _cachedEditors[abilityModuleInstance] = abilityEditor;
            }

            // Now draw the custom editor - this will use AbilityModuleBaseEditor's OnInspectorGUI
            abilityEditor.OnInspectorGUI();
        }

        // Helper methods for the new ability selector UI
        private string GetAbilityCategory(Type abilityType)
        {
            // Extract category from namespace or other attribute
            string category = "General";

            // Try to get category from namespace
            if (abilityType.Namespace != null)
            {
                string[] namespaceParts = abilityType.Namespace.Split('.');
                if (namespaceParts.Length > 2)
                {
                    category = namespaceParts[namespaceParts.Length - 1];
                }
            }

            // Try to get category from name pattern
            string typeName = abilityType.Name;
            if (typeName.EndsWith("AbilityModule"))
            {
                string baseTypeName = typeName.Substring(0, typeName.Length - "AbilityModule".Length);
                if (baseTypeName.Contains("Attack") || baseTypeName.Contains("Weapon"))
                {
                    category = "Combat";
                }
                else if (baseTypeName.Contains("Move") || baseTypeName.Contains("Jump") ||
                         baseTypeName.Contains("Dash"))
                {
                    category = "Movement";
                }
                else if (baseTypeName.Contains("Shield") || baseTypeName.Contains("Defense"))
                {
                    category = "Defense";
                }
            }

            return category;
        }

        private Texture GetAbilityIcon(Type abilityType)
        {
            // Look for icons based on ability type name or category
            string iconName = abilityType.Name;
            Texture icon = null;

            // Try to find icon in project
            string[] guids = AssetDatabase.FindAssets(iconName + " t:texture");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                icon = AssetDatabase.LoadAssetAtPath<Texture>(path);
            }

            // Default icon based on category if no specific icon found
            if (icon == null)
            {
                string category = GetAbilityCategory(abilityType);

                switch (category)
                {
                    case "Combat":
                        icon = EditorGUIUtility.IconContent("d_Animation.Play").image;
                        break;
                    case "Movement":
                        icon = EditorGUIUtility.IconContent("d_MoveTool").image;
                        break;
                    case "Defense":
                        icon = EditorGUIUtility.IconContent("d_PreMatCube").image;
                        break;
                    default:
                        icon = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image;
                        break;
                }
            }

            return icon;
        }
    }
}
#endif
