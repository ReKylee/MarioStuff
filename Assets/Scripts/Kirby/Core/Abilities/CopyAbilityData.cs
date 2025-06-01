using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Data for a Kirby copy ability
    /// </summary>
    [CreateAssetMenu(fileName = "NewCopyAbility", menuName = "Kirby/Copy Ability")]
    public class CopyAbilityData : ScriptableObject
    {
        // Enum to represent the result of attempting to add an ability
        public enum AbilityAddResult
        {
            Success,
            DuplicateNotAllowed,
            InvalidAbility
        }

        public enum AbilityType
        {
            Melee,
            Projectile,
            Transformation,
            Utility,
            Special
        }

        [Header("Contained Abilities")] public List<AbilityModuleBase> abilities = new();

        [Header("Basic Info")] public string abilityName = "New Ability";

        public Sprite abilityIcon;

        [Header("Ability Capabilities")] [Header("Stat Modifiers")] [SerializeField]
        private List<StatModifier> statModifiers = new();

        [Header("Ability Specifics")] public AbilityType abilityType = AbilityType.Melee;

        /// <summary>
        ///     Checks if an ability module of the given type can be added
        /// </summary>
        /// <param name="abilityType">The type of ability module to check</param>
        /// <returns>Result indicating if addition is allowed and why if not</returns>
        public AbilityAddResult CanAddAbilityModule(Type abilityType)
        {
            // Quick validation check
            if (abilityType == null || !typeof(AbilityModuleBase).IsAssignableFrom(abilityType))
            {
                return AbilityAddResult.InvalidAbility;
            }

            // Use LINQ for a more concise check
            return abilities.Any(module =>
                module?.GetType() == abilityType &&
                !module.AllowMultipleInstances)
                ? AbilityAddResult.DuplicateNotAllowed
                : AbilityAddResult.Success;
        }

        /// <summary>
        ///     Returns the existing module that prevents adding a new one of the given type
        /// </summary>
        /// <param name="abilityType">The type of ability module to check</param>
        /// <returns>The existing module that prevents adding a new one, or null if no conflict</returns>
        public AbilityModuleBase GetConflictingModule(Type abilityType)
        {
            if (abilityType == null || !typeof(AbilityModuleBase).IsAssignableFrom(abilityType))
            {
                return null;
            }

            return abilities.FirstOrDefault(module =>
                module?.GetType() == abilityType &&
                !module.AllowMultipleInstances);
        }

        /// <summary>
        ///     Attempts to add a new ability module instance to this ability
        /// </summary>
        /// <param name="module">The ability module to add</param>
        /// <returns>Result of the add attempt</returns>
        public AbilityAddResult AddAbilityModule(AbilityModuleBase module)
        {
            if (module == null)
            {
                return AbilityAddResult.InvalidAbility;
            }

            // Check if an ability of this type already exists
            AbilityAddResult result = CanAddAbilityModule(module.GetType());
            if (result != AbilityAddResult.Success)
            {
                return result;
            }

            // Add the module
            abilities.Add(module);
            return AbilityAddResult.Success;
        }

        /// <summary>
        ///     Apply all modifiers to the provided stats
        /// </summary>
        public KirbyStats ApplyModifiers(KirbyStats baseStats)
        {
            // Create a copy of the base stats
            KirbyStats modifiedStats = baseStats.CreateCopy();

            // Apply all stat modifiers from this CopyAbilityData
            foreach (StatModifier modifier in statModifiers)
            {
                modifiedStats.ApplySingleModifier(modifier);
            }

            return modifiedStats;
        }

        /// <summary>
        ///     Get all stat modifiers
        /// </summary>
        public List<StatModifier> GetAllModifiers() => new(statModifiers);

        /// <summary>
        ///     Add a new stat modifier
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            // Check if we already have a modifier for this stat type
            int existingIndex = statModifiers.FindIndex(m => m.statType == modifier.statType);
            if (existingIndex >= 0)
            {
                // Replace the existing modifier
                statModifiers[existingIndex] = modifier;
            }
            else
            {
                // Add new modifier
                statModifiers.Add(modifier);
            }
        }

        /// <summary>
        ///     Remove a stat modifier by stat type
        /// </summary>
        public void RemoveModifier(StatType statType)
        {
            statModifiers.RemoveAll(m => m.statType == statType);
        }

        /// <summary>
        ///     Get modifiers for a specific category
        /// </summary>
        public List<StatModifier> GetModifiersByCategory(string category)
        {
            return statModifiers.FindAll(m => m.category == category);
        }
    }

}
