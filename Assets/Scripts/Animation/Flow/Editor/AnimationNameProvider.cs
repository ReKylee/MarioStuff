using System.Collections.Generic;
using System.Reflection;
using Animation.Flow.Adapters;
using GabrielBigardi.SpriteAnimator;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Editor
{
    /// <summary>
    ///     Provides animation names to the Animation Flow Editor system
    /// </summary>
    public static class AnimationNameProvider
    {
        private static readonly List<string> DefaultAnimations = new()
        {
            "Idle",
            "Walk",
            "Run",
            "Jump",
            "Fall"
        };

        /// <summary>
        ///     Get animation names from a specific animator
        /// </summary>
        public static List<string> GetAnimationNames(IAnimator animator)
        {
            var animations = animator?.GetAvailableAnimations();
            if (animations is { Count: > 0 })
            {
                // Return a sorted copy of the animations
                var sortedList = new List<string>(animations);
                sortedList.Sort();
                return sortedList;
            }

            // Return default animations if no animator provided or no animations available
            return new List<string>(DefaultAnimations);
        }

        /// <summary>
        ///     Get animation names for a specific GameObject's animator
        /// </summary>
        public static List<string> GetAnimationNamesFromGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return new List<string>(DefaultAnimations);

            // Try to find an AnimationFlowController on the GameObject
            AnimationFlowController flowController = gameObject.GetComponent<AnimationFlowController>();
            if (flowController)
            {
                // Use reflection to access the protected GetAnimatorAdapter method
                MethodInfo methodInfo = flowController.GetType().GetMethod("GetAnimatorAdapter",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                IAnimator animator = methodInfo?.Invoke(flowController, null) as IAnimator;
                if (animator != null)
                {
                    return GetAnimationNames(animator);
                }
            }

            // Try to get a SpriteAnimator component directly
            SpriteAnimator spriteAnimator = gameObject.GetComponent<SpriteAnimator>();
            if (spriteAnimator)
            {
                SpriteAnimatorAdapter adapter = new(spriteAnimator);
                return GetAnimationNames(adapter);
            }

            return new List<string>(DefaultAnimations);
        }

        /// <summary>
        ///     Get animation names for the currently selected GameObject in the editor
        /// </summary>
        public static List<string> GetAnimationNamesFromSelection()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject)
            {
                return GetAnimationNamesFromGameObject(selectedObject);
            }

            return new List<string>(DefaultAnimations);
        }

        /// <summary>
        ///     Add default animation names
        /// </summary>
        public static void AddDefaultAnimation(string animationName)
        {
            if (!string.IsNullOrEmpty(animationName) && !DefaultAnimations.Contains(animationName))
            {
                DefaultAnimations.Add(animationName);
                DefaultAnimations.Sort();
            }
        }
    }
}
