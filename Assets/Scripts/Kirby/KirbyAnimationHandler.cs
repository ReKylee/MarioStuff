using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Implementation of IAnimationHandler for Kirby
    /// </summary>
    public class KirbyAnimationHandler : IAnimationHandler
    {
        private readonly Animator animator;
        private AnimationSet animationSet;
        private bool isCrouching;

        // State tracking
        private bool isFull;

        public KirbyAnimationHandler(Animator animator, AnimationSet animationSet)
        {
            this.animator = animator;
            this.animationSet = animationSet;
        }

        /// <summary>
        ///     Plays an animation with the given name
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            animator.Play(animationName);
        }

        /// <summary>
        ///     Determines the appropriate animation for the current terrain angle, full status, and crouch status.
        /// </summary>
        public string GetTerrainAdjustedAnimation(string baseAnimationName, float terrainAngle, bool isFullParam,
            bool isCrouchingParam)
        {
            // Get the animation clip based on the state, full status, terrain angle, and crouch status
            AnimationClip animClip =
                animationSet.GetAnimationForState(baseAnimationName, isFullParam, terrainAngle, isCrouchingParam);

            // Return the animation name if found, otherwise return the base name
            return animClip != null ? animClip.name : baseAnimationName;
        }

        /// <summary>
        ///     Checks if a specific animation is currently playing
        /// </summary>
        public bool IsPlayingAnimation(string animationName)
        {
            // Check current animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName);
        }

        /// <summary>
        ///     Sets whether Kirby has something in his mouth
        /// </summary>
        public void SetFullStatus(bool isFull)
        {
            this.isFull = isFull;
        }

        /// <summary>
        ///     Sets whether Kirby is crouching
        /// </summary>
        public void SetCrouchStatus(bool isCrouching)
        {
            this.isCrouching = isCrouching;
        }

        /// <summary>
        ///     Sets the animation set to use (used when changing forms)
        /// </summary>
        public void SetAnimationSet(AnimationSet newAnimationSet)
        {
            animationSet = newAnimationSet;
        }
    }
}
