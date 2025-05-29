using Interfaces.Damage;
using UnityEngine;

namespace Enemy
{
    public class EnemyDeath : MonoBehaviour
    {
        private IDamageable _simpleHealthController;
        private void Start()
        {
            _simpleHealthController = GetComponent<IDamageable>();
            _simpleHealthController.OnEmpty += HandleDeath;
        }
        private void OnDisable()
        {
            _simpleHealthController.OnEmpty -= HandleDeath;
        }

        private void HandleDeath()
        {
            gameObject.SetActive(false);
        }
    }
}
