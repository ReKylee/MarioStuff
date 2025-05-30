namespace Kirby.Interfaces
{
    /// <summary>
    ///     Interface for input handling to abstract the input system
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        ///     Horizontal movement input (-1 to 1)
        /// </summary>
        float HorizontalInput { get; }

        /// <summary>
        ///     Vertical movement input (-1 to 1)
        /// </summary>
        float VerticalInput { get; }

        /// <summary>
        ///     Jump button was pressed this frame
        /// </summary>
        bool JumpPressed { get; }

        /// <summary>
        ///     Jump button is being held
        /// </summary>
        bool JumpHeld { get; }

        /// <summary>
        ///     Jump button was released this frame
        /// </summary>
        bool JumpReleased { get; }

        /// <summary>
        ///     Inhale/action button was pressed this frame
        /// </summary>
        bool AttackPressed { get; }

        /// <summary>
        ///     Inhale/action button is being held
        /// </summary>
        bool AttackHeld { get; }

        /// <summary>
        ///     Inhale/action button was released this frame
        /// </summary>
        bool AttackReleased { get; }

        /// <summary>
        ///     Crouch/down button is being held
        /// </summary>
        bool CrouchHeld { get; }

        /// <summary>
        ///     Exhale/Spit button was pressed this frame
        /// </summary>
        bool ExhalePressed { get; }

        /// <summary>
        ///     Updates input states - should be called once per frame
        /// </summary>
        void UpdateInputs();
    }
}
