using System.Collections;
using UnityEngine;

public class ProjectileFireball : MonoBehaviour
{
    public float speed = 5.0f;
    public float destroyTime = 5f;
    private Rigidbody2D _rigid;

    public void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    public void Shoot(float direction)
    {
        if (!_rigid) return;
        transform.localScale = new Vector3(direction, 1, 1);
        _rigid.AddForce(new Vector3(speed * direction, 0, 0));
        StartCoroutine(DestroyObject());
    }

    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
