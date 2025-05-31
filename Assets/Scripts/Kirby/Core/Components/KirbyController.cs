using System.Collections.Generic;
using System.Linq;
using GabrielBigardi.SpriteAnimator;
using Kirby.Abilities;
using UnityEngine;

namespace Kirby
{
    public class KirbyController : MonoBehaviour
    {

        [Header("Core Stats & Abilities")] [SerializeField]
        private KirbyStats baseStats = new(); // Default stats

        [SerializeField] private CopyAbilityData currentCopyAbility;

        private readonly List<IAbilityModule> _activeAbilities = new();
        private readonly List<IMovementAbilityModule> _movementAbilities = new();
        private bool _jumpInputPressed;

        private bool _jumpInputReleased;

        // Input fields (can be expanded or moved to an InputHandler class)
        private Vector2 _moveInput;
        internal SpriteAnimator Animator;

        internal KirbyGroundCheck GroundCheck;
        internal Rigidbody2D Rigidbody;
        public KirbyStats Stats { get; private set; }


        public bool IsGrounded => GroundCheck?.IsGrounded ?? false;
        public float GroundSlopeAngle => GroundCheck?.GroundSlopeAngle ?? 0;
        public Vector2 GroundNormal => GroundCheck?.GroundNormal ?? Vector2.zero;

        private void Awake()
        {
            GroundCheck = GetComponent<KirbyGroundCheck>();
            Rigidbody = GetComponent<Rigidbody2D>();
            Animator = GetComponent<SpriteAnimator>();
            Stats = baseStats.CreateCopy();
            EquipAbility(currentCopyAbility);
        }

        private void Update()
        {
            // Gather Input (example)
            // _moveInput.x = Input.GetAxis("Horizontal"); 
            // if (Input.GetButtonDown("Jump")) _jumpInputPressed = true;
            // if (Input.GetButtonUp("Jump")) _jumpInputReleased = true;

            // Process all active abilities
            foreach (IAbilityModule ability in _activeAbilities)
            {
                ability.ProcessAbility();
            }

            // Handle jump input specifically for abilities that might consume it (like JumpAbility)
            if (_jumpInputPressed)
            {
                foreach (JumpAbilityModule ability in _activeAbilities.OfType<JumpAbilityModule>())
                {
                    ability.OnJumpPressed();
                }

                _jumpInputPressed = false; // Consume press
            }

            if (_jumpInputReleased)
            {
                foreach (JumpAbilityModule ability in _activeAbilities.OfType<JumpAbilityModule>())
                {
                    ability.OnJumpReleased();
                }

                _jumpInputReleased = false; // Consume release
            }
        }

        private void FixedUpdate()
        {
            // Movement processing

            Vector2 currentVelocity = Rigidbody.linearVelocity;
            Vector2 targetVelocity = _moveInput * Stats.walkSpeed; // Base target, abilities can modify

            // Let movement abilities process/modify velocity
            // They are processed in the order they appear in the CopyAbilityData's Abilities list.
            foreach (IMovementAbilityModule movementAbility in _movementAbilities)
            {
                currentVelocity = movementAbility.ProcessMovement(currentVelocity, targetVelocity, IsGrounded);
            }

            Rigidbody.linearVelocity = currentVelocity;
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

            currentCopyAbility = newAbilityData;
            Stats = baseStats.CreateCopy(); // Reset to base stats before applying new modifiers

            if (currentCopyAbility)
            {
                // Apply modifiers from CopyAbilityData
                Stats = currentCopyAbility.ApplyModifiers(Stats);

                // Initialize and categorize abilities from CopyAbilityData
                foreach (AbilityModuleBase abilitySo in currentCopyAbility.Abilities)
                {
                    if (abilitySo is IAbilityModule abilityInstance)
                    {
                        abilityInstance.Initialize(this);
                        _activeAbilities.Add(abilityInstance);
                        if (abilityInstance is IMovementAbilityModule movementAbility)
                        {
                            _movementAbilities.Add(movementAbility);
                            // Allow movement abilities to make final adjustments to stats
                            movementAbility.FinalizeStats(Stats);
                        }

                        abilityInstance.OnActivate();
                    }
                }
            }
            // If no CopyAbilityData, Kirby operates with base stats and no special abilities.
            // You might want a "Default" or "Normal" CopyAbilityData for this case.
        }

        // Example Input Methods (to be called by an Input Handler)
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        public void TriggerJumpPressed()
        {
            _jumpInputPressed = true;
        }

        public void TriggerJumpReleased()
        {
            _jumpInputReleased = true;
        }
    }
}
