namespace Interfaces
{
    public interface IPowerUpCollector
    {
        bool CanCollectPowerUps { get; }
        void ApplyPowerUp(IPowerUp powerUp);
    }
}
