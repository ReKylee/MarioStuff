using System;

namespace Health.Lives
{
    public interface ILives
    {
        int Current { get; }
        int Max { get; }
        bool HasLives { get; }
        
        event Action<int, int> OnChanged;
        event Action OnLifeLost;
        event Action OnDead;
        
        void Lose();
        void Gain();
        void Reset();
    }
}
