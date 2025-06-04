using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using Animation.Flow.States;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Registry for available animation state types
    /// </summary>
    public static class StateTypeRegistry
    {
        private static readonly Dictionary<Type, Func<string, string, IAnimationState>> _creators = 
            new Dictionary<Type, Func<string, string, IAnimationState>>();

        private static readonly Dictionary<AnimationStateType, Type> _stateTypeMap =
            new Dictionary<AnimationStateType, Type>();

        private static readonly Dictionary<string, AnimationStateType> _stringToEnumMap =
            new Dictionary<string, AnimationStateType>(StringComparer.OrdinalIgnoreCase);

        // Initialize registry with default state types
        static StateTypeRegistry()
        {
            // Register built-in state types
            RegisterStateType<OneTimeState>(AnimationStateType.OneTime, 
                (id, animName) => new OneTimeState(id, animName));

            RegisterStateType<LoopingState>(AnimationStateType.Looping, 
                (id, animName) => new LoopingState(id, animName));

            RegisterStateType<HoldFrameState>(AnimationStateType.HoldFrame, 
                (id, animName) => new HoldFrameState(id, animName, 0));

            // Initialize string to enum mapping
            foreach (AnimationStateType stateType in Enum.GetValues(typeof(AnimationStateType)))
            {
                _stringToEnumMap[stateType.ToString()] = stateType;
            }
        }

        /// <summary>
        ///     Register a new animation state type with the system
        /// </summary>
        public static void RegisterStateType<T>(AnimationStateType stateType, 
            Func<string, string, IAnimationState> creator) where T : IAnimationState
        {
            _creators[typeof(T)] = creator;
            _stateTypeMap[stateType] = typeof(T);
        }

        /// <summary>
        ///     Create a state instance based on its type
        /// </summary>
        public static IAnimationState CreateState(AnimationStateType stateType, string id, string animationName)
        {
            // Get the implementation type for this state type
            if (!_stateTypeMap.TryGetValue(stateType, out Type implementationType))
            {
                throw new ArgumentException($"Unknown state type: {stateType}");
            }

            // Get the creator function
            if (!_creators.TryGetValue(implementationType, out Func<string, string, IAnimationState> creator))
            {
                throw new InvalidOperationException($"No creator registered for state type: {stateType}");
            }

            // Create the state
            return creator(id, animationName);
        }

        /// <summary>
        ///     Create an instance of a specialized state type
        /// </summary>
        public static T CreateSpecializedState<T>(string id, string animationName, params object[] additionalParams) 
            where T : IAnimationState
        {
            // Implementation would depend on the specialized state type
            // For now, just handle HoldFrameState as an example
            if (typeof(T) == typeof(HoldFrameState) && additionalParams.Length > 0 && additionalParams[0] is int frameIndex)
            {
                return (T)(IAnimationState)new HoldFrameState(id, animationName, frameIndex);
            }

            // Default fallback
            return (T)CreateState(GetStateTypeForImplementation<T>(), id, animationName);
        }

        /// <summary>
        ///     Parse a string state type to the enum value
        /// </summary>
        public static AnimationStateType ParseStateType(string stateTypeString)
        {
            if (string.IsNullOrEmpty(stateTypeString))
            {
                return AnimationStateType.OneTime; // Default fallback
            }

            if (_stringToEnumMap.TryGetValue(stateTypeString, out AnimationStateType stateType))
            {
                return stateType;
            }

            // Try enum parsing as fallback
            if (Enum.TryParse(stateTypeString, true, out AnimationStateType parsedType))
            {
                return parsedType;
            }

            return AnimationStateType.OneTime; // Default fallback
        }

        /// <summary>
        ///     Get all registered state types
        /// </summary>
        public static IEnumerable<AnimationStateType> GetRegisteredStateTypes()
        {
            return _stateTypeMap.Keys;
        }

        /// <summary>
        ///     Get the state type enum value for a particular implementation type
        /// </summary>
        private static AnimationStateType GetStateTypeForImplementation<T>() where T : IAnimationState
        {
            foreach (var pair in _stateTypeMap)
            {
                if (pair.Value == typeof(T))
                {
                    return pair.Key;
                }
            }

            throw new ArgumentException($"No state type registered for implementation: {typeof(T).Name}");
        }
    }
}
