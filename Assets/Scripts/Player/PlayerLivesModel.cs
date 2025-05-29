using System;
using Health;
using Interfaces.Damage;

namespace Player
{
    public class PlayerLivesModel : IDamageable
    {

        private readonly HealthModel _healthModel;

        public PlayerLivesModel(int maxLives, int hpPerLife)
        {
            MaxLives = maxLives;
            CurrentLives = maxLives;
            MaxHp = hpPerLife;
            _healthModel = new HealthModel(hpPerLife, hpPerLife);
            _healthModel.OnHealthChanged += (hp, maxHp) => OnHealthChanged?.Invoke(hp, maxHp);
            _healthModel.OnEmpty += LoseLife;
        }
        private int MaxLives { get; }
        private int CurrentLives { get; set; }
        public int MaxHp { get; }
        public int CurrentHp => _healthModel.CurrentHp;

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
        public event Action<int, int> OnLivesChanged;

        private void LoseLife()
        {
            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
            if (CurrentLives > 0)
            {
                _healthModel.SetHp(_healthModel.MaxHp);
                OnHealthChanged?.Invoke(_healthModel.CurrentHp, MaxHp);
                return;
            }

            OnEmpty?.Invoke();
        }

        public void Reset()
        {
            CurrentLives = MaxLives;
            _healthModel.SetHp(_healthModel.MaxHp);
            OnHealthChanged?.Invoke(_healthModel.CurrentHp, MaxHp);
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        }
    }
}
