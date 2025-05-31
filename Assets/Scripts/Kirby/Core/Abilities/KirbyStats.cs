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
        // Dictionary mapping stat types to field info - filled via reflection
        private static Dictionary<StatType, FieldInfo> statFields;

        // Dictionary mapping stat types to their categories
        private static Dictionary<StatType, string> statCategories;

        [Header("Movement Settings")] [KirbyStat(StatType.WalkSpeed, "Movement")]
        public float walkSpeed = 120f;

        [KirbyStat(StatType.RunSpeed, "Movement")]
        public float runSpeed = 200f;

        [KirbyStat(StatType.GroundAcceleration, "Movement")]
        public float groundAcceleration = 500f;

        [KirbyStat(StatType.GroundDeceleration, "Movement")]
        public float groundDeceleration = 800f;

        [KirbyStat(StatType.AirAcceleration, "Movement")]
        public float airAcceleration = 200f;

        [KirbyStat(StatType.AirDeceleration, "Movement")]
        public float airDeceleration; // No auto-deceleration in air

        [Header("Jump Settings")] [KirbyStat(StatType.JumpVelocity, "Jump")]
        public float jumpVelocity = 300f;

        [KirbyStat(StatType.JumpReleaseGravityMultiplier, "Jump")]
        public float jumpReleaseGravityMultiplier = 2f;

        [KirbyStat(StatType.MaxFallSpeed, "Jump")]
        public float maxFallSpeed = 500f;

        [KirbyStat(StatType.CoyoteTime, "Jump")]
        public float coyoteTime = 0.1f;

        [KirbyStat(StatType.JumpBufferTime, "Jump")]
        public float jumpBufferTime = 0.15f;

        [Header("Float Settings")] [KirbyStat(StatType.FloatAscendSpeed, "Float")]
        public float floatAscendSpeed = 70f;

        [KirbyStat(StatType.FloatDescentSpeed, "Float")]
        public float floatDescentSpeed = 40f;

        [KirbyStat(StatType.FlapImpulse, "Float")]
        public float flapImpulse = 85f;

        [KirbyStat(StatType.FlyMaxHeight, "Float")]
        public float flyMaxHeight = 1000f;

        [Header("Physics")] [KirbyStat(StatType.GravityScale, "Physics")]
        public float gravityScale = 2f;

        [KirbyStat(StatType.GravityScaleDescending, "Physics")]
        public float gravityScaleDescending = 1.5f;

        [KirbyStat(StatType.GroundCheckRadius, "Physics")]
        public float groundCheckRadius = 0.1f;

        [Header("Combat")] [KirbyStat(StatType.AttackDamage, "Combat")]
        public float attackDamage = 10f;

        [KirbyStat(StatType.AttackRange, "Combat")]
        public float attackRange = 1f;

        [KirbyStat(StatType.AttackSpeed, "Combat")]
        public float attackSpeed = 1f;

        [Header("Other")] [KirbyStat(StatType.InhaleRange, "Other")]
        public float inhaleRange = 2f;

        [KirbyStat(StatType.InhalePower, "Other")]
        public float inhalePower = 1f;

        // Initialize the reflection cache
        static KirbyStats()
        {
            statFields = new Dictionary<StatType, FieldInfo>();
            statCategories = new Dictionary<StatType, string>();

            // Get all fields with KirbyStatAttribute
            var fields = typeof(KirbyStats).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                KirbyStatAttribute attribute = field.GetCustomAttribute<KirbyStatAttribute>();
                if (attribute != null)
                {
                    statFields[attribute.Type] = field;
                    statCategories[attribute.Type] = attribute.Category;
                }
            }
        }

        /// <summary>
        ///     Get a stat value by its enum type
        /// </summary>
        public float GetStat(StatType statType)
        {
            if (statFields.TryGetValue(statType, out FieldInfo field))
            {
                return (float)field.GetValue(this);
            }

            Debug.LogWarning($"Stat {statType} not found!");
            return 1.0f; // Default multiplier
        }

        /// <summary>
        ///     Set a stat value by its enum type
        /// </summary>
        public void SetStat(StatType statType, float value)
        {
            if (statFields.TryGetValue(statType, out FieldInfo field))
            {
                field.SetValue(this, value);
            }
            else
            {
                Debug.LogWarning($"Trying to set unknown stat: {statType}");
            }
        }

        /// <summary>
        ///     Get the category for a stat type
        /// </summary>
        public static string GetStatCategory(StatType statType)
        {
            if (statCategories.TryGetValue(statType, out string category))
            {
                return category;
            }

            return "Other";
        }

        /// <summary>
        ///     Create a deep copy of these stats
        /// </summary>
        public KirbyStats CreateCopy()
        {
            KirbyStats copy = new();

            // Copy all stat fields using reflection
            foreach (var pair in statFields)
            {
                FieldInfo field = pair.Value;
                field.SetValue(copy, field.GetValue(this));
            }

            return copy;
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

        public float value = 1.0f;
        public ModType modificationType = ModType.Multiplicative;

        // For editor organization and display
        [HideInInspector] public string category;

        public StatType statType;

        // Constructor without category
        public StatModifier(StatType statType, float value, ModType modificationType = ModType.Multiplicative)
        {
            this.statType = statType;
            this.value = value;
            this.modificationType = modificationType;
            category = KirbyStats.GetStatCategory(statType);
        }

        // Apply this modifier to a stat value
        public float ApplyModifier(float baseValue)
        {
            switch (modificationType)
            {
                case ModType.Additive:
                    return baseValue + value;
                case ModType.Multiplicative:
                    return baseValue * value;
                case ModType.Override:
                    return value;
                default:
                    return baseValue;
            }
        }
    }
}
