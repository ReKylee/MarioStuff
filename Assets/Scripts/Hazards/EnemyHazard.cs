using Interfaces;
using UnityEngine;

namespace Hazards
{
    public class EnemyHazard : MonoBehaviour, IDamageDealer
    {

        public int GetDamageAmount()
        {
            return 1;
        }
    }
}
