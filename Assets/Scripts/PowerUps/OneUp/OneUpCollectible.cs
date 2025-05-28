using Interfaces;
using Interfaces.PowerUps;
using UnityEngine;

namespace Controller
{
    public class OneUpCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private int healAmount = 1;

        public override IPowerUp CreatePowerUp()
        {
            return new OneUpPowerUp(healAmount);
        }
    }
}
