using UnityEngine;

namespace Enemy
{
    public class EnemyWeaponHandler : MonoBehaviour
    {

        [SerializeField] private FireballWeapon fireballWeapon;
        [SerializeField] private float repeatRate;
        private void Start()
        {
            fireballWeapon.Equip();
            InvokeRepeating(nameof(Shoot), 0f, repeatRate);
        }

        private void Shoot()
        {
            fireballWeapon.Shoot();
        }
    }
}
