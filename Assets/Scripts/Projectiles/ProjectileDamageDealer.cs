using Interfaces.Damage;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileDamageDealer : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damage = 1;
        public int GetDamageAmount()
        {
            return damage;
        }
    }
}
