using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Base class for all ability ScriptableObjects
    /// </summary>
    public abstract class AbilityModuleBase : ScriptableObject, IAbilityModule
    {

        [Header("Ability-Specific Modifiers")] [SerializeField]
        private List<StatModifier> abilityDefinedModifiers = new(); // Renamed from inherentModifiers

        // Reference to the controller using this ability
        protected KirbyController Controller { get; private set; }
        protected KirbyGroundCheck GroundCheck { get; private set; }
        protected Rigidbody2D Rigidbody { get; private set; }

        // Properties
        [field: Header("Basic Information")] public string AbilityID { get; } = "ability_id";

        public string DisplayName { get; } = "Ability Name";

        /// <summary>
        ///     Initialize the ability with controller reference
        /// </summary>
        public virtual void Initialize(KirbyController controller)
        {
            Controller = controller;
            GroundCheck = controller.GroundCheck;
            Rigidbody = controller.Rigidbody;
        }

        /// <summary>
        ///     Called when the ability becomes active
        /// </summary>
        public virtual void OnActivate()
        {
            // Base implementation does nothing
        }

        /// <summary>
        ///     Called when the ability becomes inactive
        /// </summary>
        public virtual void OnDeactivate()
        {
            // Base implementation does nothing
        }

        /// <summary>
        ///     Update logic for the ability
        /// </summary>
        public virtual void ProcessAbility()
        {
            // Base implementation does nothing
        }

        /// <summary>
        ///     Applies the stat modifiers defined directly on this ability's ScriptableObject.
        /// </summary>
        /// <param name="stats">The KirbyStats object to modify.</param>
        public virtual void ApplyAbilityDefinedModifiers(KirbyStats stats)
        {
            foreach (StatModifier modifier in abilityDefinedModifiers) // Use renamed list
            {
                float currentValue = stats.GetStat(modifier.statType);
                float newValue = modifier.ApplyModifier(currentValue);
                stats.SetStat(modifier.statType, newValue);
            }
        }

        /// <summary>
        ///     Returns true if the ability modifies a specific stat (now checks abilityDefinedModifiers)
        /// </summary>
        public virtual bool ModifiesStat(StatType statType) =>
            abilityDefinedModifiers.Any(m => m.statType == statType); // Use renamed list

        /// <summary>
        ///     Get the modifier value for a specific stat
        /// </summary>
        public virtual float GetStatModifier(StatType statType) =>
            1.0f; // Default is no modification (multiplicative identity)
    }
}
