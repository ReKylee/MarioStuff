using System;

namespace Interfaces.Damage
{
    public interface IDamageable
    {
        int CurrentHp { get; }
        int MaxHp { get; }

        void Damage(int amount);
        void Heal(int amount);
        void SetHp(int hp);
        event Action<int, int> OnHealthChanged;
        event Action OnEmpty;
    }

}
