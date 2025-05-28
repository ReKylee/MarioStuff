using Controller;
using Interfaces.Locks;
using Interfaces.Resettable;
using Resettables;
using UnityEngine;
using UnityEngine.Events;

namespace LocksAndKeys
{
    public class LockedDoor : BaseLock, IResettable
    {
        [SerializeField] private UnityEvent doorOpened;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                KeychainController keychain = col.GetComponent<KeychainController>();
                if (keychain == null) return;

                IKey usedKey = keychain.Keychain.TryUnlock(this);
                if (usedKey == null)
                {
                    Debug.Log($"No key was found for {gameObject.name} door!");
                    return;
                }

                keychain.Keychain.RemoveKey(usedKey);
            }

        }

        protected override void OnUnlocked()
        {
            Debug.Log($"Unlocked: {gameObject.name}");
            doorOpened?.Invoke();
        }
        public void ResetState() => SetUnlocked(false);
    }
}
