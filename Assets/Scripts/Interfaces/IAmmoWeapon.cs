namespace Interfaces
{
    public interface IAmmoWeapon : IWeaponReload
    {
        int CurrentAmmo { get; }
        void SetAmmo(int ammo);
    }
}
