using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    ///     Implementation of the animation context with strong typing for parameters
    /// </summary>
    public class AnimationContext : IAnimationContext
    {
        private readonly Dictionary<string, object> _parameters = new();
        private readonly Dictionary<string, Type> _parameterTypes = new();

        public AnimationContext(IAnimator animator, GameObject entity)
        {
            Animator = animator ?? throw new ArgumentNullException(nameof(animator));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public IAnimator Animator { get; }
        public GameObject Entity { get; }

        public T GetParameter<T>(string name)
        {
            if (_parameters.TryGetValue(name, out object value))
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

        public void SetParameter<T>(string name, T value)
        {
            _parameters[name] = value;
            _parameterTypes[name] = typeof(T);
        }

        public bool HasParameter(string name) => _parameters.ContainsKey(name);

        public Type GetParameterType(string name) => _parameterTypes.TryGetValue(name, out Type type) ? type : null;
    }
}
