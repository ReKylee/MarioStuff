using Interfaces.Resettable;
using Interfaces.Weapons;
using Managers;

namespace Resettables
{
    public class UsableWeaponResetter : IResettable
    {
        private readonly IUseableWeapon _usableWeapon;
        public UsableWeaponResetter(IUseableWeapon weapon)
        {
            _usableWeapon = weapon;
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            _usableWeapon?.UnEquip();
        }
    }
}
