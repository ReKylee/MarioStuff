using Interfaces.Damage;
using Interfaces.Resettable;
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
            _initialHp = damageable.MaxHp;
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            _damageable.SetHp(_initialHp);
        }
    }
}
