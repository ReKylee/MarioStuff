using InputSystem;
using UnityEngine;

namespace Kirby.Core
{
    public class InputHandler : MonoBehaviour
    {
        private InputContext _currentFrameInput;
        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
            _currentFrameInput = new InputContext(); // Initialize here
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        /// <summary>
        ///     Polls the current input state from the InputSystem and updates the internal context.
        ///     This should be called by KirbyController before GetCurrentInputContext().
        /// </summary>
        public InputContext PollInput()
        {
            if (_inputActions == null || !_inputActions.Player.enabled) return _currentFrameInput;

            _currentFrameInput.MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();

            _currentFrameInput.JumpPressed = _inputActions.Player.Jump.WasPressedThisFrame();
            _currentFrameInput.JumpReleased = _inputActions.Player.Jump.WasReleasedThisFrame();
            _currentFrameInput.JumpHeld = _inputActions.Player.Jump.IsPressed();

            _currentFrameInput.AttackPressed = _inputActions.Player.Attack.WasPressedThisFrame();
            _currentFrameInput.AttackReleased = _inputActions.Player.Attack.WasReleasedThisFrame();
            _currentFrameInput.AttackHeld = _inputActions.Player.Attack.IsPressed();
            return _currentFrameInput;

        }
    }
}
