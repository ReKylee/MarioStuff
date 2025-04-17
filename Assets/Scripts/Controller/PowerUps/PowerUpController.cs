using Collectables;
using Interfaces.PowerUps;
using UnityEngine;

namespace Controller
{
    public abstract class PowerUpController : CollectibleBase
    {
        public abstract IPowerUp CreatePowerUp();
        public override void OnCollect(GameObject collector)
        {
            IPowerUpCollector powerUpCollector = collector.GetComponent<IPowerUpCollector>();

            // NOTE: The Power Up still disappears even if you can't collect it.
            if (powerUpCollector?.CanCollectPowerUps != true)
                return;

            IPowerUp powerUp = CreatePowerUp();
            if (powerUp != null)
            {
                powerUpCollector.ApplyPowerUp(powerUp);
            }
        }
    }
}
