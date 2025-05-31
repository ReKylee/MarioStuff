#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Kirby.Abilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kirby.Editor
{
    /// <summary>
    ///     A helper class that creates a reorderable list for StatModifier collections,
    ///     providing a cleaner UI without "Element X" foldouts
    /// </summary>
    public class ReorderableModifierList
    {
        private readonly float _elementHeight = EditorGUIUtility.singleLineHeight + 2;
        private readonly ReorderableList _list;
        private readonly SerializedProperty _listProperty;
        private readonly SerializedObject _serializedObject;
        private readonly bool _showAddButton;
        private readonly bool _showHeader;

        public ReorderableModifierList(SerializedObject serializedObject, SerializedProperty property,
            bool showHeader = true, bool showAddButton = true)
        {
            _serializedObject = serializedObject;
            _listProperty = property;
            _showHeader = showHeader;
            _showAddButton = showAddButton;

            _list = new ReorderableList(serializedObject, property,
                true, // draggable
                showHeader,
                showAddButton,
                true); // show remove button

            // Custom drawing for the header
            _list.drawHeaderCallback = rect =>
            {
                if (!_showHeader) return;

                // Draw column headers
                float statWidth = rect.width * 0.4f;
                float typeWidth = rect.width * 0.3f;
                float valueWidth = rect.width * 0.3f - 24; // Account for remove button

                EditorGUI.LabelField(new Rect(rect.x, rect.y, statWidth, rect.height), "Stat");
                EditorGUI.LabelField(new Rect(rect.x + statWidth, rect.y, typeWidth, rect.height), "Type");
                EditorGUI.LabelField(new Rect(rect.x + statWidth + typeWidth, rect.y, valueWidth, rect.height),
                    "Value");
            };

            // Custom drawing for each element
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = property.GetArrayElementAtIndex(index);

                // Adjust rect for element drawing
                rect.y += 1;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Get properties
                SerializedProperty statTypeProp = element.FindPropertyRelative("statType");
                SerializedProperty modTypeProp = element.FindPropertyRelative("modificationType");
                SerializedProperty valueProp = element.FindPropertyRelative("value");

                // Calculate column widths
                float statWidth = rect.width * 0.4f;
                float typeWidth = rect.width * 0.3f;
                float valueWidth = rect.width * 0.3f - 24; // Account for remove button

                // Create rects for each field
                Rect statRect = new(rect.x, rect.y, statWidth, rect.height);
                Rect typeRect = new(rect.x + statWidth, rect.y, typeWidth, rect.height);
                Rect valueRect = new(rect.x + statWidth + typeWidth, rect.y, valueWidth, rect.height);

                // Draw stat type field with popup button
                string currentStatName = "Select Stat";
                if (statTypeProp.enumValueIndex >= 0 &&
                    statTypeProp.enumValueIndex < statTypeProp.enumDisplayNames.Length)
                {
                    currentStatName =
                        ObjectNames.NicifyVariableName(statTypeProp.enumDisplayNames[statTypeProp.enumValueIndex]);
                }

                if (GUI.Button(statRect, currentStatName, EditorStyles.popup))
                {
                    // Get list of StatTypes already used in this list, excluding the current element
                    var usedStatTypesInList = new List<StatType>();
                    for (int i = 0; i < _listProperty.arraySize; i++)
                    {
                        if (i == index) continue; // Skip the current element itself
                        usedStatTypesInList.Add((StatType)_listProperty.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("statType").enumValueIndex);
                    }

                    // Use the centralized utility method, passing the statTypeProp of the current element
                    // and the list of other used stats to filter them out from the selection menu.
                    StatModifierEditorUtility.ShowStatTypeSelectionMenu(statTypeProp, usedStatTypesInList);
                }

                // Draw modification type dropdown
                EditorGUI.PropertyField(typeRect, modTypeProp, GUIContent.none);

                // Draw value field
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            };

            // Set element height
            _list.elementHeight = _elementHeight;

            // Handle adding new elements
            _list.onAddCallback = list =>
            {
                // Get list of all StatTypes currently used in the list to filter them out
                var existingStatTypesInList = new List<StatType>();
                for (int i = 0; i < _listProperty.arraySize; i++)
                {
                    existingStatTypesInList.Add((StatType)_listProperty.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("statType").enumValueIndex);
                }

                // Show the stat selection menu, filtering out already used stats.
                // The menu's callback will handle adding the new element.
                // We need a dummy SerializedProperty to satisfy ShowStatTypeSelectionMenu, 
                // but its selection will be used to add a new item.
                // A more direct approach for adding might be better, but this reuses the menu.

                // Create a temporary StatType variable to hold the selection from the menu
                StatType selectedStatForNewModifier = default;

                GenericMenu menu = new();
                var availableStats = StatModifierEditorUtility.GetAllStatTypes().Except(existingStatTypesInList)
                    .ToList();

                if (availableStats.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Stats Available",
                        "All available stat types are already in use in this list.", "OK");

                    return;
                }

                var statsByCategory = availableStats
                    .GroupBy(KirbyStats.GetStatCategory)
                    .OrderBy(g => g.Key);

                foreach (var group in statsByCategory)
                {
                    foreach (StatType stat in group.OrderBy(s => s.ToString()))
                    {
                        string statName = ObjectNames.NicifyVariableName(stat.ToString());
                        menu.AddItem(new GUIContent($"{group.Key}/{statName}"), false, () =>
                        {
                            selectedStatForNewModifier = stat; // Store selected stat

                            // Add the new element to the list with the selected stat
                            int newIndex = _listProperty.arraySize;
                            _listProperty.InsertArrayElementAtIndex(newIndex);
                            SerializedProperty newElement = _listProperty.GetArrayElementAtIndex(newIndex);

                            newElement.FindPropertyRelative("statType").enumValueIndex =
                                (int)selectedStatForNewModifier;

                            newElement.FindPropertyRelative("modificationType").enumValueIndex =
                                (int)StatModifier.ModType.Multiplicative;

                            newElement.FindPropertyRelative("value").floatValue = 1f;
                            _serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                menu.ShowAsContext();
            };
        }

        public void DoLayoutList()
        {
            _serializedObject.Update();
            _list.DoLayoutList();
            _serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
