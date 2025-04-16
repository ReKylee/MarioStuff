namespace Interfaces
{
    public interface ILock
    {
        public string RequiredKeyId { get; }
        bool TryUnlock(IKey key);
    }
}
