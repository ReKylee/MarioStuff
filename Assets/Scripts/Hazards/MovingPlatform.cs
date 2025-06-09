using UnityEngine;

namespace Hazards
{
    [RequireComponent(typeof(Rigidbody2D))] // Ensure Rigidbody2D is present
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float waitTime = 1f;

        private bool _movingToEnd = true;
        private Rigidbody2D _rb;
        private float _waitTimer;
        private Vector2 _worldPathEnd;

        private Vector2 _worldPathStart;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;

            Vector2 initialObjectPosition = new(transform.position.x, transform.position.y);
            _worldPathStart = initialObjectPosition + new Vector2(startPosition.x, startPosition.y);
            _worldPathEnd = initialObjectPosition + new Vector2(endPosition.x, endPosition.y);
        }

        private void Start()
        {
            _rb.position = _worldPathStart;
            _waitTimer = waitTime;
        }

        private void FixedUpdate()
        {
            if (_waitTimer > 0)
            {
                _waitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector2 currentRbPosition = _rb.position;
            Vector2 targetPoint = _movingToEnd ? _worldPathEnd : _worldPathStart;

            Vector2 newPosition =
                Vector2.MoveTowards(currentRbPosition, targetPoint,
                    speed * Time.fixedDeltaTime);

            _rb.MovePosition(newPosition);

            // Check if the platform has reached the target point
            if (Vector2.Distance(newPosition, targetPoint) < 0.01f)
            {
                _movingToEnd = !_movingToEnd;
                _waitTimer = waitTime;
                _rb.MovePosition(targetPoint);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 worldStartPos = transform.position + startPosition;
            Vector3 worldEndPos = transform.position + endPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldStartPos, 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldEndPos, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(worldStartPos, worldEndPos);
        }
    }
}
