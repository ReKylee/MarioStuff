using System.Collections.Generic;
using System.Linq;
using GabrielBigardi.SpriteAnimator;
using Kirby.Abilities;
using UnityEngine;

namespace Kirby.Core.Components
{
    public class KirbyController : MonoBehaviour
    {

        [Header("Core Stats & Abilities")] [SerializeField]
        private KirbyStats baseStats; // Changed: No longer new(), assign in Inspector

        [SerializeField] private CopyAbilityData currentCopyAbility;

        private readonly List<IAbilityModule> _activeAbilities = new();
        private readonly List<IMovementAbilityModule> _movementAbilities = new();

        private InputContext _currentInput;

        private KirbyGroundCheck _groundCheck;

        private InputHandler _inputHandler;
        internal SpriteAnimator Animator;
        internal Collider2D Collider;
        internal Rigidbody2D Rigidbody;
        public KirbyStats Stats { get; private set; } // Changed from Stats to Stats

        public bool IsGrounded => _groundCheck?.IsGrounded ?? false;
        public float GroundSlopeAngle => _groundCheck?.GroundSlopeAngle ?? 0;
        public Vector2 GroundNormal => _groundCheck?.GroundNormal ?? Vector2.zero;

        private void Awake()
        {
            _groundCheck = GetComponent<KirbyGroundCheck>();
            Rigidbody = GetComponent<Rigidbody2D>();

            Animator = GetComponent<SpriteAnimator>();
            Collider = GetComponent<Collider2D>();
            // Stats will be initialized by RefreshRuntimeStats, called from EquipAbility or Awake/Update.

            _inputHandler = GetComponent<InputHandler>();

            if (!_inputHandler)
            {
                Debug.LogError(
                    "InputHandler not assigned and not found on the same GameObject. Please assign it in the KirbyController Inspector.");

                enabled = false;
                return;
            }

            // Initial stat setup. EquipAbility will call RefreshRuntimeStats.
            // Ensure Stats is initialized before EquipAbility if baseStats is available.
            if (baseStats != null)
            {
                Stats = Instantiate(baseStats); // Initial copy
            }
            else
            {
                Debug.LogError(
                    "KirbyController: baseStats is not assigned in the Inspector. Creating a default KirbyStats instance.");

                Stats = ScriptableObject.CreateInstance<KirbyStats>(); // Fallback
            }

            EquipAbility(currentCopyAbility);
        }

        private void Update()
        {
            if (!_inputHandler) return;

            // Refresh runtime stats to reflect any changes to baseStats asset in inspector
            // and ensure ability modifiers are correctly applied.
            RefreshRuntimeStats();

            _currentInput = _inputHandler.CurrentInput();

            // Process all active abilities, passing the current input context
            foreach (IAbilityModule ability in _activeAbilities)
            {
                ability.ProcessAbility(_currentInput);
            }

        }

        private void FixedUpdate()
        {
            // Movement processing
            // Let movement abilities process/modify velocity
            // They are processed in the order they appear in the CopyAbilityData's Abilities list.
            Rigidbody.linearVelocity = _movementAbilities.Aggregate(
                Rigidbody.linearVelocity,
                (current, movementAbility) =>
                    movementAbility.ProcessMovement(current, IsGrounded, _currentInput));
        }

        public void EquipAbility(CopyAbilityData newAbilityData)
        {
            // Deactivate and clear previous abilities
            foreach (IAbilityModule ability in _activeAbilities)
            {
                ability.OnDeactivate();
            }

            _activeAbilities.Clear();
            _movementAbilities.Clear();

            currentCopyAbility = newAbilityData; // Assign the new ability data
            // Stats are refreshed by RefreshRuntimeStats below

            if (currentCopyAbility != null)
            {
                // Initialize and categorize abilities from CopyAbilityData
                foreach (AbilityModuleBase abilitySo in currentCopyAbility.abilities)
                {
                    if (abilitySo is IAbilityModule abilityInstance)
                    {
                        abilityInstance.Initialize(this);
                        _activeAbilities.Add(abilityInstance);
                        if (abilityInstance is IMovementAbilityModule movementAbility)
                        {
                            _movementAbilities.Add(movementAbility);
                        }

                        abilityInstance.OnActivate();
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"AbilityModule '{abilitySo.name}' does not implement IAbilityModule and won't be activated.",
                            this);
                    }
                }
            }

            RefreshRuntimeStats(); // This will calculate and apply all stats
        }

        /// <summary>
        ///     Refreshes Stats based on baseStats and any active copy ability.
        ///     This allows live updates if baseStats asset is changed in the inspector.
        /// </summary>
        private void RefreshRuntimeStats()
        {
            if (!baseStats)
            {
                if (!Stats) // Ensure Stats is not null
                {
                    Stats = ScriptableObject.CreateInstance<KirbyStats>();
                }

                // ApplyStatsToComponents might be needed here if there's a default state for components
                ApplyStatsToComponents();
                return;
            }

            if (currentCopyAbility)
            {
                // ApplyModifiers from CopyAbilityData should return a NEW INSTANCE based on baseStats
                Stats = currentCopyAbility.ApplyModifiers(baseStats);

                // Apply modifiers defined directly on the AbilityModuleBase ScriptableObjects
                // These are applied to the Stats instance that already has CopyAbilityData modifiers
                foreach (AbilityModuleBase abilitySo in currentCopyAbility.abilities)
                {
                    abilitySo.ApplyAbilityDefinedModifiers(Stats);
                }
            }
            else
            {
                // No ability, Stats is a direct instance/clone of baseStats
                Stats = Instantiate(baseStats);
            }

            // Apply stats that directly affect components
            ApplyStatsToComponents();
        }


        /// <summary>
        ///     Applies stats that directly affect components like Rigidbody
        /// </summary>
        private void ApplyStatsToComponents()
        {
            if (Stats is null) return; // Changed from Stats to Stats
            if (Rigidbody)
            {
                Rigidbody.gravityScale = Stats.gravityScale; // Changed from Stats to Stats
            }
        }
    }
}
