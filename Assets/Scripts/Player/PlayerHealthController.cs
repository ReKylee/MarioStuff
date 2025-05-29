using System;
using Interfaces.Damage;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerHealthController : MonoBehaviour, IDamageable, IResettable
    {
        [SerializeField] private int maxHp = 3;
        [SerializeField] private int maxLives = 3;

        private PlayerLivesModel _model;

        private void Awake()
        {
            _model = new PlayerLivesModel(maxLives, maxHp);
        }
        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }

        public event Action<int, int> OnHealthChanged
        {
            add => _model.OnHealthChanged += value;
            remove => _model.OnHealthChanged -= value;
        }

        public event Action OnEmpty
        {
            add => _model.OnEmpty += value;
            remove => _model.OnEmpty -= value;
        }

        public int MaxHp => _model.MaxHp;
        public int CurrentHp => _model.CurrentHp;

        public void Damage(int amount) => _model.Damage(amount);
        public void Heal(int amount) => _model.Heal(amount);
        public void SetHp(int hp) => _model.SetHp(hp);
        public void ResetState() => _model.Reset();

        public event Action<int, int> OnLivesChanged
        {
            add => _model.OnLivesChanged += value;
            remove => _model.OnLivesChanged -= value;
        }
    }
}
