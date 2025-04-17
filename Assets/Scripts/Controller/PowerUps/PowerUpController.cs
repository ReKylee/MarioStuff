using Collectables;
using Interfaces.PowerUps;
using UnityEngine;

namespace Controller
{
    public abstract class PowerUpController : CollectibleBase, IPowerUpProvider
    {
        public abstract IPowerUp CreatePowerUp();
        public override void OnCollect(GameObject collector)
        {
            IPowerUpCollector powerUpCollector = collector.GetComponent<IPowerUpCollector>();

            // NOTE: The Power Up still disappears even if you can't collect it.
            if (powerUpCollector is { CanCollectPowerUps: true })
            {
                IPowerUp powerUp = CreatePowerUp();
                if (powerUp != null)
                {
                    powerUpCollector.ApplyPowerUp(powerUp);
                }
            }
        }
    }
}
