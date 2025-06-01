using UnityEditor;
using UnityEngine;

namespace Kirby.Core.Components
{
    /// <summary>
    ///     Handles ground detection for the Kirby controller with slope information
    /// </summary>
    public class KirbyGroundCheck : MonoBehaviour
    {
        public enum SurfaceType
        {
            None,
            Flat,
            Slope,
            DeepSlope
        }

        private const int CheckPointCount = 5; // Increased for better accuracy

        [Header("Ground Detection Settings")] [SerializeField]
        private LayerMask groundLayers;

        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
        [SerializeField] private Vector2 groundCheckSize = new(0.8f, 0.1f);
        [SerializeField] private bool drawGizmos = true;

        [Header("Slope Settings")] [SerializeField] [Range(0, 89)]
        private float maxSlopeAngle = 45f;

        [SerializeField] [Range(1, 35)] private float slopeThreshold = 10f;
        [SerializeField] [Range(35, 89)] private float deepSlopeThreshold = 35f;

        // Cache RaycastHit2D array to avoid GC allocation
        private readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[1];

        // Optimization: cached reference points for ground check
        private Vector2[] _checkPoints;
        private float _checkRayLength;

        /// <summary>
        ///     Returns true if the character is grounded
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        ///     Returns the current ground slope angle in degrees (0 = flat, positive = uphill, negative = downhill)
        /// </summary>
        public float GroundSlopeAngle { get; private set; }

        /// <summary>
        ///     Returns the normal vector of the ground surface
        /// </summary>
        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        /// <summary>
        ///     Returns the surface type the character is standing on
        /// </summary>
        public SurfaceType CurrentSurface { get; private set; } = SurfaceType.None;

        private void Awake()
        {
            InitializeCheckPoints();
            _checkRayLength = groundCheckSize.y * 0.75f; // Cast slightly longer than half the box height
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Calculate the position of the ground check box using the offset
            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;

            // Draw ground check box
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(boxPosition, groundCheckSize);

            if (IsGrounded)
            {
                // Draw ground normal from the bottom center of the box
                Vector3 normalOrigin = boxPosition - new Vector2(0, groundCheckSize.y * 0.5f);
                Gizmos.color = Color.blue;
                Vector3 normalEnd = normalOrigin + (Vector3)GroundNormal * 0.5f;
                Gizmos.DrawLine(normalOrigin, normalEnd);

                // Draw slope direction (tangent to the surface)
                Vector3 slopeDir = new Vector3(GroundNormal.y, -GroundNormal.x, 0).normalized;
                // Make sure slope direction points right when going uphill
                if (GroundSlopeAngle < 0) slopeDir = -slopeDir;

                // Color based on surface type
                Gizmos.color = CurrentSurface switch
                {
                    SurfaceType.Flat => Color.green,
                    SurfaceType.Slope => Color.yellow,
                    SurfaceType.DeepSlope => new Color(1f, 0.5f, 0f), // Orange
                    _ => Color.white
                };

                Gizmos.DrawRay(normalOrigin, slopeDir * 0.5f);

                // Draw text label with slope angle and type in the scene view
                if (EditorApplication.isPlaying)
                {
                    Handles.Label(boxPosition + Vector2.up * 0.3f,
                        $"{CurrentSurface}: {GroundSlopeAngle:F1}°");
                }
            }

            // Draw ground check points
            if (Application.isPlaying && _checkPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Vector2 point in _checkPoints)
                {
                    Vector2 worldPoint = boxPosition + point;
                    Gizmos.DrawSphere(worldPoint, 0.025f);

                    // Draw the raycast line
                    Gizmos.DrawLine(
                        worldPoint + new Vector2(0, groundCheckSize.y * 0.05f),
                        worldPoint + new Vector2(0, -_checkRayLength)
                    );
                }
            }
        }

        /// <summary>
        ///     Returns the maximum slope angle Kirby can walk on
        /// </summary>
        public float GetMaxSlopeAngle() => maxSlopeAngle;

        /// <summary>
        ///     Returns the threshold angle for what constitutes a deep slope
        /// </summary>
        public float GetDeepSlopeThreshold() => deepSlopeThreshold;

        private void InitializeCheckPoints()
        {
            _checkPoints = new Vector2[CheckPointCount];
            float width = groundCheckSize.x;

            // Create evenly spaced check points across the bottom of the box
            for (int i = 0; i < CheckPointCount; i++)
            {
                float xOffset = width * ((float)i / (CheckPointCount - 1) - 0.5f);
                // Position points at the bottom of the box
                _checkPoints[i] = new Vector2(xOffset, -groundCheckSize.y * 0.5f);
            }
        }

        private void CheckGround()
        {
            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;

            // Reset values
            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundSlopeAngle = 0f;
            CurrentSurface = SurfaceType.None;

            int hitCount = 0;
            Vector2 averageNormal = Vector2.zero;

            // Check all points for ground contact
            foreach (Vector2 point in _checkPoints)
            {
                Vector2 rayOrigin = boxPosition + point + new Vector2(0, groundCheckSize.y * 0.05f); // Slight offset up

                // Use non-allocating raycast that writes to our array
                if (Physics2D.RaycastNonAlloc(rayOrigin, Vector2.down, _raycastResults, _checkRayLength, groundLayers) >
                    0)
                {
                    hitCount++;
                    averageNormal += _raycastResults[0].normal;
                    IsGrounded = true;
                }
            }

            // If no raycasts hit, try a box overlap as fallback
            if (hitCount == 0)
            {
                Collider2D hit = Physics2D.OverlapBox(boxPosition, groundCheckSize, 0f, groundLayers);
                if (hit)
                {
                    IsGrounded = true;

                    // Use a center raycast for normal in this case
                    // Fix: Use the correct Raycast method that returns a RaycastHit2D
                    RaycastHit2D centerHit = Physics2D.Raycast(
                        boxPosition,
                        Vector2.down,
                        groundCheckSize.y * 0.6f,
                        groundLayers);

                    if (centerHit.collider != null)
                    {
                        GroundNormal = centerHit.normal;
                    }
                }
            }
            else
            {
                // Calculate average normal from all contacts
                GroundNormal = (averageNormal / hitCount).normalized;
            }

            if (IsGrounded)
            {
                // Calculate slope angle using the dot product method
                float upDot = Vector2.Dot(GroundNormal, Vector2.up);
                float slopeAngleRad = Mathf.Acos(Mathf.Clamp(upDot, -1f, 1f));
                GroundSlopeAngle = slopeAngleRad * Mathf.Rad2Deg;

                // Apply correct sign based on normal direction
                if (GroundNormal.x != 0)
                {
                    GroundSlopeAngle *= Mathf.Sign(-GroundNormal.x);
                }

                // Classify surface type based on absolute slope angle
                float absSlopeAngle = Mathf.Abs(GroundSlopeAngle);
                if (absSlopeAngle <= slopeThreshold)
                {
                    CurrentSurface = SurfaceType.Flat;
                }
                else if (absSlopeAngle < deepSlopeThreshold)
                {
                    CurrentSurface = SurfaceType.Slope;
                }
                else
                {
                    CurrentSurface = SurfaceType.DeepSlope;
                }
            }
        }
    }
}
