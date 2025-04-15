using Interfaces;

namespace Controller
{
    public class FireFlowerController : PowerUpController
    {
        protected override IPowerUp CreatePowerUp()
        {
            return new FireFlowerPowerUp();
        }
    }
}
