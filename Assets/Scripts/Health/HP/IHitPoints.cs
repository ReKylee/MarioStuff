using System;

namespace Health.HP
{
    public interface IHitPoints
    {
        int Current { get; }
        int Max { get; }
        bool IsAlive { get; }
        
        event Action<int, int> OnChanged;
        event Action OnEmpty;
        
        void Damage(int amount);
        void Heal(int amount);
        void Reset();
        void Set(int value);
    }
}
