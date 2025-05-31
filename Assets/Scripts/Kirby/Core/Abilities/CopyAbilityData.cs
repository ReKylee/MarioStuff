using System.Collections.Generic;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Data for a Kirby copy ability
    /// </summary>
    [CreateAssetMenu(fileName = "NewCopyAbility", menuName = "Kirby/Copy Ability")]
    public class CopyAbilityData : ScriptableObject
    {

        public enum AbilityType
        {
            Melee,
            Projectile,
            Transformation,
            Utility,
            Special
        }

        [Header("Contained Abilities")] public List<AbilityModuleBase> Abilities = new();

        [Header("Basic Info")] public string abilityName = "New Ability";

        public Sprite abilityIcon;

        [Header("Ability Capabilities")] [Header("Stat Modifiers")] [SerializeField]
        private List<StatModifier> statModifiers = new();

        [Header("Ability Specifics")] public AbilityType abilityType = AbilityType.Melee;

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
                float currentValue = modifiedStats.GetStat(modifier.statType);
                float newValue = modifier.ApplyModifier(currentValue);
                modifiedStats.SetStat(modifier.statType, newValue);
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
