using System;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    /// Defines all available animation state types
    /// </summary>
    public enum AnimationStateType
    {
        /// <summary>
        /// One-time animation that plays and stops
        /// </summary>
        OneTime,
        
        /// <summary>
        /// Looping animation that repeats
        /// </summary>
        Looping,
        
        /// <summary>
        /// Holds on a specific frame
        /// </summary>
        HoldFrame
    }
    
    /// <summary>
    /// Defines all available transition condition types
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Boolean parameter condition
        /// </summary>
        Bool,
        
        /// <summary>
        /// Float equals value condition (with small threshold)
        /// </summary>
        FloatEquals,
        
        /// <summary>
        /// Float less than value condition
        /// </summary>
        FloatLessThan,
        
        /// <summary>
        /// Float greater than value condition
        /// </summary>
        FloatGreaterThan,
        
        /// <summary>
        /// Animation complete condition
        /// </summary>
        AnimationComplete,
        
        /// <summary>
        /// Time elapsed condition
        /// </summary>
        TimeElapsed,
        
        /// <summary>
        /// String equals condition
        /// </summary>
        StringEquals,
        
        /// <summary>
        /// OR compound condition
        /// </summary>
        AnyCondition,
        
        /// <summary>
        /// AND compound condition
        /// </summary>
        AllCondition
    }
    
    /// <summary>
    /// Extension methods for converting between enum values and string representations
    /// </summary>
    public static class AnimationFlowExtensions
    {
        /// <summary>
        /// Convert string type name to AnimationStateType enum
        /// </summary>
        public static AnimationStateType ToAnimationStateType(this string typeName)
        {
            if (Enum.TryParse<AnimationStateType>(typeName, out var stateType))
                return stateType;
                
            Debug.LogWarning($"Invalid animation state type: {typeName}, falling back to OneTime");
            return AnimationStateType.OneTime;
        }
        
        /// <summary>
        /// Convert AnimationStateType enum to string
        /// </summary>
        public static string ToString(this AnimationStateType stateType) => stateType.ToString();
        
        /// <summary>
        /// Convert string condition type to ConditionType enum
        /// </summary>
        public static ConditionType ToConditionType(this string typeName)
        {
            if (Enum.TryParse<ConditionType>(typeName, out var conditionType))
                return conditionType;
                
            Debug.LogWarning($"Invalid condition type: {typeName}, falling back to Bool");
            return ConditionType.Bool;
        }
        
        /// <summary>
        /// Convert ConditionType enum to string
        /// </summary>
        public static string ToString(this ConditionType conditionType) => conditionType.ToString();
    }
}
