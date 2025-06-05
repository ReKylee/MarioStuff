using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Implementation of the animation context with strong typing for parameters
    /// </summary>
    [Serializable]
    public class AnimationContext : IAnimationContext
    {

        #region Properties

        /// <summary>
        ///     Gets the animator instance
        /// </summary>
        public IAnimator Animator => _animator;

        #endregion


        #region Runtime Fields

        private readonly Dictionary<string, object> _runtimeValues = new();

        // The animator instance
        [NonSerialized] private IAnimator _animator;

        #endregion

        #region Constructors

        public AnimationContext()
        {
        }

        public AnimationContext(IAnimator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
        }

        #endregion


        #region Parameter Management

        /// <summary>
        ///     Set the animator instance
        /// </summary>
        public void SetAnimator(IAnimator animator)
        {
            _animator = animator;

        }
        /// <summary>
        ///     Check if a parameter exists
        /// </summary>
        /// <param name="parameterName">Name of the parameter to check</param>
        /// <returns>True if the parameter exists, false otherwise</returns>
        public bool HasParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return false;

            return _runtimeValues.ContainsKey(parameterName);
        }

        /// <summary>
        ///     Get a parameter value with strong typing
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="parameterName">Name of the parameter</param>
        /// <returns>The parameter value or default(T) if not found</returns>
        public T GetParameter<T>(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return default;

            if (_runtimeValues.TryGetValue(parameterName, out object value))
            {
                try
                {
                    // Handle type conversion
                    if (value is T directValue)
                        return directValue;

                    // Try to convert the value to the requested type
                    if (value != null)
                        return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"Failed to convert parameter '{parameterName}' from {value?.GetType()?.Name} to {typeof(T).Name}: {ex.Message}");
                }
            }

            return default;
        }

        /// <summary>
        ///     Set a parameter value with strong typing
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">Value to set</param>
        public void SetParameter<T>(string parameterName, T value)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                Debug.LogWarning("Cannot set parameter with null or empty name");
                return;
            }

            _runtimeValues[parameterName] = value;
        }

        /// <summary>
        ///     Remove a parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter to remove</param>
        /// <returns>True if the parameter was removed, false if it didn't exist</returns>
        public bool RemoveParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return false;

            return _runtimeValues.Remove(parameterName);
        }


        /// <summary>
        ///     Clear all runtime parameter values
        /// </summary>
        public void ClearRuntimeValues()
        {
            _runtimeValues.Clear();
        }

        #endregion

    }
}
