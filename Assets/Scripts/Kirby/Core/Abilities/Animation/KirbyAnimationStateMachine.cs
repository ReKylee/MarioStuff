using System.Collections.Generic;
using System.Linq;
using GabrielBigardi.SpriteAnimator;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Abilities.Animation
{
    /// <summary>
    ///     Animation state machine for Kirby - manages transitions between animation states
    ///     with a proper state flow system
    /// </summary>
    public class KirbyAnimationStateMachine
    {
        private readonly SpriteAnimator _animator;
        private readonly KirbyController _controller;
        private readonly List<KirbyAnimationState> _states = new();

        // State machine flow tracking
        private KirbyAnimationState _currentState;
        private bool _initialized;
        private KirbyAnimationState _pendingState;
        private bool _stateChangedThisFrame;

        public KirbyAnimationStateMachine(KirbyController controller, SpriteAnimator animator)
        {
            _controller = controller;
            _animator = animator;
        }

        /// <summary>
        ///     Register animation states with the state machine
        /// </summary>
        public void RegisterStates(IEnumerable<KirbyAnimationState> states)
        {
            foreach (KirbyAnimationState state in states)
            {
                RegisterState(state);
            }

            _initialized = true;

            // Set initial state to Idle if available
            if (_currentState == null)
            {
                KirbyAnimationState idleState = _states.FirstOrDefault(s => s.AnimationName == "Idle");
                if (idleState != null)
                {
                    _currentState = idleState;
                    _currentState.Enter();
                }
            }
        }

        /// <summary>
        ///     Register a single animation state
        /// </summary>
        public void RegisterState(KirbyAnimationState state)
        {
            state.Initialize(_controller, _animator);
            _states.Add(state);
        }

        /// <summary>
        ///     Clean up resources when state machine is destroyed
        /// </summary>
        public void Cleanup()
        {
            // Exit current state properly
            _currentState?.Exit();
            _currentState = null;
            _pendingState = null;
            _states.Clear();
            _initialized = false;
        }

        /// <summary>
        ///     Update the state machine, evaluating state conditions
        /// </summary>
        public void Update(InputContext input)
        {
            if (!_initialized || _states.Count == 0)
                return;

            _stateChangedThisFrame = false;

            // First, update the current state
            _currentState?.Update(input);

            // Find all states that should be active based on current conditions
            var candidateStates = _states.Where(s => s.ShouldBeActive(input)).ToList();

            // If no states should be active, try to find a fallback idle state
            if (candidateStates.Count == 0)
            {
                KirbyAnimationState idleState = _states.FirstOrDefault(s => s.AnimationName == "Idle");
                if (idleState != null)
                {
                    candidateStates.Add(idleState);
                }
            }

            if (candidateStates.Count > 0)
            {
                // Find the highest priority state among candidates
                KirbyAnimationState highestPriorityState = candidateStates.OrderByDescending(s => s.Priority).First();

                // Check if we should transition to this state
                if (_currentState != highestPriorityState)
                {
                    // Only transition if the current state can be interrupted
                    if (_currentState == null || _currentState.CanBeInterrupted)
                    {
                        _pendingState = highestPriorityState;
                    }
                }
            }
        }

        /// <summary>
        ///     Apply any pending state change at the end of the frame
        /// </summary>
        public void ApplyCurrentState()
        {
            // If we have a pending state change, apply it
            if (_pendingState != null && _pendingState != _currentState)
            {
                // Exit current state
                _currentState?.Exit();

                // Enter new state
                _currentState = _pendingState;
                _currentState.Enter();

                _pendingState = null;
                _stateChangedThisFrame = true;
            }
        }

        /// <summary>
        ///     Flip the sprite based on direction
        /// </summary>
        public void SetDirection(int direction)
        {
            if (direction == 0 || _animator == null)
                return;

            if (_animator.transform.localScale.x != direction)
            {
                Vector3 scale = _animator.transform.localScale;
                scale.x = direction;
                _animator.transform.localScale = scale;
            }
        }

        /// <summary>
        ///     Get the current animation state
        /// </summary>
        public KirbyAnimationState GetCurrentState() => _currentState;

        /// <summary>
        ///     Force a transition to a specific state by name
        /// </summary>
        public bool ForceState(string stateName)
        {
            KirbyAnimationState state = _states.FirstOrDefault(s => s.AnimationName == stateName);
            if (state != null)
            {
                _pendingState = state;
                ApplyCurrentState();
                return true;
            }

            return false;
        }
    }
}
