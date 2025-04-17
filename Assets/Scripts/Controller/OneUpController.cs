using Interfaces;
using UnityEngine;

namespace Controller
{
    public class OneUpController : PowerUpController
    {
        [SerializeField] private int healAmount = 1;

        public override IPowerUp CreatePowerUp()
        {
            return new OneUpPowerUp(healAmount);
        }
    }
}
