using System;
using Health.Interfaces;
using Health.Models;
using Player.Interfaces;

namespace Player.Models
{
    public class PlayerLivesModel : IFullHealthSystem, ILivesSystem, IDisposable
    {
        private readonly IFullHealthSystem _healthModel;

        public PlayerLivesModel(int maxLives, int hpPerLife)
        {
            MaxLives = maxLives;
            CurrentLives = maxLives;
            _healthModel = new HealthModel(hpPerLife, hpPerLife);
            _healthModel.OnHealthChanged += HealthChanged;
            _healthModel.OnEmpty += LoseLife;
        }

        public void Dispose()
        {
            _healthModel.OnHealthChanged -= HealthChanged;
            _healthModel.OnEmpty -= LoseLife;
        }
        public int CurrentHp => _healthModel.CurrentHp;
        public int MaxHp => _healthModel.MaxHp;

        public event Action<int, int> OnHealthChanged;
        public event Action OnEmpty;

        public void Damage(int amount)
        {
            _healthModel.Damage(amount);
        }

        public void Heal(int amount)
        {
            _healthModel.Heal(amount);
        }

        public void SetHp(int hp)
        {
            _healthModel.SetHp(hp);
        }

        public int CurrentLives { get; private set; }
        public int MaxLives { get; }
        public event Action<int, int> OnLivesChanged;

        public void Reset()
        {
            CurrentLives = MaxLives;
            _healthModel.SetHp(MaxHp);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        }

        private void HealthChanged(int hp, int maxHp)
        {
            OnHealthChanged?.Invoke(hp, maxHp);
        }

        private void LoseLife()
        {
            CurrentLives = Math.Max(0, CurrentLives - 1);
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);

            if (CurrentLives > 0)
            {
                _healthModel.SetHp(MaxHp);
                OnHealthChanged?.Invoke(CurrentHp, MaxHp);
                return;
            }

            OnEmpty?.Invoke();
        }
    }
}
