namespace Animation.Flow.Core
{
    /// <summary>
    ///     Tracks input state for animation controllers
    /// </summary>
    public class InputContext
    {
        /// <summary>
        ///     True if the jump button is currently held down
        /// </summary>
        public bool JumpHeld { get; set; }

        /// <summary>
        ///     True if the jump button was just pressed this frame
        /// </summary>
        public bool JumpPressed { get; set; }

        /// <summary>
        ///     True if the jump button was just released this frame
        /// </summary>
        public bool JumpReleased { get; set; }

        /// <summary>
        ///     True if the attack button is currently held down
        /// </summary>
        public bool AttackHeld { get; set; }

        /// <summary>
        ///     True if the attack button was just pressed this frame
        /// </summary>
        public bool AttackPressed { get; set; }

        /// <summary>
        ///     True if the dash button is currently held down
        /// </summary>
        public bool DashHeld { get; set; }

        /// <summary>
        ///     True if the dash button was just pressed this frame
        /// </summary>
        public bool DashPressed { get; set; }

        /// <summary>
        ///     True if the run button is currently held down
        /// </summary>
        public bool RunInput { get; set; }

        /// <summary>
        ///     Horizontal input value (-1 to 1)
        /// </summary>
        public float HorizontalInput { get; set; }

        /// <summary>
        ///     Vertical input value (-1 to 1)
        /// </summary>
        public float VerticalInput { get; set; }

        /// <summary>
        ///     Resets all input states to their default values
        /// </summary>
        public void Reset()
        {
            JumpHeld = false;
            JumpPressed = false;
            JumpReleased = false;
            AttackHeld = false;
            AttackPressed = false;
            DashHeld = false;
            DashPressed = false;
            RunInput = false;
            HorizontalInput = 0f;
            VerticalInput = 0f;
        }
    }
}
