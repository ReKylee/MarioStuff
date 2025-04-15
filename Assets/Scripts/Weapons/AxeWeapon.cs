using System;
using Interfaces;
using UnityEngine;

public class AxeWeapon : MonoBehaviour, IAmmoWeapon
{

    public GameObject axe;
    public int CurrentAmmo { get; private set; }
    public void SetAmmo(int ammo)
    {
        CurrentAmmo = ammo;
    }


    public void Shoot()
    {
        if (axe && CurrentAmmo != 0)
        {
            GameObject curAxe = Instantiate(axe, transform.position, new Quaternion());
            ProjectileAxe scAxe = curAxe.GetComponent<ProjectileAxe>();
            if (scAxe)
            {
                CurrentAmmo--;
                OnAxeFired?.Invoke(CurrentAmmo);
                float direction = transform.parent?.localScale.x ?? 1;
                scAxe.Shoot(direction);
            }
        }
    }
    public void Reload()
    {
        CurrentAmmo++;
        OnAxeCollected?.Invoke(CurrentAmmo);
    }
    public static event Action<int> OnAxeCollected;
    public static event Action<int> OnAxeFired;
}
