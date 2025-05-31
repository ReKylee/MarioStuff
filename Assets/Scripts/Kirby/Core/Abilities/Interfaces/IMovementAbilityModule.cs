using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Interface for abilities that modify movement mechanics
    /// </summary>
    public interface IMovementAbilityModule : IAbilityModule
    {
        /// <summary>
        ///     Allows the ability to make final adjustments to KirbyStats after CopyAbilityData modifiers have been applied.
        ///     For example, setting specific behavioral flags or stats inherent to this ability.
        /// </summary>
        void FinalizeStats(KirbyStats stats);

        /// <summary>
        ///     Process movement modifications.
        /// </summary>
        /// <param name="currentVelocity">Current velocity before modification</param>
        /// <param name="targetVelocity">Target velocity based on input and base stats</param>
        /// <param name="isGrounded">Whether Kirby is on the ground</param>
        /// <returns>Modified velocity</returns>
        Vector2 ProcessMovement(Vector2 currentVelocity, Vector2 targetVelocity, bool isGrounded);
    }
}
