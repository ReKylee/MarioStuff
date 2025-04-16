using Interfaces;
using Managers;

namespace Resettables
{
    public class LockResetter : IResettable
    {
        private readonly bool _initialUnlocked;
        private readonly ILock _lock;
        public LockResetter(ILock mylock)
        {
            _lock = mylock;
            _initialUnlocked = _lock.IsUnlocked;
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            _lock.SetUnlocked(_initialUnlocked);
        }
    }
}
