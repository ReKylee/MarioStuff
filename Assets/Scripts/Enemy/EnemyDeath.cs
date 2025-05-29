using Interfaces.Damage;
using UnityEngine;

namespace Enemy
{
    public class EnemyDeath : MonoBehaviour
    {
        private IDamageable _hitPointsController;
        private void Start()
        {
            _hitPointsController = GetComponent<IDamageable>();
            _hitPointsController.OnEmpty += HandleDeath;
        }
        private void OnDisable()
        {
            _hitPointsController.OnEmpty -= HandleDeath;
        }

        private void HandleDeath()
        {
            gameObject.SetActive(false);
        }
    }
}
