using System.Collections;
using UnityEngine;

namespace Player.Input
{
    /// <summary>
    ///     Handles player input collection using Unity's new Input System
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        // Reference to the generated input actions class
        private InputSystem_Actions _inputActions;

        // Current input state
        public Vector2 MovementInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool InhalePressed { get; private set; }

        private void Awake()
        {
            // Create instance of the generated class
            _inputActions = new InputSystem_Actions();

            // Set up callbacks
            _inputActions.Player.Jump.started += ctx => OnJumpStarted();
            _inputActions.Player.Jump.canceled += ctx => OnJumpCanceled();
            _inputActions.Player.Inhale.performed += ctx => OnInhaleStarted();
        }

        private void Update()
        {
            // Update movement input
            MovementInput = _inputActions.Player.Move.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            // Enable the action map
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            // Disable the action map
            _inputActions.Player.Disable();
        }

        private void OnJumpStarted()
        {
            JumpPressed = true;
            JumpHeld = true;
        }

        private void OnJumpCanceled()
        {
            JumpPressed = false;
            JumpHeld = false;
        }

        private void OnInhaleStarted()
        {
            InhalePressed = true;

            // Reset in the next frame
            StartCoroutine(ResetButtonPress());
        }

        private IEnumerator ResetButtonPress()
        {
            yield return null;
            InhalePressed = false;
        }
    }
}
