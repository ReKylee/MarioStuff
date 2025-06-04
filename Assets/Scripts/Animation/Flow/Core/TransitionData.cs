using System;
using System.Collections.Generic;
using Animation.Flow.Conditions;
using Animation.Flow.States;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Serializable data for a state transition
    /// </summary>
    [Serializable]
    public class TransitionData
    {
        [Tooltip("ID of the source state")] public string FromStateId;

        [Tooltip("ID of the destination state")]
        public string ToStateId;

        [Tooltip("Type of source state (for validation)")] [SerializeField]
        private AnimationStateType _fromStateType = AnimationStateType.OneTime;

        [Tooltip("Type of destination state (for validation)")] [SerializeField]
        private AnimationStateType _toStateType = AnimationStateType.OneTime;

        [Tooltip("Conditions that must be met for this transition")]
        public List<ConditionData> Conditions = new();

        [Tooltip("Editor position of the transition connection")]
        public Vector2[] ConnectionPoints;

        /// <summary>
        ///     Create an empty transition data object
        /// </summary>
        public TransitionData()
        {
        }

        /// <summary>
        ///     Create a transition data object with the specified source and destination states
        /// </summary>
        public TransitionData(string fromStateId, string toStateId)
        {
            FromStateId = fromStateId;
            ToStateId = toStateId;
        }

        /// <summary>
        ///     Create a fully qualified transition data object
        /// </summary>
        public TransitionData(string fromStateId, AnimationStateType fromStateType,
            string toStateId, AnimationStateType toStateType)
        {
            FromStateId = fromStateId;
            ToStateId = toStateId;
            _fromStateType = fromStateType;
            _toStateType = toStateType;
        }

        /// <summary>
        ///     Type of source state
        /// </summary>
        public AnimationStateType FromStateType
        {
            get => _fromStateType;
            set => _fromStateType = value;
        }

        /// <summary>
        ///     Type of destination state
        /// </summary>
        public AnimationStateType ToStateType
        {
            get => _toStateType;
            set => _toStateType = value;
        }

        /// <summary>
        ///     Add a condition to this transition
        /// </summary>
        public void AddCondition(ConditionData condition)
        {
            if (condition != null)
            {
                Conditions.Add(condition);
            }
        }

        /// <summary>
        ///     Create a deep copy of this transition data
        /// </summary>
        public TransitionData Clone()
        {
            TransitionData clone = new()
            {
                FromStateId = FromStateId,
                ToStateId = ToStateId
            };

            // Clone conditions
            if (Conditions != null && Conditions.Count > 0)
            {
                clone.Conditions = new List<ConditionData>();
                foreach (ConditionData condition in Conditions)
                {
                    clone.Conditions.Add(condition.Clone());
                }
            }

            // Clone connection points if they exist
            if (ConnectionPoints != null && ConnectionPoints.Length > 0)
            {
                clone.ConnectionPoints = new Vector2[ConnectionPoints.Length];
                Array.Copy(ConnectionPoints, clone.ConnectionPoints, ConnectionPoints.Length);
            }

            return clone;
        }
    }
}
