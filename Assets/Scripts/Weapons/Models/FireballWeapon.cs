using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class FireballWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private GameObject fireball;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cooldownTime = 0.3f;

        private bool _isEquipped;
        private float _nextFireTime;

        public void Shoot()
        {
            // Check cooldown
            if (Time.time < _nextFireTime)
                return;

            if (!fireball || !_isEquipped)
                return;

            // Use spawn point if available, otherwise use this transform
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;

            GameObject curFireball = Instantiate(fireball, spawnPosition, Quaternion.identity);

            if (curFireball.TryGetComponent(out ProjectileFireball scFireball))
            {
                curFireball.layer = gameObject.layer;
                float direction = transform.parent?.localScale.x ?? 1;

                scFireball.Shoot(direction);

                // Set cooldown
                _nextFireTime = Time.time + cooldownTime;
            }
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
