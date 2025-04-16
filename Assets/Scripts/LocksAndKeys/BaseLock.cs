using Interfaces;
using UnityEngine;

namespace LocksAndKeys
{
    public abstract class BaseLock : MonoBehaviour, ILock
    {

        [SerializeField] private string requiredKeyId;

        [SerializeField] private bool isUnlocked;
        public string RequiredKeyId => requiredKeyId;

        public bool TryUnlock(IKey key)
        {
            if (isUnlocked)
                return true;

            if (!IsCorrectKey(key))
                return false;

            isUnlocked = true;
            OnUnlocked();
            return true;

        }

        protected virtual bool IsCorrectKey(IKey key)
        {
            return key.KeyId == RequiredKeyId;
        }

        protected abstract void OnUnlocked();
    }

}
