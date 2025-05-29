using Resettables;
using UnityEngine;

namespace Weapons
{
    public class WeaponsHandler : MonoBehaviour
    {
        public FireballWeapon fireballWeapon;
        public AxeWeapon axeWeapon;
        private AmmoResetter _axeResetter;
        private UsableWeaponResetter _fireballResetter;
        private void Awake()
        {
            InjectDependencies();
        }
        private void Update()
        {
            ActivateWeapon();
        }
        private void OnDestroy()
        {
            _fireballResetter.Dispose();
            _axeResetter.Dispose();
        }
        private void InjectDependencies()
        {
            if (fireballWeapon)
            {
                _fireballResetter = new UsableWeaponResetter(fireballWeapon);
            }

            if (axeWeapon)
            {
                _axeResetter = new AmmoResetter(axeWeapon);
            }
        }

        private void ActivateWeapon()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && fireballWeapon)
                fireballWeapon.Shoot();

            if (Input.GetKeyDown(KeyCode.X) && axeWeapon)
                axeWeapon.Shoot();
        }
    }
}
