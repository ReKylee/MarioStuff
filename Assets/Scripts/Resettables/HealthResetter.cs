using Interfaces;
using Managers;

namespace Resettables
{
    public class HealthResetter : IResettable
    {
        private readonly IDamageable _damageable;
        private readonly int _initialHp;

        public HealthResetter(IDamageable damageable)
        {
            _damageable = damageable;
            _initialHp = damageable.MaxHP;
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            _damageable.SetHP(_initialHp);
        }
    }
}
