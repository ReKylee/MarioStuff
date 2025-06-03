using System;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    /// Serializable data for an animation state
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
        /// Get the state type as an enum
        /// </summary>
        public AnimationStateType GetStateType() => StateType.ToAnimationStateType();
    }

    /// <summary>
    /// Serializable data for a transition between states
    /// </summary>
    [Serializable]
    public class TransitionData
    {
        [Tooltip("ID of the source state")] 
        public string FromStateId;

        [Tooltip("ID of the destination state")]
        public string ToStateId;

        [Tooltip("Conditions that trigger this transition")]
        public System.Collections.Generic.List<ConditionData> Conditions = new();
    }

    /// <summary>
    /// Serializable data for a transition condition
    /// </summary>
    [Serializable]
    public class ConditionData
    {
        [Tooltip("Type of condition")] 
        public string Type;

        [Tooltip("Name of the parameter to check")]
        public string ParameterName;

        [Tooltip("Boolean value for Bool conditions")]
        public bool BoolValue;

        [Tooltip("Float value for numeric conditions")]
        public float FloatValue;

        [Tooltip("String value for string conditions")]
        public string StringValue;
        
        /// <summary>
        /// Get the condition type as an enum
        /// </summary>
        public ConditionType GetConditionType() => Type.ToConditionType();
    }
}
