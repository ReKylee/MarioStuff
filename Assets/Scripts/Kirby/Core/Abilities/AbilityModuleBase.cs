using System.Collections.Generic;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Base class for all ability ScriptableObjects
    /// </summary>
    public abstract class AbilityModuleBase : ScriptableObject, IAbilityModule
    {

        [SerializeField] private List<StatModifier> abilityDefinedModifiers = new();

        // Reference to the controller using this ability
        protected KirbyController Controller { get; private set; }
        protected Rigidbody2D Rigidbody { get; private set; }

        // Properties
        [field: Header("Basic Information")] public string AbilityID { get; } = "ability_id"; // Initialized

        public string DisplayName { get; } = "Ability Name"; // Initialized

        /// <summary>
        ///     Initialize the ability with controller reference
        /// </summary>
        public virtual void Initialize(KirbyController controller)
        {
            Controller = controller;
            Rigidbody = controller.Rigidbody;
        }

        /// <summary>
        ///     Called when the ability becomes active
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        ///     Called when the ability becomes inactive
        /// </summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>
        ///     Update logic for the ability
        /// </summary>
        public virtual void ProcessAbility(InputContext inputContext)
        {
        }

        /// <summary>
        ///     Applies the stat modifiers defined directly on this ability's ScriptableObject.
        /// </summary>
        /// <param name="stats">The KirbyStats object to modify.</param>
        public void ApplyAbilityDefinedModifiers(KirbyStats stats)
        {
            foreach (StatModifier modifier in abilityDefinedModifiers)
            {
                stats.ApplySingleModifier(modifier);
            }
        }
    }
}
