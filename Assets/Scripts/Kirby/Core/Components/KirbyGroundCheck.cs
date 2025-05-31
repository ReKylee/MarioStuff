using UnityEditor;
using UnityEngine;

namespace Kirby
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

        private const int CheckPointCount = 3;

        [Header("Ground Detection Settings")] [SerializeField]
        private LayerMask groundLayers;

        [SerializeField] private Vector2 groundCheckSize = new(0.8f, 0.1f);
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private bool drawGizmos = true;

        [Header("Slope Settings")] [SerializeField] [Range(0, 89)]
        private float maxSlopeAngle = 45f;

        [SerializeField] [Range(1, 35)] private float slopeThreshold = 10f;
        [SerializeField] [Range(35, 89)] private float deepSlopeThreshold = 35f;

        // Optimization: use cached reference points for calculating slope points
        private Vector2[] _checkPoints;

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
            // Initialize check points for optimized slope detection
            InitializeCheckPoints();
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Draw ground check box
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 boxPosition = transform.position + (Vector3)(Vector2.down * groundCheckDistance);
            Gizmos.DrawWireCube(boxPosition, groundCheckSize);

            if (IsGrounded)
            {
                // Draw ground normal
                Gizmos.color = Color.blue;
                Vector3 normalEnd = (Vector2)transform.position + GroundNormal * 0.5f;
                Gizmos.DrawLine(transform.position, normalEnd);

                // Draw slope indicator with color based on slope type
                Vector3 slopeDir = Vector3.Cross(Vector3.forward, GroundNormal);

                // Color based on surface type
                switch (CurrentSurface)
                {
                    case SurfaceType.Flat:
                        Gizmos.color = Color.green;
                        break;
                    case SurfaceType.Slope:
                        Gizmos.color = Color.yellow;
                        break;
                    case SurfaceType.DeepSlope:
                        Gizmos.color = new Color(1f, 0.5f, 0f);
                        break;
                }

                Gizmos.DrawRay(transform.position, slopeDir * 0.5f);

                // Draw text label with slope angle and type in the scene view
                if (EditorApplication.isPlaying)
                {
                    Handles.Label(transform.position + Vector3.up * 0.5f,
                        $"{CurrentSurface}: {GroundSlopeAngle:F1}°");
                }
            }

            // Draw ground check points
            if (Application.isPlaying && _checkPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Vector2 point in _checkPoints)
                {
                    Vector2 worldPoint = (Vector2)transform.position + point;
                    Gizmos.DrawRay(worldPoint, Vector2.down * (groundCheckDistance + groundCheckSize.y / 2));
                }
            }
        }

        private void InitializeCheckPoints()
        {
            _checkPoints = new Vector2[CheckPointCount];
            float width = groundCheckSize.x;

            for (int i = 0; i < CheckPointCount; i++)
            {
                float offset = width * ((float)i / (CheckPointCount - 1) - 0.5f);
                _checkPoints[i] = new Vector2(offset, 0);
            }
        }

        private void CheckGround()
        {
            // Use a single box cast for ground detection and slope calculation
            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,
                groundCheckSize,
                0f,
                Vector2.down,
                groundCheckDistance,
                groundLayers
            );

            IsGrounded = hit.collider is not null;

            if (IsGrounded)
            {
                // Store ground normal
                GroundNormal = hit.normal;

                // Optimize with dot product calculations
                // We only need 2 dot products for full slope classification
                float upDot = Vector2.Dot(GroundNormal, Vector2.up);

                // Get slope angle using optimized calculation (acos of dot product)
                GroundSlopeAngle = Mathf.Acos(Mathf.Clamp(upDot, -1f, 1f)) * Mathf.Rad2Deg;

                // Apply sign to the angle (positive = uphill right, negative = uphill left)
                // Using the normal's x component directly since it's already a dot product with right vector
                if (GroundNormal.x != 0)
                {
                    GroundSlopeAngle *= Mathf.Sign(-GroundNormal.x);
                }

                // Fast direct classification of surface type using absolute dot product value
                float absSlopeAngle = Mathf.Abs(GroundSlopeAngle);

                // Classify surface type using direct angle comparisons
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
            else
            {
                // Reset values when not grounded
                GroundNormal = Vector2.up;
                GroundSlopeAngle = 0f;
                CurrentSurface = SurfaceType.None;
            }
        }
    }
}
