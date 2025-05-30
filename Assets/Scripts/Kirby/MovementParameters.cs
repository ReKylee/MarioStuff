using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Scriptable Object for configuring Kirby's movement parameters
    /// </summary>
    [CreateAssetMenu(fileName = "NewMovementParameters", menuName = "Kirby/Movement Parameters")]
    public class MovementParameters : ScriptableObject
    {
        [Header("Ground Movement")] [Tooltip("Maximum walking speed")]
        public float maxWalkSpeed = 5f;

        [Tooltip("Maximum running speed")] public float maxRunSpeed = 8f;

        [Tooltip("How quickly Kirby accelerates on the ground")]
        public float groundAcceleration = 50f;

        [Tooltip("How quickly Kirby decelerates on the ground")]
        public float groundDeceleration = 60f;

        [Header("Jumping")] [Tooltip("Initial jump force")]
        public float jumpForce = 12f;

        [Tooltip("Extra force applied when holding jump")]
        public float jumpHoldForce = 5f;

        [Tooltip("Maximum time jump hold force is applied")]
        public float maxJumpHoldTime = 0.25f;

        [Header("Flying")] [Tooltip("Upward force applied when flapping")]
        public float flapForce = 10f;

        [Tooltip("Constant downward force during gentle descent")]
        public float gentleDescentForce = 2f;

        [Tooltip("Maximum horizontal speed while flying")]
        public float maxFlyingHorizontalSpeed = 7f;

        [Tooltip("Horizontal acceleration while flying")]
        public float flyingHorizontalAcceleration = 30f;

        [Tooltip("Cooldown between flaps")] public float flapCooldown = 0.2f;

        [Tooltip("Maximum height Kirby can fly")]
        public float maxFlyHeight = 100f;

        [Header("Falling")] [Tooltip("Gravity scale when falling normally")]
        public float fallingGravityScale = 3f;

        [Tooltip("Maximum falling speed")] public float maxFallSpeed = 15f;

        [Header("Air Control")] [Tooltip("Horizontal acceleration in air")]
        public float airAcceleration = 20f;

        [Tooltip("Horizontal deceleration in air")]
        public float airDeceleration = 10f;
    }
}
