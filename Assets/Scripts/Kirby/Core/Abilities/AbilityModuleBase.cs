using System.Collections.Generic;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Base class for all ability ScriptableObjects
    /// </summary>
    public abstract class AbilityModuleBase : ScriptableObject, IAbilityModule
    {

        [SerializeField] private List<StatModifier> abilityDefinedModifiers = new();

        [Header("Module Settings")]
        [Tooltip("If false, CopyAbilityData cannot contain multiple instances of this module type")]
        [SerializeField]
        private bool allowMultipleInstances;

        [Header("Basic Information")] [SerializeField]
        private string abilityID = "ability_id";

        [SerializeField] private string displayName = "Ability Name";

        // Reference to the controller using this ability
        protected KirbyController Controller { get; private set; }
        protected Rigidbody2D Rigidbody { get; private set; }

        /// <summary>
        ///     Gets or sets whether multiple instances of this module type can be added to a CopyAbilityData
        /// </summary>
        public bool AllowMultipleInstances
        {
            get => allowMultipleInstances;
            set => allowMultipleInstances = value;
        }

        /// <summary>
        ///     Gets or sets the ability's unique identifier
        /// </summary>
        public string AbilityID
        {
            get => abilityID;
            set => abilityID = value;
        }

        /// <summary>
        ///     Gets or sets the ability's display name
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

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
