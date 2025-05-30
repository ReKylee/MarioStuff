using Kirby;
using Kirby.Interfaces;
using Kirby.States;
// Added for CrouchState reference

// Added for Debug.LogWarning if needed

public class KirbyStateBase : IKirbyState
{
    protected KirbyController kirbyController;

    protected KirbyStateBase(KirbyController controller)
    {
        kirbyController = controller;
    }

    /// <summary>
    ///     Called when entering this state
    /// </summary>
    public virtual void EnterState()
    {
        // Default implementation does nothing
    }

    /// <summary>
    ///     Called when exiting this state
    /// </summary>
    public virtual void ExitState()
    {
        // Default implementation does nothing
    }

    /// <summary>
    ///     Called during FixedUpdate when this state is active
    /// </summary>
    public virtual void PhysicsUpdate()
    {
        // Default implementation does nothing
    }

    /// <summary>
    ///     Called during Update when this state is active
    /// </summary>
    public virtual void LogicUpdate()
    {
        // Default implementation does nothing
    }

    /// <summary>
    ///     Checks state transitions and returns the next state if applicable
    /// </summary>
    public virtual IKirbyState CheckTransitions() => null;

    /// <summary>
    ///     Plays the appropriate animation for the current state
    /// </summary>
    /// <param name="stateName">The base name of the animation state</param>
    /// <param name="isFull">Whether Kirby has a full mouth</param>
    protected void PlayStateAnimation(string stateName, bool isFull)
    {
        // Get terrain-adjusted animation name from the animation handler
        float terrainAngle = kirbyController.MovementController.TerrainAngle;
        // Correctly determine if the current state IS a CrouchState
        bool isCrouching = this is CrouchState;
        string animationName =
            kirbyController.AnimationHandler.GetTerrainAdjustedAnimation(stateName, terrainAngle, isFull, isCrouching);

        // Play the animation
        kirbyController.AnimationHandler.PlayAnimation(animationName);
    }
}
