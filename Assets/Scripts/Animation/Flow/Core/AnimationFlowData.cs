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

        [Tooltip("Type of state (Looping, OneTime, HoldFrame)")]
        public string StateType;

        [Tooltip("Name of the animation to play")]
        public string AnimationName;

        [Tooltip("Whether this is the initial state")]
        public bool IsInitialState;

        [Tooltip("Editor position of the state node")]
        public Vector2 Position;

        [Tooltip("Frame to hold for HoldFrame states")]
        public int FrameToHold;

        /// <summary>
        ///     Create a new animation state data instance
        /// </summary>
        public AnimationStateData()
        {
        }

        /// <summary>
        ///     Create a new animation state data with specific type
        /// </summary>
        public AnimationStateData(string id, string animationName, AnimationStateType stateType)
        {
            Id = id;
            AnimationName = animationName;
            StateType = stateType.ToString();
        }

        /// <summary>
        ///     Get the state type as an enum
        /// </summary>
        public AnimationStateType GetStateType()
        {
            if (Enum.TryParse(StateType, out AnimationStateType stateType))
                return stateType;

            return AnimationStateType.OneTime; // Default fallback
        }
    }
}
