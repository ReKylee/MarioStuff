using UnityEngine;
using UnityEngine.InputSystem;

namespace Movement
{
    /// <summary>
    ///     Context for handling input in movement abilities
    /// </summary>
    public class InputContext
    {
        /// <summary>
        ///     Raw movement input (usually from input system)
        /// </summary>
        public Vector2 MoveInput { get; set; }

        /// <summary>
        ///     Whether the jump button is pressed
        /// </summary>
        public bool JumpPressed { get; set; }

        /// <summary>
        ///     Whether the jump button is held
        /// </summary>
        public bool JumpHeld { get; set; }

        /// <summary>
        ///     Whether the inhale/ability button is pressed
        /// </summary>
        public bool AbilityPressed { get; set; }

        /// <summary>
        ///     Delta time for this frame
        /// </summary>
        public float DeltaTime { get; set; }

        /// <summary>
        ///     Type of input event
        /// </summary>
        public InputEventType EventType { get; set; }

        /// <summary>
        ///     Original input action callback context if available
        /// </summary>
        public InputAction.CallbackContext? OriginalContext { get; set; }
    }

    /// <summary>
    ///     Type of input event
    /// </summary>
    public enum InputEventType
    {
        /// <summary>
        ///     Button was just pressed this frame
        /// </summary>
        Pressed,

        /// <summary>
        ///     Button is being held
        /// </summary>
        Held,

        /// <summary>
        ///     Button was just released this frame
        /// </summary>
        Released,

        /// <summary>
        ///     Input value changed
        /// </summary>
        ValueChanged,

        /// <summary>
        ///     Regular update tick
        /// </summary>
        Update
    }
}
