using UnityEngine;

namespace Interfaces
{
    public class OneUpPowerUp : IPowerUp
    {
        private readonly int _healAmount;
        public OneUpPowerUp(int healAmount = 1)
        {
            _healAmount = healAmount;
        }

        public void ApplyPowerUp(GameObject player)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            damageable.Heal(_healAmount);
        }
    }
}
