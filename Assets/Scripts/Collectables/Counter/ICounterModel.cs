using System;

namespace Collectables.Base
{
    public interface ICounterModel
    {
        int Count { get; }
        event Action<int> OnCountChanged;
        void Increment();
        void Reset();
    }
}
