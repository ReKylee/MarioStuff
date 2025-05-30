using UnityEngine;

namespace Kirby.Interfaces
{
    /// <summary>
    /// Interface for handling Kirby's movement physics
    /// </summary>
    public interface IMovementController
    {
        /// <summary>
        /// Moves Kirby horizontally
        /// </summary>
        /// <param name="horizontalInput">Horizontal input value from -1 to 1</param>
        /// <param name="isRunning">Whether Kirby is running (affects speed)</param>
        void MoveHorizontal(float horizontalInput, bool isRunning);
        
        /// <summary>
        /// Makes Kirby jump with variable height based on how long the jump button is held
        /// </summary>
        /// <param name="jumpHeld">Whether the jump button is being held</param>
        void Jump(bool jumpHeld);
        
        /// <summary>
        /// Makes Kirby flap while flying to gain height
        /// </summary>
        void FlyFlap();
        
        /// <summary>
        /// Applies gentle descent physics for flying state
        /// </summary>
        void FlyGentleDescent();
        
        /// <summary>
        /// Applies standard falling physics
        /// </summary>
        void ApplyFallingPhysics();
        
        /// <summary>
        /// Gets whether Kirby is grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Gets the angle of the terrain Kirby is standing on
        /// </summary>
        float TerrainAngle { get; }
        
        /// <summary>
        /// Gets whether Kirby is on a slope
        /// </summary>
        bool IsOnSlope { get; }
        
        /// <summary>
        /// Gets whether Kirby is on a deep slope
        /// </summary>
        bool IsOnDeepSlope { get; }
        
        /// <summary>
        /// Gets whether Kirby is moving left
        /// </summary>
        bool IsFacingLeft { get; }
        
        /// <summary>
        /// Reset all velocity
        /// </summary>
        void ResetVelocity();
        
        /// <summary>
        /// Gets Kirby's current velocity
        /// </summary>
        Vector2 CurrentVelocity { get; }
    }
}
