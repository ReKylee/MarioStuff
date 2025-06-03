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
            Debug.Log(
                $"[AnimationNameProvider] Getting animations from IAnimator: {(animator != null ? animator.GetType().Name : "null")}");

            var animations = animator?.GetAvailableAnimations();
            if (animations is { Count: > 0 })
            {
                Debug.Log(
                    $"[AnimationNameProvider] Found {animations.Count} animations: {string.Join(", ", animations)}");

                // Return a sorted copy of the animations
                var sortedList = new List<string>(animations);
                sortedList.Sort();
                return sortedList;
            }

            Debug.LogWarning(
                $"[AnimationNameProvider] No animations found from animator: {(animator != null ? animator.GetType().Name : "null")}");

            // Return default animations if no animator provided or no animations available
            Debug.Log($"[AnimationNameProvider] Returning default animations: {string.Join(", ", DefaultAnimations)}");
            return new List<string>(DefaultAnimations);
        }

        /// <summary>
        ///     Get animation names for a specific GameObject's animator
        /// </summary>
        public static List<string> GetAnimationNamesFromGameObject(GameObject gameObject)
        {
            if (!gameObject)
            {
                Debug.LogWarning("[AnimationNameProvider] Null GameObject provided, returning defaults");
                return new List<string>(DefaultAnimations);
            }

            Debug.Log($"[AnimationNameProvider] Getting animations from GameObject: {gameObject.name}");

            // Try to find an AnimationFlowController on the GameObject
            AnimationFlowController flowController = gameObject.GetComponent<AnimationFlowController>();
            if (flowController)
            {
                Debug.Log($"[AnimationNameProvider] Found AnimationFlowController on {gameObject.name}");
                // Use reflection to access the protected GetAnimatorAdapter method
                MethodInfo methodInfo = flowController.GetType().GetMethod("GetAnimatorAdapter",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                IAnimator animator = methodInfo?.Invoke(flowController, null) as IAnimator;
                if (animator != null)
                {
                    Debug.Log($"[AnimationNameProvider] Got animator from controller: {animator.GetType().Name}");
                    return GetAnimationNames(animator);
                }

                Debug.LogWarning(
                    $"[AnimationNameProvider] Controller.GetAnimator() returned null on {gameObject.name}");
            }
            else
            {
                Debug.Log($"[AnimationNameProvider] No AnimationFlowController found on {gameObject.name}");
            }

            // Try to get a SpriteAnimator component directly
            SpriteAnimator spriteAnimator = gameObject.GetComponent<SpriteAnimator>();
            if (spriteAnimator)
            {
                Debug.Log($"[AnimationNameProvider] Found SpriteAnimator component on {gameObject.name}");
                SpriteAnimatorAdapter adapter = new(spriteAnimator);
                return GetAnimationNames(adapter);
            }

            Debug.Log($"[AnimationNameProvider] No SpriteAnimator found on {gameObject.name}");

            Debug.Log($"[AnimationNameProvider] No animation sources found, returning defaults for {gameObject.name}");
            return new List<string>(DefaultAnimations);
        }

        /// <summary>
        ///     Get animation names for the currently selected GameObject in the editor
        /// </summary>
        public static List<string> GetAnimationNamesFromSelection()
        {
            GameObject selectedObject = Selection.activeGameObject;
            Debug.Log(
                $"[AnimationNameProvider] Getting animations from selection: {(selectedObject ? selectedObject.name : "null")}");

            if (selectedObject)
            {
                return GetAnimationNamesFromGameObject(selectedObject);
            }

            Debug.Log("[AnimationNameProvider] No selection, returning default animations");
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
