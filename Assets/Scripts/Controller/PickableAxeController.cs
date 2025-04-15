using Player;
using UnityEngine;

namespace Controller
{
    public class PickableAxeController : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
            if (col.gameObject.CompareTag("Player"))
            {
                Debug.Log("Mario Collision! Fire Flower");
                gameObject.SetActive(false);
                col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new PickableAxePowerUp());

            }
        }
    }
}
