using Interfaces.Damage;
using UnityEngine;

namespace Managers
{
    public class PlayerDeath : MonoBehaviour
    {
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
            ResetManager.Instance?.ResetAll();
        }
    }
}
