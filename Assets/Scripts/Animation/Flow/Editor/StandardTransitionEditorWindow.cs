using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Animation.Flow.Editor
{
    public class StandardTransitionEditorWindow : EditorWindow
    {
        private bool _boolValue = true;
        private string _compareType = "Equals";
        private string _conditionType = "Bool";
        private string _currentEdgeId;
        private Edge _currentEdge;
        private List<ConditionData> _conditions;
        private string _floatValue = "0";
        private string _parameterName = "";
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            // Make sure we have valid data when window is shown
            if (_currentEdge != null && string.IsNullOrEmpty(_currentEdgeId))
            {
                _currentEdgeId = EdgeConditionManager.Instance.GetEdgeId(_currentEdge);
                if (!string.IsNullOrEmpty(_currentEdgeId))
                {
                    _conditions = EdgeConditionManager.Instance.GetConditions(_currentEdgeId);
                }
            }
        }

        private void OnGUI()
        {
            if (_currentEdge == null || string.IsNullOrEmpty(_currentEdgeId) || _conditions == null)
            {
                EditorGUILayout.HelpBox("No transition selected.", MessageType.Warning);
                return;
            }

            AnimationStateNode sourceNode = _currentEdge.output.node as AnimationStateNode;
            AnimationStateNode targetNode = _currentEdge.input.node as AnimationStateNode;

            if (sourceNode == null || targetNode == null)
            {
                EditorGUILayout.HelpBox("Invalid transition.", MessageType.Error);
                return;
            }

            GUILayout.Label($"Transition: {sourceNode.AnimationName} → {targetNode.AnimationName}",
                EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // Display existing conditions
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

            if (_conditions.Count == 0)
            {
                EditorGUILayout.HelpBox("No conditions. This transition will always occur.", MessageType.Info);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

                for (int i = 0; i < _conditions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    ConditionData condition = _conditions[i];
                    string conditionDesc = GetConditionDescription(condition);

                    EditorGUILayout.LabelField(conditionDesc);

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        _conditions.RemoveAt(i);
                        i--;
                        
                        // Update conditions in the manager
                        EdgeConditionManager.Instance.SetConditions(_currentEdgeId, _conditions);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            // Add new condition
            EditorGUILayout.LabelField("Add Condition", EditorStyles.boldLabel);

            _parameterName = EditorGUILayout.TextField("Parameter Name", _parameterName);

            // Toggle between Bool and Float
            bool isBool = _conditionType == "Bool";
            bool newIsBool = EditorGUILayout.Toggle("Is Boolean", isBool);
            if (newIsBool != isBool)
            {
                _conditionType = newIsBool ? "Bool" : "Float";
            }

            if (_conditionType == "Bool")
            {
                _boolValue = EditorGUILayout.Toggle("Value", _boolValue);
            }
            else
            {
                _compareType = EditorGUILayout.Popup("Comparison",
                        _compareType == "Equals" ? 0 : _compareType == "Less Than" ? 1 : 2,
                        new[] { "Equals", "Less Than", "Greater Than" }) == 0 ? "Equals" :
                    _compareType == "Less Than" ? "Less Than" : "Greater Than";

                _floatValue = EditorGUILayout.TextField("Value", _floatValue);
            }

            if (GUILayout.Button("Add Condition"))
            {
                if (!string.IsNullOrEmpty(_parameterName))
                {
                    AddCondition();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Condition", "Parameter name cannot be empty.", "OK");
                }
            }

            EditorGUILayout.Space();

            // Special conditions
            if (GUILayout.Button("Add Animation Complete Condition"))
            {
                ConditionData condition = new()
                {
                    Type = "AnimationComplete",
                    ParameterName = ""
                };

                _conditions.Add(condition);
                
                // Update conditions in the manager
                EdgeConditionManager.Instance.SetConditions(_currentEdgeId, _conditions);
            }

            if (GUILayout.Button("Add Time Elapsed Condition"))
            {
                ConditionData condition = new()
                {
                    Type = "TimeElapsed",
                    ParameterName = "StateTime",
                    FloatValue = 0.5f
                };

                _conditions.Add(condition);
                
                // Update conditions in the manager
                EdgeConditionManager.Instance.SetConditions(_currentEdgeId, _conditions);

                // Create a temporary editing UI for the newly added condition
                _conditionType = "Float";
                _parameterName = "StateTime";
                _floatValue = "0.5";
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        public static void ShowWindow(Edge edge)
        {
            string edgeId = EdgeConditionManager.Instance.GetEdgeId(edge);
            if (string.IsNullOrEmpty(edgeId))
            {
                Debug.LogError("Cannot show transition editor - invalid edge");
                return;
            }

            StandardTransitionEditorWindow window = GetWindow<StandardTransitionEditorWindow>(true, "Edit Transition");
            window.minSize = new Vector2(300, 400);
            window._currentEdge = edge;
            window._currentEdgeId = edgeId;
            window._conditions = EdgeConditionManager.Instance.GetConditions(edgeId);
            window.Show();
        }

        private void AddCondition()
        {
            ConditionData condition = new()
            {
                ParameterName = _parameterName
            };

            if (_conditionType == "Bool")
            {
                condition.Type = "Bool";
                condition.BoolValue = _boolValue;
            }
            else
            {
                condition.Type = "Float" + _compareType.Replace(" ", "");
                if (float.TryParse(_floatValue, out float value))
                {
                    condition.FloatValue = value;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Value", "Please enter a valid float value.", "OK");
                    return;
                }
            }

            _conditions.Add(condition);
            
            // Update conditions in the manager
            EdgeConditionManager.Instance.SetConditions(_currentEdgeId, _conditions);

            // Clear input fields
            _parameterName = "";
        }

        private string GetConditionDescription(ConditionData condition)
        {
            // Dictionary mapping condition types to human-readable descriptions
            var conditionDescriptions = new Dictionary<string, Func<ConditionData, string>>
            {
                // Boolean conditions
                { "Bool", c => $"{c.ParameterName} {(c.BoolValue ? "is true" : "is false")}" },

                // Float comparison conditions
                { "FloatEquals", c => $"{c.ParameterName} = {c.FloatValue}" },
                { "FloatLessThan", c => $"{c.ParameterName} < {c.FloatValue}" },
                { "FloatGreaterThan", c => $"{c.ParameterName} > {c.FloatValue}" },

                // Special conditions
                { "AnimationComplete", _ => "Animation is complete" },
                { "TimeElapsed", c => $"Time in state > {c.FloatValue}s" },
                { "AnyCondition", _ => "Any condition is met" }
            };

            // If we have a description for this condition type, use it
            return conditionDescriptions.TryGetValue(condition.Type, out var formatter)
                ? formatter(condition)
                : $"Unknown condition: {condition.Type}";
        }
    }
}
