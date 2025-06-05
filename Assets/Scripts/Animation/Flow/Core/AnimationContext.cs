using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using Animation.Flow.Parameters;
using Animation.Flow.Parameters.ConcreteParameters;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Implementation of the animation context with strong typing for parameters
    /// </summary>
    [Serializable]
    public class AnimationContext : IAnimationContext
    {
        #region Serialized Fields

        // Store local parameter definitions when not connected to ParameterRegistry
        [SerializeField] private List<FlowParameter> _localParameterDefinitions = new();

        #endregion

        #region Runtime Fields

        // Cache of actual runtime values (may differ from default values)
        private readonly Dictionary<string, object> _runtimeValues = new();

        // The animator instance
        [NonSerialized] private IAnimator _animator;

        // The entity this context belongs to
        [NonSerialized] private GameObject _entity;

        // Track which parameters were modified during this frame
        private readonly HashSet<string> _dirtyParameters = new();

        #endregion

        #region Constructors

        public AnimationContext() { }

        public AnimationContext(IAnimator animator, GameObject entity)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));

            // Initialize the parameter registry and import local parameters
            InitializeRegistry();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the entity this context is for
        /// </summary>
        public GameObject Entity => _entity;

        /// <summary>
        ///     Gets the animator instance
        /// </summary>
        public IAnimator Animator => _animator;

         #endregion

        /// <summary>
        ///     Initialize the parameter registry with local parameters
        /// </summary>
        private void InitializeRegistry()
        {
            // Register any local parameters with the registry
            if (_localParameterDefinitions != null && _localParameterDefinitions.Count > 0)
            {
                foreach (var param in _localParameterDefinitions)
                {
                    if (param != null && !string.IsNullOrEmpty(param.Name))
                    {
                        ParameterRegistry.RegisterParameter(param.Clone());
                    }
                }
            }
        }


        #region Parameter Management

        /// <summary>
        ///     Get a parameter value by name with type checking
        /// </summary>
        /// <typeparam name="T">Expected parameter type</typeparam>
        /// <param name="name">The parameter name</param>
        /// <returns>The parameter value or default if not found or wrong type</returns>
        public T GetParameter<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
                return default;

            // Try to get runtime value first (local override)
            if (TryGetRuntimeValue<T>(name, out T result))
                return result;

            // Then fall back to default value from registry
            if (TryGetRegistryDefaultValue<T>(name, out result))
                return result;

            // Parameter doesn't exist or has incompatible type
            return default;
        }

        public void SetParameter<T>(string parameterName, T value)
        {
            
        }

        /// <summary>
        ///     Try to get a runtime parameter value with type checking
        /// </summary>
        private bool TryGetRuntimeValue<T>(string name, out T result)
        {
            result = default;

            if (!_runtimeValues.TryGetValue(name, out object value))
                return false;

            // Check if value is of the expected type
            if (value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            // Try to convert value if possible
            try
            {
                if (typeof(T) == typeof(bool) && value is int intValue)
                {
                    result = (T)(object)(intValue != 0);
                    return true;
                }
                else if (typeof(T) == typeof(int) && value is float floatValue)
                {
                    result = (T)(object)Mathf.RoundToInt(floatValue);
                    return true;
                }
                // Add other common conversions as needed
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to convert parameter '{name}' to {typeof(T).Name}: {ex.Message}");
            }

            Debug.LogWarning($"Parameter '{name}' exists but is of type {value.GetType().Name}, not {typeof(T).Name}");
            return false;
        }

        /// <summary>
        ///     Try to get a default parameter value from the registry with type checking
        /// </summary>
        private bool TryGetRegistryDefaultValue<T>(string name, out T result)
        {
            result = default;

            // Get the parameter from the registry
            FlowParameter parameter = ParameterRegistry.GetParameterDefinition(name);

            if (parameter == null)
                return false;

            // Fast path for exact type match
            if (parameter is FlowParameter<T> typedParam)
            {
                result = typedParam.DefaultValue;
                return true;
            }

            // Try to convert from untyped default value
            object defaultValue = parameter.GetDefaultValue();
            if (defaultValue is T defaultTypedValue)
            {
                result = defaultTypedValue;
                return true;
            }

            Debug.LogWarning($"Parameter '{name}' default value is of type {defaultValue?.GetType().Name ?? "null"}, not {typeof(T).Name}");
            return false;
        }

        /// <summary>
        ///     Set a parameter value and mark it as modified
        /// </summary>
        /// <typeparam name="T">Parameter type</typeparam>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="createIfMissing">Whether to create a parameter definition if it doesn't exist</param>
        public void SetParameter<T>(string name, T value, bool createIfMissing = true)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value), "Parameter value cannot be null");

            bool parameterExists = ParameterRegistry.IsParameterRegistered(name);

            // Ensure the parameter definition exists if requested
            if (!parameterExists && createIfMissing)
            {
                CreateParameterDefinition<T>(name, value);
            }
            else if (!parameterExists)
            {
                Debug.LogWarning($"Parameter '{name}' does not exist and createIfMissing is false");
                return;
            }

            // Check if the value has actually changed
            bool valueChanged = true;
            if (_runtimeValues.TryGetValue(name, out object oldValue) && oldValue != null)
            {
                valueChanged = !oldValue.Equals(value);
            }

            // Store the runtime value
            _runtimeValues[name] = value;

            // Mark parameter as dirty if changed
            if (valueChanged)
            {
                _dirtyParameters.Add(name);
            }
        }

        /// <summary>
        ///     Create a new parameter definition based on a value and register it
        /// </summary>
        private void CreateParameterDefinition<T>(string name, T defaultValue)
        {
            FlowParameter paramDef;

            // Create the appropriate parameter type
            if (typeof(T) == typeof(bool))
            {
                paramDef = new BoolParameter(name, (bool)(object)defaultValue);
            }
            else if (typeof(T) == typeof(int))
            {
                paramDef = new IntParameter(name, (int)(object)defaultValue);
            }
            else if (typeof(T) == typeof(float))
            {
                paramDef = new FloatParameter(name, (float)(object)defaultValue);
            }
            else if (typeof(T) == typeof(string))
            {
                paramDef = new StringParameter(name, (string)(object)defaultValue);
            }
            else
            {
                // For unsupported types, store as string
                paramDef = new StringParameter(name, defaultValue.ToString());
                Debug.LogWarning($"Parameter '{name}' type {typeof(T).Name} is not directly supported. Converting to string.");
            }

            // Register with parameter registry
            ParameterRegistry.RegisterParameter(paramDef);

            // Also keep a local copy if needed for serialization/persistence
            _localParameterDefinitions.Add(paramDef.Clone());
        }


        /// <summary>
        ///     Check if a parameter exists
        /// </summary>
        public bool HasParameter(string name) => 
            !string.IsNullOrEmpty(name) && 
            (ParameterRegistry.IsParameterRegistered(name) || _runtimeValues.ContainsKey(name));

        /// <summary>
        ///     Get the parameter type
        /// </summary>
        public Type GetParameterType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // Check runtime values first (more likely to have the current type)
            if (_runtimeValues.TryGetValue(name, out object value) && value != null)
            {
                return value.GetType();
            }

            // Check parameter registry
            var parameter = ParameterRegistry.GetParameterDefinition(name);
            if (parameter != null)
            {
                return parameter.ParameterType;
            }

            return null;
        }

        /// <summary>
        ///     Synchronize this context with the parameter registry
        /// </summary>
        public void SyncWithRegistry()
        {
            // Register all our local parameters with the registry
            foreach (var param in _localParameterDefinitions)
            {
                if (param != null && !string.IsNullOrEmpty(param.Name))
                {
                    ParameterRegistry.RegisterParameter(param.Clone());
                }
            }
        }

        /// <summary>
        ///     Add a parameter definition
        /// </summary>
        public void AddParameterDefinition(FlowParameter parameter)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.Name))
                return;

            // Keep a local copy for serialization/persistence
            _localParameterDefinitions.RemoveAll(p => p != null && p.Name == parameter.Name);
            _localParameterDefinitions.Add(parameter.Clone());

            // Register with parameter registry
            ParameterRegistry.RegisterParameter(parameter.Clone());
        }

        /// <summary>
        ///     Remove a parameter definition
        /// </summary>
        public void RemoveParameterDefinition(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            // Remove from local definitions list
            _localParameterDefinitions.RemoveAll(p => p != null && p.Name == name);

            // Unregister from parameter registry
            ParameterRegistry.UnregisterParameter(name);

            // Runtime values are kept so they continue to work during gameplay
        }

        /// <summary>
        ///     Set the animator instance
        /// </summary>
        public void SetAnimator(IAnimator animator)
        {
            _animator = animator;

            // Sync with the registry after setting a new animator
            if (_animator != null)
            {
                SyncWithRegistry();
            }
        }

        /// <summary>
        ///     Set the entity
        /// </summary>
        public void SetEntity(GameObject entity)
        {
            _entity = entity;
        }

        /// <summary>
        ///     Clear all runtime parameter values
        /// </summary>
        public void ClearRuntimeValues()
        {
            _runtimeValues.Clear();
            _dirtyParameters.Clear();
        }

      

        #endregion

        #region Flow Transition Management


        #endregion
    }
}
