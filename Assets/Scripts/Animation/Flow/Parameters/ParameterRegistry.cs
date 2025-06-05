using System;
using System.Collections.Generic;
using Animation.Flow.Parameters.ConcreteParameters;
using UnityEngine;

namespace Animation.Flow.Parameters
{
    /// <summary>
    ///     Central registry for all parameter definitions in the animation system
    /// </summary>
    public static class ParameterRegistry
    {
        private static readonly Dictionary<string, FlowParameter> _parameterDefinitions = new();

        #region Registration Methods

        /// <summary>
        ///     Register a parameter definition
        /// </summary>
        public static void RegisterParameter(FlowParameter parameter)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.Name))
            {
                Debug.LogWarning("Cannot register null or invalid parameter");
                return;
            }

            if (!parameter.Validate())
            {
                Debug.LogWarning($"Parameter {parameter.Name} failed validation and will not be registered");
                return;
            }

            _parameterDefinitions[parameter.Name] = parameter;
        }

        /// <summary>
        ///     Register a bool parameter
        /// </summary>
        public static void RegisterBoolParameter(string name, bool defaultValue = false, string description = "")
        {
            RegisterParameter(new BoolParameter(name, defaultValue, description));
        }

        /// <summary>
        ///     Register a float parameter
        /// </summary>
        public static void RegisterFloatParameter(string name, float defaultValue = 0f, string description = "",
            float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            RegisterParameter(new FloatParameter(name, defaultValue, description, minValue, maxValue));
        }

        /// <summary>
        ///     Register an int parameter
        /// </summary>
        public static void RegisterIntParameter(string name, int defaultValue = 0, string description = "",
            int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            RegisterParameter(new IntParameter(name, defaultValue, description, minValue, maxValue));
        }

        /// <summary>
        ///     Register a string parameter
        /// </summary>
        public static void RegisterStringParameter(string name, string defaultValue = "", string description = "", 
            int maxLength = 0)
        {
            RegisterParameter(new StringParameter(name, defaultValue, description, maxLength));
        }

        /// <summary>
        ///     Unregister a parameter
        /// </summary>
        public static void UnregisterParameter(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            _parameterDefinitions.Remove(name);
        }

        #endregion

        #region Query Methods

        /// <summary>
        ///     Check if a parameter is registered
        /// </summary>
        public static bool IsParameterRegistered(string name)
        {
            return !string.IsNullOrEmpty(name) && _parameterDefinitions.ContainsKey(name);
        }

        /// <summary>
        ///     Get a parameter definition
        /// </summary>
        public static FlowParameter GetParameterDefinition(string name)
        {
            if (!IsParameterRegistered(name)) return null;

            return _parameterDefinitions[name];
        }

        /// <summary>
        ///     Get a typed parameter definition
        /// </summary>
        public static T GetParameterDefinition<T>(string name) where T : FlowParameter
        {
            var param = GetParameterDefinition(name);
            if (param is T typedParam)
                return typedParam;

            return null;
        }

        /// <summary>
        ///     Get all parameter definitions
        /// </summary>
        public static IEnumerable<FlowParameter> GetAllParameterDefinitions()
        {
            return _parameterDefinitions.Values;
        }

        /// <summary>
        ///     Create a new parameter of the specified type
        /// </summary>
        public static FlowParameter CreateParameter(Type parameterType, string name, object defaultValue = null, string description = "")
        {
            if (parameterType == typeof(bool) || parameterType == typeof(BoolParameter))
                return new BoolParameter(name, defaultValue as bool? ?? false, description);

            if (parameterType == typeof(float) || parameterType == typeof(FloatParameter))
                return new FloatParameter(name, defaultValue as float? ?? 0f, description);

            if (parameterType == typeof(int) || parameterType == typeof(IntParameter))
                return new IntParameter(name, defaultValue as int? ?? 0, description);

            if (parameterType == typeof(string) || parameterType == typeof(StringParameter))
                return new StringParameter(name, defaultValue as string ?? "", description);

            Debug.LogWarning($"Cannot create parameter of unknown type: {parameterType}");
            return null;
        }

        #endregion
    }
}
