using System;
using UnityEngine;

namespace Collectables
{
    public class CoinCollectable : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D col)

        {
            Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
            if (col.gameObject.CompareTag("Player"))
            {
                Debug.Log("Mario Collision!");
                OnCoinCollected?.Invoke();

                gameObject.SetActive(false);
            }
        }
        public static event Action OnCoinCollected;
    }
}
