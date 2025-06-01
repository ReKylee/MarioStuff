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

        [Header("Ground Detection Settings")] [SerializeField]
        private LayerMask groundLayers;

        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
        [SerializeField] private Vector2 groundCheckSize = new(0.8f, 0.1f);
        [SerializeField] private bool drawGizmos = true;

        [Header("Slope Settings")] [SerializeField] [Range(0, 89)]
        private float maxSlopeAngle = 45f;

        [SerializeField] [Range(1, 35)] private float slopeThreshold = 10f;
        [SerializeField] [Range(35, 89)] private float deepSlopeThreshold = 35f;


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

#if UNITY_EDITOR
                // Draw text label with slope angle and type in the scene view
                Handles.Label(boxPosition + Vector2.up * 0.3f,
                    $"{CurrentSurface}: {GroundSlopeAngle:F1}°");
#endif
            }

            // Draw raycasts used for fallback detection
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Vector2 rayOrigin = new(
                    boxPosition.x,
                    boxPosition.y + groundCheckSize.y * 0.5f + 0.05f
                );

                Gizmos.DrawRay(rayOrigin, Vector2.down * (groundCheckSize.y + 0.1f));
            }
        }

        private void CheckGround()
        {
            // Reset values
            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundSlopeAngle = 0f;
            CurrentSurface = SurfaceType.None;

            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;

            // Use BoxCast to get both collision and normal information in one call
            RaycastHit2D hit = Physics2D.BoxCast(
                boxPosition,
                groundCheckSize,
                0f,
                Vector2.down,
                0f,
                groundLayers
            );

            if (!hit.collider)
            {
                return;
            }

            // We found ground
            IsGrounded = true;
            GroundNormal = hit.normal;

            // Calculate slope angle using the dot product method
            float slopeAngleRad = Mathf.Acos(Mathf.Clamp(Vector2.Dot(GroundNormal, Vector2.up), -1f, 1f));
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
