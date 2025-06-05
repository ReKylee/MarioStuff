using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.Conditions.Core;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Implementation of the animation context with strong typing for parameters
    /// </summary>
    public class AnimationContext : IAnimationContext
    {
        // Single collection to store parameter data (using Dictionary for faster lookups by name)
        private readonly Dictionary<string, ParameterData> _parameters = new();

        // Cache of actual runtime values (may differ from DefaultValue in ParameterData)
        private readonly Dictionary<string, object> _runtimeValues = new();

        public AnimationContext(IAnimator animator, GameObject entity)
        {
            Animator = animator ?? throw new ArgumentNullException(nameof(animator));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public GameObject Entity { get; }

        public IAnimator Animator { get; }

        /// <summary>
        ///     Get a parameter value by name with type checking
        /// </summary>
        public T GetParameter<T>(string name)
        {
            if (_runtimeValues.TryGetValue(name, out object value))
            {
                // Type check for better error reporting
                if (value is T typedValue)
                {
                    return typedValue;
                }

                Debug.LogWarning(
                    $"Parameter '{name}' exists but is of type {value.GetType().Name}, not {typeof(T).Name}");
            }

            return default;
        }

        /// <summary>
        ///     Set a parameter value
        /// </summary>
        public void SetParameter<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Parameter value cannot be null");
            }

            // Define the parameter type based on the value
            ConditionDataType parameterType = GetConditionDataTypeFromType(typeof(T));

            // Create or update parameter data
            if (!_parameters.TryGetValue(name, out ParameterData paramData))
            {
                // Create new parameter data if it doesn't exist
                paramData = new ParameterData
                {
                    Name = name,
                    Type = parameterType,
                    DefaultValue = value
                };

                // Add to parameters dictionary
                _parameters[name] = paramData;
            }
            else
            {
                // Update existing parameter data
                paramData.Type = parameterType;
                paramData.DefaultValue = value;
            }

            // Store the runtime value
            _runtimeValues[name] = value;
        }

        /// <summary>
        ///     Check if a parameter exists
        /// </summary>
        public bool HasParameter(string name) => _parameters.ContainsKey(name);

        /// <summary>
        ///     Get the parameter type
        /// </summary>
        public Type GetParameterType(string name)
        {
            if (_parameters.TryGetValue(name, out ParameterData paramData))
            {
                return GetTypeFromConditionDataType(paramData.Type);
            }

            return null;
        }

        /// <summary>
        ///     Get all parameter data for serialization or editor usage
        /// </summary>
        public IEnumerable<ParameterData> GetAllParameters() => _parameters.Values;

        /// <summary>
        ///     Convert a Type to ConditionDataType enum
        /// </summary>
        private static ConditionDataType GetConditionDataTypeFromType(Type type)
        {
            if (type == typeof(bool)) return ConditionDataType.Boolean;
            if (type == typeof(int)) return ConditionDataType.Integer;
            if (type == typeof(float)) return ConditionDataType.Float;
            if (type == typeof(string)) return ConditionDataType.String;

            // Default to string for unknown types
            return ConditionDataType.String;
        }

        /// <summary>
        ///     Convert a ConditionDataType enum to Type
        /// </summary>
        private static Type GetTypeFromConditionDataType(ConditionDataType dataType)
        {
            return dataType switch
            {
                ConditionDataType.Boolean => typeof(bool),
                ConditionDataType.Integer => typeof(int),
                ConditionDataType.Float => typeof(float),
                ConditionDataType.String => typeof(string),
                _ => typeof(string)
            };
        }
    }
}
