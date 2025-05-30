namespace Kirby.Interfaces
{
    /// <summary>
    ///     Interface for handling Kirby's animation
    /// </summary>
    public interface IAnimationHandler
    {
        /// <summary>
        ///     Plays an animation with the given name
        /// </summary>
        /// <param name="animationName">Name of the animation to play</param>
        void PlayAnimation(string animationName);

        /// <summary>
        ///     Determines the appropriate animation for the current terrain angle, full status, and crouch status.
        /// </summary>
        /// <param name="baseAnimationName">Base animation name (e.g., "Idle")</param>
        /// <param name="terrainAngle">Angle of the terrain Kirby is standing on</param>
        /// <param name="isFull">Whether Kirby has something in his mouth</param>
        /// <param name="isCrouching">Whether Kirby is crouching</param>
        /// <returns>The adjusted animation name based on terrain and other states</returns>
        string GetTerrainAdjustedAnimation(string baseAnimationName, float terrainAngle, bool isFull, bool isCrouching);

        /// <summary>
        ///     Checks if a specific animation is currently playing
        /// </summary>
        /// <param name="animationName">Name of the animation to check</param>
        /// <returns>True if the animation is playing, false otherwise</returns>
        bool IsPlayingAnimation(string animationName);

        /// <summary>
        ///     Sets whether Kirby has something in his mouth.
        ///     This status is used by GetTerrainAdjustedAnimation.
        /// </summary>
        /// <param name="isFull">True if Kirby's mouth is full, false otherwise.</param>
        void SetFullStatus(bool isFull);

        /// <summary>
        ///     Sets whether Kirby is crouching.
        ///     This status is used by GetTerrainAdjustedAnimation.
        /// </summary>
        /// <param name="isCrouching">True if Kirby is crouching, false otherwise.</param>
        void SetCrouchStatus(bool isCrouching);

        /// <summary>
        ///     Sets the animation set to use (used when changing forms)
        /// </summary>
        void SetAnimationSet(AnimationSet newAnimationSet);
    }
}
