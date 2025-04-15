using Interfaces;
using Player;
using UnityEngine;

namespace Controller
{
    public abstract class PowerUpController : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("OnTriggerEnter2D " + col.gameObject.name);
            if (col.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player Collision! Applying power-up.");
                gameObject.SetActive(false);
                col.GetComponent<PlayerPowerUp>().CollectPowerUp(CreatePowerUp());
            }
        }
        protected abstract IPowerUp CreatePowerUp();
    }
}
