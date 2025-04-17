using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 15f;

        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            float direction = Input.GetAxis("Horizontal");
            float targetSpeed = direction * maxSpeed;
            float speedDiff = targetSpeed - _rigid.linearVelocityX;
            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);

            _rigid.AddForce(new Vector2(movement, 0));

            if (direction != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
            }
        }
    }
}
