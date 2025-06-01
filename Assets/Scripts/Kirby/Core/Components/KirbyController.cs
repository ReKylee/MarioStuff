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
        private KirbyStats baseStats = new(); // Default stats

        [SerializeField] private CopyAbilityData currentCopyAbility;

        private readonly List<IAbilityModule> _activeAbilities = new();
        private readonly List<IMovementAbilityModule> _movementAbilities = new();
        private KirbyGroundCheck _groundCheck;
        internal SpriteAnimator Animator;
        internal Collider2D Collider;


        internal InputHandler InputHandler;
        internal Rigidbody2D Rigidbody;
        public KirbyStats Stats { get; private set; }
        public InputContext CurrentInput { get; private set; }

        public bool IsGrounded => _groundCheck?.IsGrounded ?? false;
        public float GroundSlopeAngle => _groundCheck?.GroundSlopeAngle ?? 0;
        public Vector2 GroundNormal => _groundCheck?.GroundNormal ?? Vector2.zero;

        private void Awake()
        {
            _groundCheck = GetComponent<KirbyGroundCheck>();
            Rigidbody = GetComponent<Rigidbody2D>();
            Animator = GetComponent<SpriteAnimator>();
            Collider = GetComponent<Collider2D>();
            Stats = baseStats.CreateCopy();

            InputHandler = GetComponent<InputHandler>();

            if (!InputHandler)
            {
                Debug.LogError(
                    "InputHandler not assigned and not found on the same GameObject. Please assign it in the KirbyController Inspector.");

                enabled = false;
                return;
            }

            EquipAbility(currentCopyAbility);
        }

        private void Update()
        {
            if (InputHandler == null) return;

            CurrentInput = InputHandler.PollInput();

            // Process all active abilities, passing the current input context
            foreach (IAbilityModule ability in _activeAbilities)
            {
                ability.ProcessAbility(CurrentInput);
            }

            // Reset transient inputs at the end of the frame
            InputContext tempInput = CurrentInput; // Structs are value types, this creates a copy
            tempInput.ResetTransientInputs();
            CurrentInput = tempInput; // Assign the modified copy back
        }

        private void FixedUpdate()
        {
            // Movement processing
            // Let movement abilities process/modify velocity
            // They are processed in the order they appear in the CopyAbilityData's Abilities list.
            Rigidbody.linearVelocity = _movementAbilities.Aggregate(
                Rigidbody.linearVelocity,
                (current, movementAbility) =>
                    movementAbility.ProcessMovement(current, IsGrounded, CurrentInput));
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
            Stats = baseStats.CreateCopy(); // Reset to base stats before applying new modifiers

            if (currentCopyAbility) // Check the newly assigned currentCopyAbility (which is newAbilityData)
            {
                // Apply modifiers from CopyAbilityData
                Stats = currentCopyAbility.ApplyModifiers(Stats); // Uses currentCopyAbility (newAbilityData)

                // Initialize and categorize abilities from CopyAbilityData
                foreach (AbilityModuleBase abilitySo in
                         currentCopyAbility.abilities) // Uses currentCopyAbility (newAbilityData)
                {
                    if (abilitySo is IAbilityModule abilityInstance)
                    {
                        abilityInstance.Initialize(this);

                        // Apply modifiers defined directly on the AbilityModuleBase ScriptableObject
                        // This applies to the Stats object that has already been modified by CopyAbilityData
                        abilitySo.ApplyAbilityDefinedModifiers(Stats);

                        _activeAbilities.Add(abilityInstance);
                        if (abilityInstance is IMovementAbilityModule movementAbility)
                        {
                            _movementAbilities.Add(movementAbility);
                        }

                        abilityInstance.OnActivate();
                    }
                }
            }
            // If no CopyAbilityData, Kirby operates with base stats and no special abilities.
            // You might want a "Default" or "Normal" CopyAbilityData for this case.
        }
    }
}
