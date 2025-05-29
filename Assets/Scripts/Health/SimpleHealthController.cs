using System;
using Interfaces.Damage;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Health
{
    public class SimpleHealthController : MonoBehaviour, IDamageable, IResettable
    {
        [SerializeField] private int maxHp = 3;
        private HealthModel _model;


        private void Awake()
        {
            _model = new HealthModel(maxHp, maxHp);
        }
        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }
        private void OnDestroy()
        {
            ResetManager.Instance?.Unregister(this);
        }

        public int MaxHp => _model.MaxHp;
        public int CurrentHp => _model.CurrentHp;

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

        public void Damage(int amount) => _model.Damage(amount);
        public void Heal(int amount) => _model.Heal(amount);
        public void SetHp(int hp) => _model.SetHp(hp);
        public void ResetState() => _model.SetHp(_model.MaxHp);
    }
}
