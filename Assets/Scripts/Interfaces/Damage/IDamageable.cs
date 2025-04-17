using System;

namespace Interfaces
{
    public interface IDamageable
    {
        int CurrentHP { get; }
        int MaxHP { get; }

        void Damage(int amount);
        void Heal(int amount);
        void SetHP(int hp);
        event Action<int, int> OnHealthChanged;
    }

}
