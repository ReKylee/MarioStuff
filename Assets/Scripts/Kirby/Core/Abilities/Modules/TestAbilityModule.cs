using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    ///     Handles horizontal movement with acceleration/deceleration for a responsive feel
    /// </summary>
    public class TestAbilityModule : AbilityModuleBase, IAttackAbilityModule
    {

        public float AttackRange { get; }
        public float BaseDamage { get; }
        public float AttackCooldown { get; }
        public bool IsOnCooldown { get; }
        public AttackType AttackType { get; }
        public void PerformAttack(Vector2 direction)
        {
        }
        public bool PerformSecondaryAttack(Vector2 direction) => false;
    }
}
