using System;
using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Settings for Kirby's animation system
    /// </summary>
    [Serializable]
    public class AnimationSettings
    {
        [Header("Input Thresholds")] [Tooltip("Threshold for detecting run input")]
        public float runInputThreshold = 0.7f;

        [Tooltip("Threshold for detecting movement input")]
        public float moveInputThreshold = 0.1f;

        [Header("Bounce Settings")] [Tooltip("Time falling before bounce animation starts")]
        public float fallTimeBeforeBounce = 0.5f;

        [Tooltip("Height threshold to trigger bounce")]
        public float bounceHeightThreshold = 3f;

        [Tooltip("Force applied during bounce")]
        public float bounceForce = 5f;

        [Tooltip("Distance to ground before playing bounce animation")]
        public float preFloorBounceDistance = 0.5f;
    }
}
