using System;

namespace Health.Lives
{
    public class Lives : ILives
    {
        public int Current { get; private set; }
        public int Max { get; private set; }
        public bool HasLives => Current > 0;

        public event Action<int, int> OnChanged;
        public event Action OnLifeLost;
        public event Action OnDead;

        public Lives(int max = 3)
        {
            Max = max;
            Reset();
        }

        public void Lose()
        {
            if (Current <= 0) return;

            Current--;
            OnChanged?.Invoke(Current, Max);

            if (Current > 0)
            {
                OnLifeLost?.Invoke();
            }
            else
            {
                OnDead?.Invoke();
            }
        }

        public void Gain()
        {
            if (Current >= Max) return;

            Current++;
            OnChanged?.Invoke(Current, Max);
        }

        public void Reset()
        {
            Current = Max;
            OnChanged?.Invoke(Current, Max);
        }
    }
}
