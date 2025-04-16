using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace Managers
{
    public class KeysManager
    {
        private readonly List<IKey> _keychain = new();
        public void PickUpKey(IKey key)
        {
            if (!_keychain.Contains(key)) _keychain.Add(key);
        }

        public bool TryUnlock(ILock lockComponent)
        {
            return _keychain.Any(lockComponent.TryUnlock);
        }
    }
}
