using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    ///     Handles horizontal movement with acceleration/deceleration for a responsive feel
    /// </summary>
    public class WalkAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private const float RUN_MOMENTUM_DURATION = 0.2f;
        private const float WALL_DETECTION_DISTANCE = 0.1f;

        // Cache for collider bounds calculation
        private Bounds _colliderBounds;

        // Track direction facing for sprite flipping
        private int _facingDirection = 1;
        private Vector2 _previousDirection = Vector2.zero;
        private float _runningTimer;
        private int _wallHitCount;

        // Cache raycast hit results
        private RaycastHit2D[] _wallHits = new RaycastHit2D[3];

        // Track if player was recently running for maintaining momentum
        private bool _wasRunning;

        public Vector2 ProcessMovement(
            Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            if (!Controller || Controller.Stats == null) return currentVelocity;

            // Convert input into a normalized direction vector
            Vector2 inputDirection = new Vector2(inputContext.MoveInput.x, 0).normalized;
            float inputMagnitude = inputDirection.magnitude;
            bool hasInput = inputMagnitude > 0.01f;

            // Create velocity direction vector (normalized)
            Vector2 velocityDirection = currentVelocity.magnitude > 0.01f
                ? new Vector2(currentVelocity.x, 0).normalized
                : Vector2.zero;

            // Update facing direction when there's input
            if (hasInput)
            {
                _facingDirection = inputDirection.x > 0 ? 1 : -1;
            }

            // Determine acceleration/deceleration rates based on ground state
            float acceleration = isGrounded ? Controller.Stats.groundAcceleration : Controller.Stats.airAcceleration;
            float deceleration = isGrounded ? Controller.Stats.groundDeceleration : Controller.Stats.airDeceleration;

            // Maintain running momentum
            if (Mathf.Abs(currentVelocity.x) >= Controller.Stats.runSpeed * 0.9f)
            {
                _wasRunning = true;
                _runningTimer = RUN_MOMENTUM_DURATION;
            }
            else if (_runningTimer > 0)
            {
                _runningTimer -= Time.deltaTime;
                if (_runningTimer <= 0)
                {
                    _wasRunning = false;
                }
            }

            // Determine target speed
            float targetSpeed = Controller.Stats.walkSpeed;

            // Use run speed if:
            // 1. Already moving fast and continuing to move, or
            // 2. Recently was running and moving in same direction
            bool shouldRun = false;

            if (hasInput)
            {
                // Calculate dot product between input and velocity directions
                // A value close to 1 means same direction, close to -1 means opposite directions
                float directionDot = velocityDirection.magnitude > 0.01f
                    ? Vector2.Dot(inputDirection, velocityDirection)
                    : 0;

                if (Mathf.Abs(currentVelocity.x) > Controller.Stats.walkSpeed ||
                    _wasRunning && directionDot > 0) // Using dot product to check same direction
                {
                    targetSpeed = Controller.Stats.runSpeed;
                    shouldRun = true;
                }
            }

            // Check for wall collision in movement direction to prevent wall sticking
            bool isAgainstWall = CheckForWall(inputDirection);

            // Apply horizontal movement with acceleration/deceleration
            if (hasInput && !isAgainstWall)
            {
                // Apply acceleration toward target speed
                float targetVelocity = targetSpeed * _facingDirection;

                // Use dot product to detect direction changes more efficiently
                float directionChange = velocityDirection.magnitude > 0.01f
                    ? Vector2.Dot(inputDirection, velocityDirection)
                    : 1;

                // Higher acceleration when changing directions for responsiveness
                float appliedAcceleration = acceleration;
                if (directionChange < 0 && Mathf.Abs(currentVelocity.x) > 0.1f)
                {
                    appliedAcceleration *= 1.5f; // Turn around faster
                }

                currentVelocity.x = Mathf.MoveTowards(
                    currentVelocity.x,
                    targetVelocity,
                    appliedAcceleration * Time.deltaTime
                );
            }
            else
            {
                // Apply deceleration when no input or against wall
                currentVelocity.x = Mathf.MoveTowards(
                    currentVelocity.x,
                    0,
                    deceleration * Time.deltaTime
                );
            }

            // Apply maximum horizontal speed constraint (in case of external forces)
            float maxHorizontalSpeed = shouldRun ? Controller.Stats.runSpeed : Controller.Stats.walkSpeed;
            maxHorizontalSpeed *= 1.1f; // Allow slight overspeed from external forces
            currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);

            // Store previous direction for next frame comparison
            _previousDirection = inputDirection;

            return currentVelocity;
        }

        private bool CheckForWall(Vector2 inputDirection)
        {
            // If no input or missing components, no wall check needed
            if (inputDirection.magnitude < 0.01f || !Controller || !Controller.Collider)
                return false;

            // Get the collider bounds directly
            _colliderBounds = Controller.Collider.bounds;

            // Simple and efficient wall check using a single BoxCast
            Vector2 rayDirection = inputDirection.x > 0 ? Vector2.right : Vector2.left;
            float xOffset = _colliderBounds.extents.x * Mathf.Sign(inputDirection.x);

            // Calculate positions
            Vector2 startPos = new(_colliderBounds.center.x + xOffset, _colliderBounds.center.y);
            Vector2 boxSize = new(WALL_DETECTION_DISTANCE, _colliderBounds.size.y * 0.7f);

            // Single efficient BoxCast
            return Physics2D.BoxCast(
                startPos,
                boxSize,
                0f,
                rayDirection,
                0.01f, // Minimal distance needed for detection
                LayerMask.GetMask("Ground", "Wall")
            );
        }
    }
}
