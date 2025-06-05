using System;
using System.Collections.Generic;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Central registry of available animation states in the system
    /// </summary>
    public static class StateRegistry
    {
        // Store all registered states by their ID
        private static readonly Dictionary<string, IAnimationState> RegisteredStates = new();

        // Store state IDs by their owner (for cleanup)
        private static readonly Dictionary<object, HashSet<string>> StateOwners = new();

        /// <summary>
        ///     Register a state with the global registry
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
    }
}
