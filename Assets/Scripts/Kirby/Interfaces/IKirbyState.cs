namespace Kirby.Interfaces
{
    /// <summary>
    ///     Interface for all Kirby states
    /// </summary>
    public interface IKirbyState
    {
        /// <summary>
        ///     Called when entering this state
        /// </summary>
        void EnterState();

        /// <summary>
        ///     Called when exiting this state
        /// </summary>
        void ExitState();

        /// <summary>
        ///     Called during FixedUpdate when this state is active
        /// </summary>
        void PhysicsUpdate();

        /// <summary>
        ///     Called during Update when this state is active
        /// </summary>
        void LogicUpdate();

        /// <summary>
        ///     Checks state transitions and returns the next state if applicable
        /// </summary>
        /// <returns>The next state to transition to, or null if no transition should occur</returns>
        IKirbyState CheckTransitions();
    }
}
