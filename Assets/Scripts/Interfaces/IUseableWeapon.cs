namespace Interfaces
{
    public interface IUseableWeapon : IWeapon
    {
        void Equip();
        void UnEquip();
    }
}
