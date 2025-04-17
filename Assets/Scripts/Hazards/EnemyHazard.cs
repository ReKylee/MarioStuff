using Interfaces.Damage;
using UnityEngine;

namespace Hazards
{
    public class EnemyHazard : MonoBehaviour, IDamageDealer
    {

        [SerializeField] private int damageAmount = 1;
        public int GetDamageAmount()
        {
            return damageAmount;
        }
    }
}
