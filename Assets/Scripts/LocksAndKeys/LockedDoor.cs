using Controller;
using Interfaces;
using Resettables;
using UnityEngine;
using UnityEngine.Events;

namespace LocksAndKeys
{
    public class LockedDoor : BaseLock
    {
        [SerializeField] private UnityEvent doorOpened;
        private LockResetter _lockResetter;
        private void Start()
        {
            _lockResetter = new LockResetter(this);
        }
        private void OnTriggerEnter2D(Collider2D col)

        {
            if (col.CompareTag("Player"))
            {
                KeychainController keychain = col.GetComponent<KeychainController>();
                if (keychain == null) return;

                IKey usedKey = keychain.KeysManager.TryUnlock(this);
                if (usedKey == null)
                {
                    Debug.Log($"No key was found for {gameObject.name} door!");
                    return;
                }

                keychain.KeysManager.RemoveKey(usedKey);
            }

        }

        protected override void OnUnlocked()
        {
            Debug.Log($"Unlocked: {gameObject.name}");
            doorOpened?.Invoke();
        }
    }
}
