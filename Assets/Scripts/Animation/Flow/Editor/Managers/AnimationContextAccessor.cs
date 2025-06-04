using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Core;
using Animation.Flow.Editor.Panels.Parameters;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Editor.Managers
{
    /// <summary>
    ///     Provides editor access to parameters from the runtime AnimationContext
    /// </summary>
    public class AnimationContextAccessor
    {

        private const string ParametersKey = "AnimationFlowParameters";
        private static AnimationContextAccessor _instance;

        // Reference to the active AnimationContext (set by the AnimationFlowController)
        private static AnimationContext _activeContext;

        public static AnimationContextAccessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AnimationContextAccessor();
                    _instance.LoadParameters();
                }

                return _instance;
            }
        }

        public List<ParameterData> Parameters { get; private set; } = new();

        /// <summary>
        ///     Set the active animation context
        /// </summary>
        public static void SetActiveContext(AnimationContext context)
        {
            _activeContext = context;
        }

        /// <summary>
        ///     Get the active animation context
        /// </summary>
        public static AnimationContext GetActiveContext() => _activeContext;

        /// <summary>
        ///     Add a new parameter to the system and to the active context if available
        /// </summary>
        public void AddParameter(ParameterData parameter)
        {
            if (!Parameters.Exists(p => p.Name == parameter.Name))
            {
                Parameters.Add(parameter);
                SaveParameters();

                // Set default value in active context if available
                if (_activeContext != null)
                {
                    SetParameterInContext(parameter);
                }
            }
        }

        /// <summary>
        ///     Remove a parameter from the system
        /// </summary>
        public void RemoveParameter(string parameterName)
        {
            Parameters.RemoveAll(p => p.Name == parameterName);
            SaveParameters();
        }

        /// <summary>
        ///     Update an existing parameter
        /// </summary>
        public void UpdateParameter(ParameterData parameter)
        {
            int index = Parameters.FindIndex(p => p.Name == parameter.Name);
            if (index >= 0)
            {
                Parameters[index] = parameter;
                SaveParameters();

                // Update in active context if available
                if (_activeContext != null)
                {
                    SetParameterInContext(parameter);
                }
            }
        }

        /// <summary>
        ///     Sets the parameter value in the active animation context
        /// </summary>
        private void SetParameterInContext(ParameterData parameter)
        {
            if (_activeContext == null) return;

            switch (parameter.Type)
            {
                case ConditionDataType.Boolean:
                    _activeContext.SetParameter(parameter.Name, (bool)parameter.DefaultValue);
                    break;
                case ConditionDataType.Integer:
                    _activeContext.SetParameter(parameter.Name, (int)parameter.DefaultValue);
                    break;
                case ConditionDataType.Float:
                    _activeContext.SetParameter(parameter.Name, (float)parameter.DefaultValue);
                    break;
                case ConditionDataType.String:
                    _activeContext.SetParameter(parameter.Name, (string)parameter.DefaultValue);
                    break;
            }
        }

        /// <summary>
        ///     Find a parameter by name
        /// </summary>
        public ParameterData GetParameter(string name)
        {
            return Parameters.Find(p => p.Name == name);
        }

        /// <summary>
        ///     Save parameters to EditorPrefs
        /// </summary>
        private void SaveParameters()
        {
            string json = JsonUtility.ToJson(new ParameterDataList { Parameters = Parameters });
            EditorPrefs.SetString(ParametersKey, json);
        }

        /// <summary>
        ///     Load parameters from EditorPrefs
        /// </summary>
        private void LoadParameters()
        {
            if (EditorPrefs.HasKey(ParametersKey))
            {
                string json = EditorPrefs.GetString(ParametersKey);
                ParameterDataList dataList = JsonUtility.FromJson<ParameterDataList>(json);
                Parameters = dataList?.Parameters ?? new List<ParameterData>();
            }
            else
            {
                // Create default parameters if none exist
                Parameters = new List<ParameterData>
                {
                    new() { Name = "IsGrounded", Type = ConditionDataType.Boolean, DefaultValue = false },
                    new() { Name = "Speed", Type = ConditionDataType.Float, DefaultValue = 0f },
                    new() { Name = "Health", Type = ConditionDataType.Integer, DefaultValue = 100 },
                    new() { Name = "State", Type = ConditionDataType.String, DefaultValue = "Idle" }
                };

                SaveParameters();
            }
        }

        /// <summary>
        ///     Synchronize parameters with the active AnimationContext
        /// </summary>
        public void SyncWithActiveContext()
        {
            if (_activeContext == null) return;

            // Apply all current parameters to the active context
            foreach (ParameterData parameter in Parameters)
            {
                SetParameterInContext(parameter);
            }
        }

        /// <summary>
        ///     Apply a list of condition data to the active context
        /// </summary>
        public void ApplyConditionsToContext(List<ConditionData> conditions)
        {
            if (_activeContext == null || conditions == null) return;

            foreach (ConditionData condition in conditions)
            {
                // For now, we're just applying the default value from the parameter
                // In a real implementation, you'd likely want to apply the condition's specific value
                ParameterData param = GetParameter(condition.ParameterName);
                if (param != null)
                {
                    SetParameterInContext(param);
                }
            }
        }

        [Serializable]
        private class ParameterDataList
        {
            public List<ParameterData> Parameters;
        }
    }
}
