using Controller;
using UnityEngine;
using UnityEngine.Events;

namespace LocksAndKeys
{
    public class LockedDoor : BaseLock
    {
        [SerializeField] private UnityEvent doorOpened;
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                KeychainController keychain = col.GetComponent<KeychainController>();
                if (keychain == null) return;

                bool success = keychain.KeysManager.TryUnlock(this);
                if (!success)
                {
                    Debug.Log("Player doesn’t have the correct key.");
                }
            }

        }

        protected override void OnUnlocked()
        {
            Debug.Log($"Unlocked: {gameObject.name}");
            doorOpened?.Invoke();
        }
    }
}
