using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces.Resettable;
using Managers;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Controllers
{
    /// <summary>
    ///     Controller for managing multiple weapons following the MVC pattern
    /// </summary>
    public class WeaponController : MonoBehaviour, IResettable
    {

        [SerializeField] private List<WeaponMapping> weaponMappings = new();

        private readonly Dictionary<KeyCode, IWeapon> _weaponsByKey = new();

        private void Awake()
        {
            InitializeWeapons();
        }

        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }

        private void Update()
        {
            HandleWeaponInput();
        }

        private void OnDestroy()
        {
            ResetManager.Instance?.Unregister(this);
        }

        public void ResetState()
        {
            // Reset ammo on all ammo weapons
            foreach (IWeapon weapon in _weaponsByKey.Values)
            {
                if (weapon is IAmmoWeapon ammoWeapon)
                {
                    ammoWeapon.Reload();
                }
            }
        }

        private void InitializeWeapons()
        {
            _weaponsByKey.Clear();

            foreach (WeaponMapping mapping in weaponMappings)
            {
                if (mapping.weaponComponent is IWeapon weapon)
                {
                    _weaponsByKey[mapping.activationKey] = weapon;
                }
            }
        }

        private void HandleWeaponInput()
        {
            foreach (var kvp in _weaponsByKey.Where(kvp => Input.GetKeyDown(kvp.Key)))
            {
                ActivateWeapon(kvp.Value);
            }
        }

        private void ActivateWeapon(IWeapon weapon) => weapon.Shoot();

        [Serializable]
        public class WeaponMapping
        {
            public string weaponName;
            public KeyCode activationKey;
            public MonoBehaviour weaponComponent;
        }
    }
}
