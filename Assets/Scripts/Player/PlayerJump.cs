using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.2f;

        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            {
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocityX, jumpForce);
            }

            if (Input.GetKeyUp(KeyCode.Space) && _rigid.linearVelocity.y > 0)
            {
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocityX, _rigid.linearVelocityY * jumpCutMultiplier);
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        }
    }
}
