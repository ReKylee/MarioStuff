using Interfaces.PowerUps;

namespace Controller
{
    public class PickableAxeController : PowerUpController
    {

        public override IPowerUp CreatePowerUp()
        {
            return new PickableAxePowerUp();
        }
    }
}
