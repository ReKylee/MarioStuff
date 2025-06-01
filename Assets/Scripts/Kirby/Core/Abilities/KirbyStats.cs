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
    [CreateAssetMenu(fileName = "NewKirbyStats", menuName = "Kirby/Kirby Stats")]
    public class KirbyStats : ScriptableObject
    {
        private static readonly Dictionary<StatType, (FieldInfo field, string category)> _statInfoCache;

        [Header("Movement Settings")] [KirbyStat(StatType.WalkSpeed, "Movement")]
        public float walkSpeed = 4.0f;

        [KirbyStat(StatType.RunSpeed, "Movement")]
        public float runSpeed = 6f;

        [KirbyStat(StatType.GroundAcceleration, "Movement")]
        public float groundAcceleration = 50f;

        [KirbyStat(StatType.GroundDeceleration, "Movement")]
        public float groundDeceleration = 70f;

        [KirbyStat(StatType.AirAcceleration, "Movement")]
        public float airAcceleration = 25f;

        [KirbyStat(StatType.AirDeceleration, "Movement")]
        public float airDeceleration = 10f;

        [Header("Jump Settings")] [KirbyStat(StatType.JumpVelocity, "Jump")]
        public float jumpVelocity = 14f;

        [KirbyStat(StatType.JumpReleaseVelocityMultiplier, "Jump")]
        public float jumpReleaseVelocityMultiplier = 0.5f;

        [KirbyStat(StatType.MaxFallSpeed, "Jump")]
        public float maxFallSpeed = 15f;

        [KirbyStat(StatType.CoyoteTime, "Jump")]
        public float coyoteTime = 0.08f;

        [KirbyStat(StatType.JumpBufferTime, "Jump")]
        public float jumpBufferTime = 0.1f;

        [Header("Float Settings")] [KirbyStat(StatType.FloatAscendSpeed, "Float")]
        public float floatAscendSpeed = 2.0f;

        [KirbyStat(StatType.FloatDescentSpeed, "Float")]
        public float floatDescentSpeed = 1.0f;

        [KirbyStat(StatType.FlapImpulse, "Float")]
        public float flapImpulse = 5.5f;

        [KirbyStat(StatType.FlyMaxHeight, "Float")]
        public float flyMaxHeight = 10f;

        [Header("Physics")] [KirbyStat(StatType.GravityScale, "Physics")]
        public float gravityScale = 2.8f;

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
                : 1.0f; // ReSharper disable Unity.PerformanceAnalysis
        // ReSharper disable Unity.PerformanceAnalysis
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
        ///     Applies a single StatModifier directly to this KirbyStats instance.
        ///     This is more efficient as it looks up FieldInfo only once.
        /// </summary>
        /// <param name="modifier">The StatModifier to apply.</param>
        public void ApplySingleModifier(StatModifier modifier)
        {
            if (_statInfoCache.TryGetValue(modifier.StatType, out (FieldInfo field, string category) statData))
            {
                float currentValue = (float)statData.field.GetValue(this);
                statData.field.SetValue(this,
                    StatModifier.ApplyModifier(currentValue, modifier.Value, modifier.ModificationType));
            }
        }
    }
}
