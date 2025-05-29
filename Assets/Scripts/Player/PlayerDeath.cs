using Interfaces.Damage;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class PlayerDeath : MonoBehaviour
    {
        public UnityEvent onDeath;
        private IDamageable _livesController;

        private void Start()
        {
            _livesController = GetComponent<IDamageable>();
            _livesController.OnEmpty += HandleDeath;
        }
        private void OnDisable()
        {
            _livesController.OnEmpty -= HandleDeath;
        }
        private void HandleDeath()
        {
            onDeath?.Invoke();
        }
    }
}
