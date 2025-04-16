using System;
using UnityEngine;

namespace Collectables
{
    public class CoinCollectable : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D col)

        {
            if (col.gameObject.CompareTag("Player"))
            {
                OnCoinCollected?.Invoke();

                gameObject.SetActive(false);
            }
        }
        public static event Action OnCoinCollected;
    }
}
