using System.Collections.Generic;
using System.Linq;
using Interfaces.Locks;

namespace Managers
{
    public class Keychain
    {
        private readonly HashSet<IKey> _keychain = new();
        public void PickUpKey(IKey key)
        {
            _keychain.Add(key);
        }
        public void RemoveKey(IKey key)
        {
            _keychain.Remove(key);
        }

        public IKey TryUnlock(ILock lockComponent)
        {
            return _keychain.FirstOrDefault(lockComponent.TryUnlock);
        }
        public void Reset()
        {
            _keychain.Clear();
        }
    }
}
