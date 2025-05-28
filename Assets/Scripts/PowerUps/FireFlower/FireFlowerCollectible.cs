using Interfaces.PowerUps;

namespace Controller
{
    public class FireFlowerCollectible : PowerUpCollectibleBase
    {
        public override IPowerUp CreatePowerUp()
        {
            return new FireFlowerPowerUp();
        }
    }
}
