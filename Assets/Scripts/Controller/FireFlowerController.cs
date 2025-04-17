using Interfaces;

namespace Controller
{
    public class FireFlowerController : PowerUpController
    {
        public override IPowerUp CreatePowerUp()
        {
            return new FireFlowerPowerUp();
        }
    }
}
