using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 5;
        private float _direction;
        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            _direction = Input.GetAxis("Horizontal");
            if (_direction != 0 && _rigid)
            {
                _rigid.linearVelocity = new Vector2(_direction * speed, _rigid.linearVelocity.y);

                transform.localScale = _direction > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            }
        }
    }
}
