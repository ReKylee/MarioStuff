using Projectiles.Core;
using Projectiles.OverEngineeredLaser;
using Resettables;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class LaserWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private float cooldownTime = 0.3f;
        private bool _isEquipped;
        private LaserFactory _laserFactory;
        private float _nextFireTime;

        private UsableWeaponResetter _resetter;
        private void Awake()
        {
            _laserFactory = new LaserFactory();
        }
        private void Start()
        {
            _resetter = new UsableWeaponResetter(this);
        }

        public void Shoot()
        {
            if (Time.time < _nextFireTime || !_isEquipped)
                return;

            GameObject laserInstance = _laserFactory.Create();

            Vector3 spawnPosition = transform.position + Vector3.up;
            laserInstance.transform.position = spawnPosition;
            laserInstance.transform.rotation = Quaternion.identity;

            if (laserInstance.TryGetComponent(out BaseProjectile projectile))
            {
                projectile.Fire();
            }
            else
            {
                Debug.LogError("The object provided by the factory does not have a BaseProjectile component!");
            }

            _nextFireTime = Time.time + cooldownTime;
        }

        public void Equip()
        {
            _isEquipped = true;
        }

        public void UnEquip()
        {
            _isEquipped = false;
        }
    }

}
