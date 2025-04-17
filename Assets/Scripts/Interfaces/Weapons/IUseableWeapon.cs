namespace Interfaces.Weapons
{
    public interface IUseableWeapon : IWeapon
    {
        void Equip();
        void UnEquip();
    }
}
