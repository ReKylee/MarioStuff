using System;
using Interfaces.Damage;
using UnityEngine;

namespace Controller
{
    public class HealthController : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maximumHp = 3;
        private void Awake()
        {
            CurrentHp = maximumHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public int CurrentHp { get; private set; }

        public int MaxHp => maximumHp;

        public void Damage(int amount)
        {
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            if (CurrentHp == 0) OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            CurrentHp = Mathf.Min(maximumHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
        public void SetHp(int hp)
        {
            CurrentHp = Mathf.Clamp(hp, 0, MaxHp);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;
    }
}
