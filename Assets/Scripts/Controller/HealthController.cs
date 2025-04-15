using System;
using Interfaces;
using UnityEngine;

namespace Controller
{
    public class HealthController : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maximumHp = 3;
        private void Awake()
        {
            CurrentHP = maximumHp;
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public int CurrentHP { get; private set; }

        public int MaxHP => maximumHp;

        public void Damage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
            if (CurrentHP == 0) OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(maximumHp, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }
        public void SetHP(int hp)
        {
            CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }
        public event Action<int, int> OnHealthChanged;


        public event Action OnDeath;
    }
}
