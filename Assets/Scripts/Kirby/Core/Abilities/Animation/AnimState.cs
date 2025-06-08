using UnityEngine;

namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    /// Enum representing all possible animation states for Kirby
    /// </summary>
    public enum AnimState
    {
        // Ground States
        Idle,
        Walk,
        Run,
        Crouch,

        // Jump States
        JumpStart,
        Jump,
        Fall,
        BounceOffFloor,

        // Flying States
        JumpToFly,
        Fly,
        Float,

        // Special States
        Inhale,
        Spit,
        Swallow,
        Skid
    }
}
