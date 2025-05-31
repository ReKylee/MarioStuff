using System.Collections.Generic;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    /// </summary>
    public class WalkAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        [Header("Movement Settings - Walk Specific")]
        [Tooltip("Additional modifiers specific to walking, e.g., different acceleration on slopes.")]
        [SerializeField]
        private List<StatModifier> walkSpecificModifiers = new();


        // IMovementAbility Implementation
        public void FinalizeStats(KirbyStats stats)
        {
            // Apply walk-specific modifiers
            foreach (StatModifier modifier in walkSpecificModifiers)
            {
                float currentValue = stats.GetStat(modifier.statType);
                float newValue = modifier.ApplyModifier(currentValue);
                stats.SetStat(modifier.statType, newValue);
            }
        }

        public Vector2 ProcessMovement(Vector2 currentVelocity, Vector2 targetVelocity, bool isGrounded)
        {
            if (!Controller || Controller.Stats == null) return currentVelocity;

            float acceleration = isGrounded ? Controller.Stats.groundAcceleration : Controller.Stats.airAcceleration;
            float deceleration = isGrounded ? Controller.Stats.groundDeceleration : Controller.Stats.airDeceleration;

            // Determine move speed (walk or run)
            // This is a simplified check. You might have a dedicated run input or other logic to switch to runSpeed.
            // For now, if current horizontal speed is already greater than walkSpeed (implying running) 
            // and there's horizontal input, use runSpeed. Otherwise, use walkSpeed.
            float moveSpeed;
            if (Mathf.Abs(Controller.Rigidbody.linearVelocity.x) > Controller.Stats.walkSpeed &&
                Mathf.Abs(targetVelocity.x) > 0.01f)
            {
                moveSpeed = Controller.Stats.runSpeed;
            }
            else
            {
                moveSpeed = Controller.Stats.walkSpeed;
            }

            // If there's input, accelerate towards target velocity scaled by the chosen moveSpeed
            if (Mathf.Abs(targetVelocity.x) > 0.01f)
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocity.x * moveSpeed,
                    acceleration * Time.deltaTime);
            }
            // If no input, decelerate
            else
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0, deceleration * Time.deltaTime);
            }

            return currentVelocity;
        }
    }
}
