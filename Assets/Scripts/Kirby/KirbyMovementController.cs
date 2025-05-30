using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Implementation of IMovementController for Kirby
    /// </summary>
    public class KirbyMovementController : IMovementController
    {
        private const float groundCheckDistance = 0.1f;

        // Ground detection
        private readonly LayerMask groundLayer;
        private readonly Rigidbody2D rb;
        private bool flyingInitialized;
        private RaycastHit2D groundHit;
        private float initialYPosition;

        // State tracking
        private float jumpHoldTimeRemaining;

        // Flying tracking
        private float lastFlapTime;
        private MovementParameters movementParams;

        public KirbyMovementController(Rigidbody2D rigidbody, MovementParameters parameters)
        {
            rb = rigidbody;
            movementParams = parameters;

            // Configure rigidbody
            rb.gravityScale = parameters.fallingGravityScale;
            rb.freezeRotation = true;

            // Configure ground detection
            groundLayer = LayerMask.GetMask("Ground");
        }

        // Public properties
        public bool IsGrounded { get; private set; }

        public float TerrainAngle { get; private set; }

        public bool IsOnSlope => Mathf.Abs(TerrainAngle) > 10f && Mathf.Abs(TerrainAngle) < 45f;
        public bool IsOnDeepSlope => Mathf.Abs(TerrainAngle) >= 45f;
        public bool IsFacingLeft => rb.transform.localScale.x < 0;
        public Vector2 CurrentVelocity => rb.linearVelocity;

        /// <summary>
        ///     Moves Kirby horizontally
        /// </summary>
        public void MoveHorizontal(float horizontalInput, bool isRunning)
        {
            // Get target speed based on input and whether we're running
            float targetSpeed =
                horizontalInput * (isRunning ? movementParams.maxRunSpeed : movementParams.maxWalkSpeed);

            // Get current horizontal velocity
            float currentSpeed = rb.linearVelocity.x;

            // Calculate acceleration to use (deceleration if changing direction or stopping)
            float accelRate;
            if (IsGrounded)
            {
                // If we're changing direction or stopping, use deceleration
                accelRate = Mathf.Abs(targetSpeed) > 0.01f
                    ? Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed)
                        ? movementParams.groundDeceleration
                        : movementParams.groundAcceleration
                    : movementParams.groundDeceleration;
            }
            else
            {
                // In air, use air acceleration/deceleration
                accelRate = Mathf.Abs(targetSpeed) > 0.01f
                    ? Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed)
                        ? movementParams.airDeceleration
                        : movementParams.airAcceleration
                    : movementParams.airDeceleration;
            }

            // Calculate new speed
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // Apply the new velocity
            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

            // Flip the sprite based on direction if we're moving
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                Vector3 scale = rb.transform.localScale;
                if (horizontalInput > 0 && scale.x < 0 || horizontalInput < 0 && scale.x > 0)
                {
                    scale.x *= -1;
                    rb.transform.localScale = scale;
                }
            }
        }

        /// <summary>
        ///     Makes Kirby jump with variable height based on how long the jump button is held
        /// </summary>
        public void Jump(bool jumpHeld)
        {
            // Initial jump
            if (IsGrounded)
            {
                // Apply initial jump force
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, movementParams.jumpForce);

                // Start the jump hold time tracking
                jumpHoldTimeRemaining = movementParams.maxJumpHoldTime;

                // No longer grounded
                IsGrounded = false;
            }
            // Variable jump height when holding jump
            else if (jumpHeld && jumpHoldTimeRemaining > 0)
            {
                // Apply additional upward force
                rb.AddForce(Vector2.up * movementParams.jumpHoldForce, ForceMode2D.Force);

                // Reduce remaining hold time
                jumpHoldTimeRemaining -= Time.fixedDeltaTime;
            }
            // If jump button released, end jump hold
            else if (!jumpHeld)
            {
                jumpHoldTimeRemaining = 0;
            }
        }

        /// <summary>
        ///     Makes Kirby flap while flying to gain height
        /// </summary>
        public void FlyFlap()
        {
            // Check if enough time has passed since last flap
            if (Time.time - lastFlapTime < movementParams.flapCooldown)
                return;

            // Check if Kirby is already at max height
            float currentHeight = rb.transform.position.y - initialYPosition;
            if (currentHeight >= movementParams.maxFlyHeight)
            {
                // Don't allow further upward movement if at max height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(0, rb.linearVelocity.y));
                return;
            }

            // Reset vertical velocity to prevent accumulated momentum
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // Apply flap force with height limitation
            float adjustedFlapForce = movementParams.flapForce;

            // Reduce flap force as Kirby approaches max height
            if (currentHeight > movementParams.maxFlyHeight * 0.7f)
            {
                float heightRatio = (currentHeight - movementParams.maxFlyHeight * 0.7f) /
                                    (movementParams.maxFlyHeight * 0.3f);

                adjustedFlapForce *= Mathf.Clamp01(1 - heightRatio);
            }

            // Apply the adjusted flap force
            rb.AddForce(Vector2.up * adjustedFlapForce, ForceMode2D.Impulse);

            // Update last flap time
            lastFlapTime = Time.time;
        }

        /// <summary>
        ///     Applies gentle descent physics for flying state
        /// </summary>
        public void FlyGentleDescent()
        {
            if (!flyingInitialized)
            {
                InitializeFlying();
            }

            // Set gravity scale to 0 to disable normal gravity
            rb.gravityScale = 0f;

            // Apply a constant gentle downward force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -movementParams.gentleDescentForce);
        }

        /// <summary>
        ///     Applies standard falling physics
        /// </summary>
        public void ApplyFallingPhysics()
        {
            // Reset flying state tracking
            flyingInitialized = false;

            // Restore normal gravity
            rb.gravityScale = movementParams.fallingGravityScale;

            // Cap fall speed
            if (rb.linearVelocity.y < -movementParams.maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -movementParams.maxFallSpeed);
            }
        }

        /// <summary>
        ///     Reset all velocity
        /// </summary>
        public void ResetVelocity()
        {
            rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        ///     Initialize flying state
        /// </summary>
        public void InitializeFlying()
        {
            // Store initial Y position to enforce max height
            initialYPosition = rb.transform.position.y;
            flyingInitialized = true;

            // Reset gravity to prevent normal falling physics
            rb.gravityScale = 0f;

            // Reset vertical velocity to start with gentle descent
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // Set initial flap time
            lastFlapTime = -movementParams.flapCooldown; // Allow immediate flap
        }

        /// <summary>
        ///     Updates ground detection - should be called before using ground-related properties
        /// </summary>
        public void UpdateGroundDetection()
        {
            // Raycast downward to check for ground
            groundHit = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, groundLayer);
            IsGrounded = groundHit.collider != null;

            // Calculate terrain angle if grounded
            if (IsGrounded)
            {
                TerrainAngle = Vector2.SignedAngle(Vector2.up, groundHit.normal);

                // Reset flying state when touching ground
                flyingInitialized = false;
            }
            else
            {
                TerrainAngle = 0f;
            }
        }

        /// <summary>
        ///     Set new movement parameters (used when changing forms)
        /// </summary>
        public void SetMovementParameters(MovementParameters parameters)
        {
            movementParams = parameters;
            rb.gravityScale = parameters.fallingGravityScale;
        }

        /// <summary>
        ///     Gets the current movement parameters
        /// </summary>
        public MovementParameters GetMovementParameters() => movementParams;
    }
}
