using Interfaces;

namespace Controller
{
    public class PickableAxeController : PowerUpController
    {

        protected override IPowerUp CreatePowerUp()
        {
            return new PickableAxePowerUp();
        }
    }
}
