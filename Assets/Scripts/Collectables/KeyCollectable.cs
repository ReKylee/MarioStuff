using System;
using Interfaces;
using UnityEngine;

namespace Collectables
{
    public class KeyCollectable : MonoBehaviour
    {
        private IKey _myKey;
        private void Awake()
        {
            _myKey = GetComponent<IKey>();
        }
        private void OnTriggerEnter2D(Collider2D col)

        {
            if (col.gameObject.CompareTag("Player"))
            {
                OnKeyCollected?.Invoke(_myKey);

                gameObject.SetActive(false);
            }
        }
        public static event Action<IKey> OnKeyCollected;
    }
}
