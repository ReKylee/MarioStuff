using UnityEngine;

/// <summary>
/// Context for processing movement in movement abilities
/// </summary>
public class MovementContext
{
    /// <summary>
    /// Current velocity of the character
    /// </summary>
    public Vector2 Velocity { get; set; }
    
    /// <summary>
    /// Desired velocity to be applied to the character
    /// </summary>
    public Vector2 DesiredVelocity { get; set; }
    
    /// <summary>
    /// Whether the character is grounded
    /// </summary>
    public bool IsGrounded { get; set; }
    
    /// <summary>
    /// Normal of the ground surface (if grounded)
    /// </summary>
    public Vector2 GroundNormal { get; set; }
    
    /// <summary>
    /// Angle of the ground in degrees
    /// </summary>
    public float GroundAngle { get; set; }
    
    /// <summary>
    /// Type of slope the character is on
    /// </summary>
    public SlopeType SlopeType { get; set; }
    
    /// <summary>
    /// Whether the character is facing uphill on a slope
    /// </summary>
    public bool IsUphill { get; set; }
    
    /// <summary>
    /// Direction the character is facing (true = right, false = left)
    /// </summary>
    public bool IsFacingRight { get; set; }
    
    /// <summary>
    /// Current form of the character
    /// </summary>
    public CharacterForm CurrentForm { get; set; }
    
    /// <summary>
    /// Delta time for this frame
    /// </summary>
    public float DeltaTime { get; set; }
    
    /// <summary>
    /// The accumulated fall distance when falling
    /// </summary>
    public float FallDistance { get; set; }
    
    /// <summary>
    /// Reference to the character's rigidbody
    /// </summary>
    public Rigidbody2D Rigidbody { get; set; }
    
    /// <summary>
    /// Reference to the character's collider
    /// </summary>
    public Collider2D Collider { get; set; }
}

/// <summary>
/// Types of slopes
/// </summary>
public enum SlopeType
{
    /// <summary>
    /// Flat ground (0-1 degrees)
    /// </summary>
    Flat,
    
    /// <summary>
    /// Gentle slope (1-30 degrees)
    /// </summary>
    Gentle,
    
    /// <summary>
    /// Steep slope (30-60 degrees)
    /// </summary>
    Steep
}

/// <summary>
/// Character forms
/// </summary>
public enum CharacterForm
{
    /// <summary>
    /// Normal Kirby
    /// </summary>
    Normal,
    
    /// <summary>
    /// Full Kirby (after inhaling)
    /// </summary>
    Full,
    
    /// <summary>
    /// Rider ability
    /// </summary>
    Rider,
    
    /// <summary>
    /// Fire ability
    /// </summary>
    Fire,
    
    /// <summary>
    /// Ice ability
    /// </summary>
    Ice
    
    // Add more forms as needed
}
