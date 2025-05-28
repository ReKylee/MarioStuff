using Health.HP;
using Health.Lives;
using UnityEngine;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        private HitPointController _hitPointController;
        private LivesController _livesController;

        private void Awake()
        {
            _hitPointController = GetComponent<HitPointController>();
            _livesController = GetComponent<LivesController>();
        }

        private void OnEnable()
        {
            if (_hitPointController?.Health != null)
            {
                _hitPointController.Health.OnEmpty += HandleHealthEmpty;
            }

            if (_livesController?.Lives != null)
            {
                _livesController.Lives.OnLifeLost += HandleLifeLost;
            }
        }

        private void OnDisable()
        {
            if (_hitPointController?.Health != null)
            {
                _hitPointController.Health.OnEmpty -= HandleHealthEmpty;
            }

            if (_livesController?.Lives != null)
            {
                _livesController.Lives.OnLifeLost -= HandleLifeLost;
            }
        }

        private void HandleHealthEmpty()
        {
            _livesController?.Lives?.Lose();
        }

        private void HandleLifeLost()
        {
            _hitPointController?.Health?.Reset();
        }
    }
}

