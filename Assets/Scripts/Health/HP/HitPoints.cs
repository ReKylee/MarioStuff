using System;

namespace Health.HP
{
    public class HitPoints : IHitPoints
    {
        public int Current { get; private set; }
        public int Max { get; private set; }
        public bool IsAlive => Current > 0;

        public event Action<int, int> OnChanged;
        public event Action OnEmpty;

        public HitPoints(int max = 3)
        {
            Max = max;
            Reset();
        }

        public void Damage(int amount)
        {
            if (amount <= 0) return;

            Current = Math.Max(0, Current - amount);
            OnChanged?.Invoke(Current, Max);

            if (Current == 0)
            {
                OnEmpty?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            Current = Math.Min(Max, Current + amount);
            OnChanged?.Invoke(Current, Max);
        }

        public void Reset()
        {
            Current = Max;
            OnChanged?.Invoke(Current, Max);
        }

        public void Set(int value)
        {
            Current = Math.Clamp(value, 0, Max);
            OnChanged?.Invoke(Current, Max);
        }
    }
}
