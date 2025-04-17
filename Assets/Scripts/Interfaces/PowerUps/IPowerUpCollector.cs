namespace Interfaces.PowerUps
{
    public interface IPowerUpCollector
    {
        bool CanCollectPowerUps { get; }
        void ApplyPowerUp(IPowerUp powerUp);
    }
}
