using System;
using Interfaces.Damage;
using UnityEngine;

namespace Health.HP
{
    public class HitPointController : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;

        public int CurrentHp => Health.Current;
        public int MaxHp => Health.Max;

        public IHitPoints Health { get; private set; }

        public event Action<int, int> OnHealthChanged;
        public event Action OnOutOfHealth;

        private void Awake()
        {
            Health = new HitPoints(maxHealth);
            Health.OnChanged += (current, max) => OnHealthChanged?.Invoke(current, max);
            Health.OnEmpty += () => OnOutOfHealth?.Invoke();
        }

        public void Damage(int amount) => Health.Damage(amount);
        public void Heal(int amount) => Health.Heal(amount);
        public void SetHp(int hp) => Health.Set(hp);
        public void ResetHealth() => Health.Reset();
    }
}
