using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Implementation of IInputHandler for Kirby
    /// </summary>
    public class KirbyInputHandler : IInputHandler
    {

        // Input state

        // Input System reference
        private readonly InputSystem_Actions inputActions;
        private bool wasExhalePressed;
        private bool wasInhalePressed;

        // State tracking for one-frame checks
        private bool wasJumpPressed;

        public KirbyInputHandler()
        {
            // Create input actions
            inputActions = new InputSystem_Actions();

            // Enable input actions
            inputActions.Player.Enable();
        }

        // Public properties
        public float HorizontalInput { get; private set; }

        public float VerticalInput { get; private set; }

        public bool JumpPressed { get; private set; }

        public bool JumpHeld { get; private set; }

        public bool JumpReleased { get; private set; }

        public bool AttackPressed { get; private set; }

        public bool AttackHeld { get; private set; }

        public bool AttackReleased { get; private set; }

        public bool CrouchHeld { get; private set; }

        public bool ExhalePressed { get; private set; }

        /// <summary>
        ///     Updates input states - should be called once per frame
        /// </summary>
        public void UpdateInputs()
        {
            // Read move input
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            HorizontalInput = moveInput.x;
            VerticalInput = moveInput.y;

            // Track jump input
            bool isJumpPressed = inputActions.Player.Jump.IsPressed();
            JumpPressed = isJumpPressed && !wasJumpPressed;
            JumpReleased = !isJumpPressed && wasJumpPressed;
            JumpHeld = isJumpPressed;
            wasJumpPressed = isJumpPressed;

            // Track inhale input
            bool isInhalePressed = inputActions.Player.Attack.IsPressed();
            AttackPressed = isInhalePressed && !wasInhalePressed;
            AttackReleased = !isInhalePressed && wasInhalePressed;
            AttackHeld = isInhalePressed;
            wasInhalePressed = isInhalePressed;

            // Track exhale input
            bool isExhalePressed = inputActions.Player.Attack.IsPressed();
            ExhalePressed = isExhalePressed && !wasExhalePressed;
            wasExhalePressed = isExhalePressed;

            // Crouch is handled by checking if vertical input is down
            CrouchHeld = VerticalInput < -0.5f;
        }
    }
}
