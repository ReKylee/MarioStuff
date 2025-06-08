using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    /// Interface for animation state machines to handle state transitions
    /// </summary>
    public interface IAnimationStateMachine
    {
        /// <summary>
        /// Updates the current animation state based on input and character state
        /// </summary>
        /// <param name="input">Current input context</param>
        /// <param name="isGrounded">Whether Kirby is on the ground</param>
        /// <param name="velocity">Current vertical velocity</param>
        void UpdateState(InputContext input, bool isGrounded, Vector2 velocity);

        /// <summary>
        /// Called when an animation completes
        /// </summary>
        void OnAnimationComplete();

        /// <summary>
        /// Gets the current animation state
        /// </summary>
        AnimState GetCurrentState();
    }
}
