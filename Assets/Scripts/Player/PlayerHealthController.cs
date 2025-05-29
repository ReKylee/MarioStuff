using System;
using Health;
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
        [SerializeField] private SliderHealthView healthView;
        [SerializeField] private TextHealthView livesView;

        private PlayerLivesModel _model;

        private void Awake()
        {
            _model = new PlayerLivesModel(maxLives, maxHp);

            if (healthView) _model.OnHealthChanged += healthView.UpdateDisplay;
            if (livesView) _model.OnLivesChanged += livesView.UpdateDisplay;

            if (healthView) healthView.UpdateDisplay(_model.CurrentHp, _model.MaxHp);
            if (livesView) livesView.UpdateDisplay(_model.CurrentLives, _model.MaxLives);
        }
        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }
        private void OnDestroy()
        {
            _model.Dispose();
            ResetManager.Instance?.Unregister(this);
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

        public event Action<int, int> OnHealthChanged
        {
            add => _model.OnHealthChanged += value;
            remove => _model.OnHealthChanged -= value;
        }

        public void ResetState() => _model.Reset();
    }
}
