using Controller;
using UnityEngine;

namespace Enemy
{
    public class EnemyDeath : MonoBehaviour
    {

        private HealthController _health;

        private void Awake()
        {
            _health = GetComponent<HealthController>();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            Destroy(gameObject);
        }
    }
}
