using UnityEngine;

namespace Player.Physics
{
    /// <summary>
    ///     Handles ground detection using a variety of methods
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        // Coyote time handling
        [SerializeField] private float coyoteTime = 0.15f;

        // Ground state
        public bool IsGrounded { get; private set; }
        public bool WasGrounded { get; private set; }
        public float CoyoteTimer { get; private set; }

        private void Update()
        {
            // Update ground state
            WasGrounded = IsGrounded;
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Update coyote time
            if (WasGrounded && !IsGrounded)
            {
                CoyoteTimer = coyoteTime;
            }
            else if (!WasGrounded && !IsGrounded)
            {
                CoyoteTimer -= Time.deltaTime;
            }
            else if (IsGrounded)
            {
                CoyoteTimer = 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
