using System;
using Interfaces.Damage;

namespace Health
{
    public class HealthModel : IDamageable
    {
        public HealthModel(int currentHp, int maxHp)
        {
            CurrentHp = currentHp;
            MaxHp = maxHp;
        }

        public int CurrentHp { get; private set; }
        public int MaxHp { get; }
        public event Action<int, int> OnHealthChanged;
        public event Action OnEmpty;
        public void SetHp(int hp)
        {
            CurrentHp = Math.Max(0, hp);
        }
        public void Heal(int amount)
        {
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
        public void Damage(int amount)
        {
            if (CurrentHp <= 0)
            {
                OnEmpty?.Invoke();
                return;
            }

            CurrentHp = Math.Max(CurrentHp - amount, 0);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
