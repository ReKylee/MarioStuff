using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Attribute to mark a stat field in KirbyStats and associate it with a StatType
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class KirbyStatAttribute : Attribute
    {
        public KirbyStatAttribute(StatType type, string category)
        {
            Type = type;
            Category = category;
        }
        public StatType Type { get; }
        public string Category { get; }
    }

    /// <summary>
    ///     Simple stat value container - used for original values and modified values
    /// </summary>
    [Serializable]
    public class KirbyStats
    {
        private static Dictionary<StatType, (FieldInfo field, string category)> _statInfoCache;

        [Header("Movement Settings")] [KirbyStat(StatType.WalkSpeed, "Movement")]
        public float walkSpeed = 4.5f;

        [KirbyStat(StatType.RunSpeed, "Movement")]
        public float runSpeed = 7.5f;

        [KirbyStat(StatType.GroundAcceleration, "Movement")]
        public float groundAcceleration = 40f;

        [KirbyStat(StatType.GroundDeceleration, "Movement")]
        public float groundDeceleration = 60f;

        [KirbyStat(StatType.AirAcceleration, "Movement")]
        public float airAcceleration = 20f;

        [KirbyStat(StatType.AirDeceleration, "Movement")]
        public float airDeceleration = 5f;

        [Header("Jump Settings")] [KirbyStat(StatType.JumpVelocity, "Jump")]
        public float jumpVelocity = 18f;

        [KirbyStat(StatType.JumpReleaseGravityMultiplier, "Jump")]
        public float jumpReleaseGravityMultiplier = 2.0f;

        [KirbyStat(StatType.MaxFallSpeed, "Jump")]
        public float maxFallSpeed = 15f;

        [KirbyStat(StatType.CoyoteTime, "Jump")]
        public float coyoteTime = 0.08f;

        [KirbyStat(StatType.JumpBufferTime, "Jump")]
        public float jumpBufferTime = 0.15f;

        [Header("Float Settings")] [KirbyStat(StatType.FloatAscendSpeed, "Float")]
        public float floatAscendSpeed = 1.5f;

        [KirbyStat(StatType.FloatDescentSpeed, "Float")]
        public float floatDescentSpeed = 1.0f;

        [KirbyStat(StatType.FlapImpulse, "Float")]
        public float flapImpulse = 4f;

        [KirbyStat(StatType.FlyMaxHeight, "Float")]
        public float flyMaxHeight = 10f;

        [Header("Physics")] [KirbyStat(StatType.GravityScale, "Physics")]
        public float gravityScale = 3.0f;

        [KirbyStat(StatType.GravityScaleDescending, "Physics")]
        public float gravityScaleDescending = 3.5f;

        [Header("Combat")] [KirbyStat(StatType.AttackDamage, "Combat")]
        public float attackDamage = 10f;

        [KirbyStat(StatType.AttackRange, "Combat")]
        public float attackRange = 0.5f;

        [KirbyStat(StatType.AttackSpeed, "Combat")]
        public float attackSpeed = 1.0f;

        [Header("Other")] [KirbyStat(StatType.InhaleRange, "Other")]
        public float inhaleRange = 2.5f;

        [KirbyStat(StatType.InhalePower, "Other")]
        public float inhalePower = 5f;

        // Initialize the reflection cache
        static KirbyStats()
        {
            _statInfoCache = new Dictionary<StatType, (FieldInfo field, string category)>();

            // Get all fields with KirbyStatAttribute
            var fields = typeof(KirbyStats).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                KirbyStatAttribute attribute = field.GetCustomAttribute<KirbyStatAttribute>();
                if (attribute != null)
                {
                    _statInfoCache[attribute.Type] = (field, attribute.Category);
                }
            }
        }

        /// <summary>
        ///     Get a stat value by its enum type
        /// </summary>
        public float GetStat(StatType statType) =>
            _statInfoCache.TryGetValue(statType, out (FieldInfo field, string category) statData)
                ? (float)statData.field.GetValue(this)
                : 1.0f;

        /// <summary>
        ///     Set a stat value by its enum type
        /// </summary>
        public void SetStat(StatType statType, float value)
        {
            if (_statInfoCache.TryGetValue(statType, out (FieldInfo field, string category) statData))
            {
                statData.field.SetValue(this, value);
            }
            else
            {
                Debug.LogWarning($"Trying to set unknown stat: {statType}");
            }
        }

        public static (FieldInfo field, string category) GetStatInfo(StatType statType) =>
            _statInfoCache.GetValueOrDefault(statType, (null, "Other"));

        public static string GetStatCategory(StatType statType) => GetStatInfo(statType).category;

        /// <summary>
        ///     Create a deep copy of these stats
        /// </summary>
        public KirbyStats CreateCopy()
        {
            KirbyStats copy = new();

            foreach (var pair in _statInfoCache)
            {
                FieldInfo field = pair.Value.field;
                field.SetValue(copy, field.GetValue(this));
            }

            return copy;
        }

        /// <summary>
        ///     Applies a single StatModifier directly to this KirbyStats instance.
        ///     This is more efficient as it looks up FieldInfo only once.
        /// </summary>
        /// <param name="modifier">The StatModifier to apply.</param>
        public void ApplySingleModifier(StatModifier modifier)
        {
            if (_statInfoCache.TryGetValue(modifier.statType, out (FieldInfo field, string category) statData))
            {
                if (statData.field is not null)
                {
                    float currentValue = (float)statData.field.GetValue(this);
                    float newValue = modifier.ApplyModifier(currentValue);
                    statData.field.SetValue(this, newValue);
                }
                else
                {
                    Debug.LogWarning(
                        $"FieldInfo is null for StatType: {modifier.statType} in _statInfoCache during ApplySingleModifier.");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Trying to apply modifier for unknown stat: {modifier.statType} in ApplySingleModifier.");
            }
        }
    }

    /// <summary>
    ///     Defines a modification to a stat - used in CopyAbilityData
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public enum ModType
        {
            Additive, // Add to the base value
            Multiplicative, // Multiply the base value
            Override // Completely override the value
        }

        public float value;
        public ModType modificationType;
        [HideInInspector] public string category;
        public StatType statType;
        public StatModifier(StatType statType, float value, ModType modificationType = ModType.Multiplicative)
        {
            this.statType = statType;
            this.value = value;
            this.modificationType = modificationType;
            category = KirbyStats.GetStatCategory(statType);
        }
        public float ApplyModifier(float baseValue)
        {
            return modificationType switch
            {
                ModType.Additive => baseValue + value,
                ModType.Multiplicative => baseValue * value,
                ModType.Override => value,
                _ => baseValue
            };
        }
    }
}
