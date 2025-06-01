using InputSystem;
using UnityEngine;

namespace Kirby.Core
{
    /// <summary>
    ///     Fully event-based input handler for Kirby's controls
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        private InputContext _currentInput;
        private InputSystem_Actions _inputActions;

        /// <summary>
        ///     Gets the current input state without polling
        ///     Only for special cases - prefer subscribing to OnInputUpdated event
        /// </summary>
        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
            _currentInput = new InputContext();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        #region DEBUG

#if UNITY_EDITOR
        private void OnGUI()
        {
            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.fontSize = 30; // Make the font even bigger
            labelStyle.normal.background = Texture2D.grayTexture; // Add a slight background
            labelStyle.alignment = TextAnchor.MiddleCenter;

            const float offsetX = 20;
            const float labelWidth = 500;
            const float labelHeight = 50;

            float screenHeight = Screen.height;
            const float totalHeight = 6 * labelHeight + 5 * 20;
            float startY = (screenHeight - totalHeight) / 2;

            GUI.Label(new Rect(offsetX, startY, labelWidth, labelHeight), $"Move Input: {_currentInput.MoveInput}",
                labelStyle);

            GUI.Label(new Rect(offsetX, startY + labelHeight + 20, labelWidth, labelHeight),
                $"Jump Pressed: {_currentInput.JumpPressed}",
                labelStyle);

            GUI.Label(new Rect(offsetX, startY + 2 * (labelHeight + 20), labelWidth, labelHeight),
                $"Jump Released: {_currentInput.JumpReleased}",
                labelStyle);

            GUI.Label(new Rect(offsetX, startY + 3 * (labelHeight + 20), labelWidth, labelHeight),
                $"Jump Held: {_currentInput.JumpHeld}",
                labelStyle);

            GUI.Label(new Rect(offsetX, startY + 4 * (labelHeight + 20), labelWidth, labelHeight),
                $"Attack Pressed: {_currentInput.AttackPressed}",
                labelStyle);

            GUI.Label(new Rect(offsetX, startY + 5 * (labelHeight + 20), labelWidth, labelHeight),
                $"Attack Released: {_currentInput.AttackReleased}", labelStyle);

            GUI.Label(new Rect(offsetX, startY + 6 * (labelHeight + 20), labelWidth, labelHeight),
                $"Attack Held: {_currentInput.AttackHeld}",
                labelStyle);
        }


#endif

        #endregion

        public InputContext CurrentInput()
        {
            _currentInput.MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();

            _currentInput.JumpPressed = _inputActions.Player.Jump.WasPerformedThisFrame();
            _currentInput.JumpReleased = _inputActions.Player.Jump.WasCompletedThisFrame();
            _currentInput.JumpHeld = _inputActions.Player.Jump.IsPressed();

            _currentInput.AttackPressed = _inputActions.Player.Attack.WasPerformedThisFrame();
            _currentInput.AttackReleased = _inputActions.Player.Attack.WasCompletedThisFrame();
            _currentInput.AttackHeld = _inputActions.Player.Attack.IsPressed();

            return _currentInput;
        }
    }
}
