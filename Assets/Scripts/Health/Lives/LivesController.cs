using System;
using UnityEngine;

namespace Health.Lives
{
    public class LivesController : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        
        private ILives _lives;

        public ILives Lives => _lives;

        public event Action<int, int> OnLivesChanged;
        public event Action OnLifeLost;
        public event Action OnDead;

        private void Awake()
        {
            _lives = new Lives(maxLives);
            _lives.OnChanged += (current, max) => OnLivesChanged?.Invoke(current, max);
            _lives.OnLifeLost += () => OnLifeLost?.Invoke();
            _lives.OnDead += () => OnDead?.Invoke();
        }

        public void LoseLife() => _lives.Lose();
        public void GainLife() => _lives.Gain();
        public void ResetLives() => _lives.Reset();
    }
}
