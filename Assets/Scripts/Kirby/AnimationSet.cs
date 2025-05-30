using System;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    /// Scriptable Object to hold animation clips for a Kirby form
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimationSet", menuName = "Kirby/Animation Set")]
    public class AnimationSet : ScriptableObject
    {
        [Header("Ground Animations")]
        public AnimationClip idle;
        public AnimationClip walk;
        public AnimationClip run;
        public AnimationClip crouch;
        
        [Header("Slope Animations")]
        public AnimationClip slopeLeft;
        public AnimationClip slopeRight;
        public AnimationClip deepSlopeLeft;
        public AnimationClip deepSlopeRight;
        
        [Header("Aerial Animations")]
        public AnimationClip jump;
        public AnimationClip jumpToFly; // Transition animation
        public AnimationClip fly;
        public AnimationClip flyEnd; // Transition animation
        public AnimationClip fall;
        
        [Header("Action Animations")]
        public AnimationClip inhale;
        public AnimationClip spit;
        public AnimationClip swallow;
        
        [Header("Full Mouth Animations")]
        public AnimationClip fullIdle;
        public AnimationClip fullRun;
        public AnimationClip fullJump;
        public AnimationClip fullFall;
        public AnimationClip fullSlopeLeft;
        public AnimationClip fullSlopeRight;
        public AnimationClip fullDeepSlopeLeft;
        public AnimationClip fullDeepSlopeRight;
        
        [Header("Other Animations")]
        public AnimationClip guard; // Defensive stance
        
        [Header("Squashed Animations (for Crouch on slopes)")]
        public AnimationClip squashedSlopeLeft;
        public AnimationClip squashedSlopeRight;
        public AnimationClip squashedDeepSlopeLeft;
        public AnimationClip squashedDeepSlopeRight;
        
        /// <summary>
        /// Gets the appropriate animation clip based on the state name and conditions
        /// </summary>
        /// <param name="stateName">Base state name (e.g., "Idle", "Run")</param>
        /// <param name="isFull">Whether Kirby has something in his mouth</param>
        /// <param name="terrainAngle">Angle of the terrain (-180 to 180)</param>
        /// <param name="isCrouching">Whether Kirby is crouching</param>
        /// <returns>The appropriate animation clip or null if not found</returns>
        public AnimationClip GetAnimationForState(string stateName, bool isFull, float terrainAngle, bool isCrouching)
        {
            // Check if we're on a slope
            bool isOnSlope = Mathf.Abs(terrainAngle) > 10f && Mathf.Abs(terrainAngle) < 45f;
            bool isOnDeepSlope = Mathf.Abs(terrainAngle) >= 45f;
            bool isLeftSlope = terrainAngle > 0; // Positive angle means left slope in Unity 2D
            
            // Handle crouch on slopes specially
            if (isCrouching && isOnSlope)
            {
                if (isOnDeepSlope)
                {
                    return isLeftSlope ? squashedDeepSlopeLeft : squashedDeepSlopeRight;
                }
                else
                {
                    return isLeftSlope ? squashedSlopeLeft : squashedSlopeRight;
                }
            }
            
            // Handle full mouth states
            if (isFull)
            {
                if (isOnDeepSlope)
                {
                    return isLeftSlope ? fullDeepSlopeLeft : fullDeepSlopeRight;
                }
                else if (isOnSlope)
                {
                    return isLeftSlope ? fullSlopeLeft : fullSlopeRight;
                }
                
                switch (stateName)
                {
                    case "Idle": return fullIdle;
                    case "Run": 
                    case "Walk": return fullRun;
                    case "Jump": return fullJump;
                    case "Fall": return fullFall;
                    default: return fullIdle; // Default if no specific full animation
                }
            }
            
            // Handle regular states on slopes
            if (!isCrouching && (stateName == "Idle" || stateName == "Crouch"))
            {
                if (isOnDeepSlope)
                {
                    return isLeftSlope ? deepSlopeLeft : deepSlopeRight;
                }
                else if (isOnSlope)
                {
                    return isLeftSlope ? slopeLeft : slopeRight;
                }
            }
            
            // Handle standard animations
            switch (stateName)
            {
                case "Idle": return idle;
                case "Walk": return walk;
                case "Run": return run;
                case "Jump": return jump;
                case "JumpToFly": return jumpToFly;
                case "Fly": return fly;
                case "FlyEnd": return flyEnd;
                case "Fall": return fall;
                case "Inhale": return inhale;
                case "Spit": return spit;
                case "Swallow": return swallow;
                case "Crouch": return crouch;
                case "Guard": return guard;
                default: return idle; // Default fallback
            }
        }
    }
}
