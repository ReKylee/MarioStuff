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
        private static readonly Dictionary<string, IAnimationState> _registeredStates = 
            new Dictionary<string, IAnimationState>();

        // Store state IDs by their owner (for cleanup)
        private static readonly Dictionary<object, HashSet<string>> _stateOwners =
            new Dictionary<object, HashSet<string>>();

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
            _registeredStates[state.Id] = state;

            // Register with owner for cleanup if provided
            if (owner != null)
            {
                if (!_stateOwners.TryGetValue(owner, out var ownedStates))
                {
                    ownedStates = new HashSet<string>();
                    _stateOwners[owner] = ownedStates;
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

            _registeredStates.Remove(stateId);

            // Clean up owner references
            foreach (var ownerStates in _stateOwners.Values)
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

            if (_stateOwners.TryGetValue(owner, out var ownedStates))
            {
                foreach (var stateId in ownedStates)
                {
                    _registeredStates.Remove(stateId);
                }

                _stateOwners.Remove(owner);
            }
        }

        /// <summary>
        ///     Get a state by its ID
        /// </summary>
        public static IAnimationState GetState(string stateId)
        {
            return _registeredStates.TryGetValue(stateId, out var state) ? state : null;
        }

        /// <summary>
        ///     Get a strongly-typed state by its ID
        /// </summary>
        public static T GetState<T>(string stateId) where T : class, IAnimationState
        {
            if (_registeredStates.TryGetValue(stateId, out var state) && state is T typedState)
            {
                return typedState;
            }

            return null;
        }

        /// <summary>
        ///     Get all registered states
        /// </summary>
        public static IEnumerable<IAnimationState> GetAllStates()
        {
            return _registeredStates.Values;
        }

        /// <summary>
        ///     Get all registered state IDs
        /// </summary>
        public static IEnumerable<string> GetAllStateIds()
        {
            return _registeredStates.Keys;
        }

        /// <summary>
        ///     Get all states owned by a specific owner
        /// </summary>
        public static IEnumerable<IAnimationState> GetStatesForOwner(object owner)
        {
            if (owner == null || !_stateOwners.TryGetValue(owner, out var ownedStates))
            {
                return Array.Empty<IAnimationState>();
            }

            List<IAnimationState> result = new List<IAnimationState>();
            foreach (var stateId in ownedStates)
            {
                if (_registeredStates.TryGetValue(stateId, out var state))
                {
                    result.Add(state);
                }
            }

            return result;
        }

        /// <summary>
        ///     Check if a state with the given ID exists
        /// </summary>
        public static bool StateExists(string stateId)
        {
            return !string.IsNullOrEmpty(stateId) && _registeredStates.ContainsKey(stateId);
        }
    }
}
