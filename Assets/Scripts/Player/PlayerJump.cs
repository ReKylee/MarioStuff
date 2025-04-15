using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        public float jumpSpeed = 100;
        private bool _isJumping;
        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Jump();
        }

        private void OnEnable()
        {
            SC_Floor.OnFloorCollision += OnFloorCollision;
        }

        private void OnDisable()
        {
            SC_Floor.OnFloorCollision -= OnFloorCollision;
        }

        private void Jump()
        {
            if (!_isJumping)
            {
                _rigid.AddForce(new Vector2(0, jumpSpeed));
                _isJumping = true;
            }
        }
        private void OnFloorCollision()
        {
            _isJumping = false;
        }
    }
}
