using Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerPowerUp : MonoBehaviour
    {
        public void CollectPowerUp(IPowerUp powerUp)
        {
            powerUp.ApplyPowerUp(gameObject);
        }
    }
}
