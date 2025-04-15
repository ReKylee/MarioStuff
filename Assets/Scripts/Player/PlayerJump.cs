using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 10f;

        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private float groundCheckRadius = 0.05f;

        [SerializeField] private float groundCheckDistance = 1f;
        private bool _isGrounded;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            CheckGround();

            if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }


        private void CheckGround()
        {
            _isGrounded = Physics2D.CircleCast(
                transform.position,
                groundCheckRadius,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

        }

        private void Jump()
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}
