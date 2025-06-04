using System;
using Animation.Flow.States;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Serializable data for an animation state
    /// </summary>
    [Serializable]
    public class AnimationStateData
    {
        [Tooltip("Unique identifier for this state")]
        public string Id;

        [Tooltip("Type of state (Looping, OneTime, HoldFrame)")] [SerializeField]
        private AnimationStateType _stateType = AnimationStateType.OneTime;

        [Tooltip("Name of the animation to play")]
        public string AnimationName;

        [Tooltip("Whether this is the initial state")]
        public bool IsInitialState;

        [Tooltip("Editor position of the state node")]
        public Vector2 Position;

        [Tooltip("Frame to hold (only used by HoldFrame state type)")] [SerializeField]
        private int _frameToHold;

        /// <summary>
        ///     Create a new animation state data with specific type
        /// </summary>
        public AnimationStateData(string id, string animationName, AnimationStateType stateType, int frameToHold = 0)
        {
            Id = id;
            AnimationName = animationName;
            _frameToHold = frameToHold;
            _stateType = stateType;
        }

        public AnimationStateData()
        {
        }

        /// <summary>
        ///     Frame index to hold for HoldFrame states
        /// </summary>
        public int FrameToHold
        {
            get => _frameToHold;
            set => _frameToHold = value;
        }

        /// <summary>
        ///     Type of this animation state
        /// </summary>
        public AnimationStateType StateType
        {
            get => _stateType;
            set => _stateType = value;
        }

        /// <summary>
        ///     Get the state type (for backward compatibility)
        /// </summary>
        public AnimationStateType GetStateType() => _stateType;

        /// <summary>
        ///     Clone this state data
        /// </summary>
        public AnimationStateData Clone() =>
            new()
            {
                Id = Id,
                _stateType = _stateType,
                AnimationName = AnimationName,
                IsInitialState = IsInitialState,
                Position = Position,
                _frameToHold = _frameToHold
            };
    }
}
