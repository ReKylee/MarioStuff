using UnityEngine;

public class MovingPlatform2D : MonoBehaviour
{
    [Header("Platform Movement")] [SerializeField]
    private float speed = 2f;

    [SerializeField] private float waitTime = 1f;

    [Header("Position Points (assign as children)")] [SerializeField]
    private Transform startPoint;

    [SerializeField] private Transform endPoint;

    [Header("Player Settings")] [SerializeField]
    private string playerTag = "Player";

    private bool _movingToEnd = true;
    private Transform _originalParent;
    private Rigidbody2D _rb;
    private Vector3 _targetPosition;
    private float _waitTimer;

    private void Start()
    {
        // Get Rigidbody2D component
        _rb = GetComponent<Rigidbody2D>();
        if (!_rb)
        {
            Debug.LogError("MovingPlatform2D requires a Rigidbody2D component!");
            return;
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;

        if (!startPoint || !endPoint)
        {
            Debug.LogWarning("Start and End points not assigned.");
            return;
        }

        _targetPosition = endPoint.position;
        _rb.MovePosition(startPoint.position);
    }

    private void FixedUpdate()
    {
        MovePlatform();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag)
            && collision.transform.position.y > transform.position.y)
        {
            _originalParent = collision.transform.parent;
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(_originalParent);
        }
    }


    private void MovePlatform()
    {
        if (!startPoint || !endPoint || !_rb) return;
        if (_waitTimer > 0)
        {
            _waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 newPosition = Vector3.MoveTowards(_rb.position, _targetPosition, speed * Time.deltaTime);

        _rb.MovePosition(newPosition);

        if (Vector3.Distance(_rb.position, _targetPosition) < 0.01f)
        {
            _targetPosition = _movingToEnd ? endPoint.position : startPoint.position;
            _movingToEnd = !_movingToEnd;
            _waitTimer = waitTime;
            _rb.MovePosition(_targetPosition);
        }
    }
}
