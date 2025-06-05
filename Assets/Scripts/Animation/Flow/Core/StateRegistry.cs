using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using Animation.Flow.States;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Central registry for animation states and state types in the system.
    ///     Combines functionality of the previous StateRegistry and StateTypeRegistry.
    /// </summary>
    public static class StateRegistry
    {
        #region Instance Registry

        // Store all registered state instances by their ID
        private static readonly Dictionary<string, IAnimationState> RegisteredStates = new();

        // Store state IDs by their owner (for cleanup)
        private static readonly Dictionary<object, HashSet<string>> StateOwners = new();

        /// <summary>
        ///     Register a state instance with the global registry
        /// </summary>
        public static void RegisterState(IAnimationState state, object owner = null)
        {
            if (state == null || string.IsNullOrEmpty(state.Id))
            {
                Debug.LogWarning("Cannot register null or invalid state");
                return;
            }

            // Register the state
            RegisteredStates[state.Id] = state;

            // Register with owner for cleanup if provided
            if (owner != null)
            {
                if (!StateOwners.TryGetValue(owner, out var ownedStates))
                {
                    ownedStates = new HashSet<string>();
                    StateOwners[owner] = ownedStates;
                }

                ownedStates.Add(state.Id);
            }
        }

        /// <summary>
        ///     Unregister a state from the registry
        /// </summary>
        public static void UnregisterState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId)) return;

            RegisteredStates.Remove(stateId);

            // Clean up owner references
            foreach (var ownerStates in StateOwners.Values)
            {
                ownerStates.Remove(stateId);
            }
        }

        /// <summary>
        ///     Unregister all states owned by a specific owner
        /// </summary>
        public static void UnregisterStatesForOwner(object owner)
        {
            if (owner == null) return;

            if (StateOwners.TryGetValue(owner, out var ownedStates))
            {
                foreach (string stateId in ownedStates)
                {
                    RegisteredStates.Remove(stateId);
                }

                StateOwners.Remove(owner);
            }
        }

        /// <summary>
        ///     Get a state by its ID
        /// </summary>
        public static IAnimationState GetState(string stateId) =>
            RegisteredStates.TryGetValue(stateId, out IAnimationState state) ? state : null;

        /// <summary>
        ///     Get a strongly-typed state by its ID
        /// </summary>
        public static T GetState<T>(string stateId) where T : class, IAnimationState
        {
            if (RegisteredStates.TryGetValue(stateId, out IAnimationState state) && state is T typedState)
            {
                return typedState;
            }

            return null;
        }

        /// <summary>
        ///     Get all registered states
        /// </summary>
        public static IEnumerable<IAnimationState> GetAllStates() => RegisteredStates.Values;

        /// <summary>
        ///     Get all registered state IDs
        /// </summary>
        public static IEnumerable<string> GetAllStateIds() => RegisteredStates.Keys;

        /// <summary>
        ///     Get all states owned by a specific owner
        /// </summary>
        public static IEnumerable<IAnimationState> GetStatesForOwner(object owner)
        {
            if (owner == null || !StateOwners.TryGetValue(owner, out var ownedStates))
            {
                return Array.Empty<IAnimationState>();
            }

            var result = new List<IAnimationState>();
            foreach (string stateId in ownedStates)
            {
                if (RegisteredStates.TryGetValue(stateId, out IAnimationState state))
                {
                    result.Add(state);
                }
            }

            return result;
        }

        /// <summary>
        ///     Check if a state with the given ID exists
        /// </summary>
        public static bool StateExists(string stateId) =>
            !string.IsNullOrEmpty(stateId) && RegisteredStates.ContainsKey(stateId);

        #endregion

        #region State Type Registry

        // Maps Types to creator functions
        private static readonly Dictionary<Type, Func<string, string, IAnimationState>> TypeCreators = 
            new Dictionary<Type, Func<string, string, IAnimationState>>();

        // Maps enum values to implementation types
        private static readonly Dictionary<AnimationStateType, Type> StateTypeMap =
            new Dictionary<AnimationStateType, Type>();

        // Maps string names to enum values (for deserialization)
        private static readonly Dictionary<string, AnimationStateType> StringToEnumMap =
            new Dictionary<string, AnimationStateType>(StringComparer.OrdinalIgnoreCase);

        // Initialize registry with default state types
        static StateRegistry()
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
                StringToEnumMap[stateType.ToString()] = stateType;
            }
        }

        /// <summary>
        ///     Register a new animation state type with the system
        /// </summary>
        public static void RegisterStateType<T>(AnimationStateType stateType, 
            Func<string, string, IAnimationState> creator) where T : IAnimationState
        {
            TypeCreators[typeof(T)] = creator;
            StateTypeMap[stateType] = typeof(T);
        }

        /// <summary>
        ///     Create a state instance based on its type
        /// </summary>
        public static IAnimationState CreateState(AnimationStateType stateType, string id, string animationName)
        {
            // Get the implementation type for this state type
            if (!StateTypeMap.TryGetValue(stateType, out Type implementationType))
            {
                throw new ArgumentException($"Unknown state type: {stateType}");
            }

            // Get the creator function
            if (!TypeCreators.TryGetValue(implementationType, out Func<string, string, IAnimationState> creator))
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

            if (StringToEnumMap.TryGetValue(stateTypeString, out AnimationStateType stateType))
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
            return StateTypeMap.Keys;
        }

        /// <summary>
        ///     Get the state type enum value for a particular implementation type
        /// </summary>
        private static AnimationStateType GetStateTypeForImplementation<T>() where T : IAnimationState
        {
            foreach (var pair in StateTypeMap)
            {
                if (pair.Value == typeof(T))
                {
                    return pair.Key;
                }
            }

            throw new ArgumentException($"No state type registered for implementation: {typeof(T).Name}");
        }

        #endregion

        #region State Factory Methods

        /// <summary>
        ///     Create a state from serialized state data
        /// </summary>
        public static IAnimationState CreateFromData(AnimationStateData stateData)
        {
            if (stateData == null) return null;

            AnimationStateType stateType = stateData.GetStateType();

            try
            {
                // Special case for HoldFrameState which needs additional parameters
                if (stateType == AnimationStateType.HoldFrame)
                {
                    return CreateSpecializedState<HoldFrameState>(
                        stateData.Id, stateData.AnimationName, stateData.FrameToHold);
                }

                // Use registry for standard state types
                return CreateState(stateType, stateData.Id, stateData.AnimationName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"Failed to create state of type {stateType}: {ex.Message}. Creating default OneTime state.");

                return new OneTimeState(stateData.Id, stateData.AnimationName);
            }
        }

        #endregion
    }
}