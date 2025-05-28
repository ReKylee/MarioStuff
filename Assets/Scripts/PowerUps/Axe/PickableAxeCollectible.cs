using Interfaces.PowerUps;

namespace Controller
{
    public class PickableAxeCollectible : PowerUpCollectibleBase
    {

        public override IPowerUp CreatePowerUp()
        {
            return new PickableAxePowerUp();
        }
    }
}
