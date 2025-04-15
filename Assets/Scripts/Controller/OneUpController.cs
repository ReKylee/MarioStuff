using Interfaces;
using Player;
using UnityEngine;

namespace Controller
{
    public class OneUpController : MonoBehaviour
    {
        [SerializeField] private int healAmount = 1;
        private void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
            if (col.gameObject.CompareTag("Player"))
            {
                Debug.Log("Mario Collision! One Up Shroom!");
                gameObject.SetActive(false);
                col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new OneUpPowerUp(healAmount));
            }
        }
    }
}
