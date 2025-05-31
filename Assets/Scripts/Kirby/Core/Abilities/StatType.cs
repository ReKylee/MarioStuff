namespace Kirby.Abilities
{
    /// <summary>
    ///     Enum of all stats that abilities can modify
    /// </summary>
    public enum StatType
    {
        // Movement
        WalkSpeed,
        RunSpeed,
        GroundAcceleration,
        GroundDeceleration,
        AirAcceleration,
        AirDeceleration,

        // Jump
        JumpVelocity,
        JumpReleaseGravityMultiplier,
        MaxFallSpeed,
        CoyoteTime,
        JumpBufferTime,

        // Float
        FloatAscendSpeed,
        FloatDescentSpeed,
        FlapImpulse,
        FlyMaxHeight,

        // Physics
        GravityScale,
        GravityScaleDescending,
        GroundCheckRadius,

        // Combat
        AttackDamage,
        AttackRange,
        AttackSpeed,

        // Other
        InhaleRange,
        InhalePower
    }
}
