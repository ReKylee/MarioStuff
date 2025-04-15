using Controller;
using Resettables;
using UnityEngine;

namespace Managers
{
    public class PlayerDeath : MonoBehaviour
    {
        private HealthController _health;
        private HealthResetter _healthResetter;

        private void Awake()
        {
            _health = GetComponent<HealthController>();
            _healthResetter = new HealthResetter(_health);
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private static void HandleDeath()
        {
            ResetManager.Instance?.ResetAll();
        }
    }
}
