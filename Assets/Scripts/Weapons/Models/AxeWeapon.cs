using System;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class AxeWeapon : MonoBehaviour, IAmmoWeapon
    {
        [SerializeField] private GameObject axe;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int maxAmmo = 3;
        [SerializeField] private float cooldownTime = 0.5f;

        private float _nextFireTime;

        private void Awake()
        {
            CurrentAmmo = 0;
        }

        public int CurrentAmmo { get; private set; }
        public int MaxAmmo => maxAmmo;

        // Check if weapon has ammo
        public bool HasAmmo => CurrentAmmo > 0;

        public void SetAmmo(int ammo)
        {
            int oldAmmo = CurrentAmmo;
            CurrentAmmo = Mathf.Clamp(ammo, 0, maxAmmo);

            if (oldAmmo != CurrentAmmo)
            {
                OnAmmoChanged?.Invoke(CurrentAmmo);
            }
        }

        public void Shoot()
        {
            // Check cooldown
            if (Time.time < _nextFireTime)
                return;

            if (!axe || !HasAmmo)
                return;

            // Use spawn point if available, otherwise use this transform
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;

            GameObject curAxe = Instantiate(axe, spawnPosition, Quaternion.identity);

            if (curAxe.TryGetComponent(out ProjectileAxe scAxe))
            {
                curAxe.layer = gameObject.layer;

                // Reduce ammo and notify listeners
                SetAmmo(CurrentAmmo - 1);

                float direction = transform.parent?.localScale.x ?? 1;
                scAxe.Shoot(direction);

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
        }

        public void Reload()
        {
            // Add one ammo but don't exceed max
            SetAmmo(CurrentAmmo + 1);
        }

        // Events for ammo changes
        public event Action<int> OnAmmoChanged;
    }
}
