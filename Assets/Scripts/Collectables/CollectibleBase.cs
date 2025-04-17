using Interfaces;
using UnityEngine;

namespace Collectables
{
    public abstract class CollectibleBase : MonoBehaviour, ICollectable
    {
        protected virtual void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                OnCollect(col.gameObject);
                gameObject.SetActive(false);
            }
        }
        public abstract void OnCollect(GameObject collector);
    }
}
