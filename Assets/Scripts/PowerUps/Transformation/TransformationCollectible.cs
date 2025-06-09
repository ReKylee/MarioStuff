using Kirby.Abilities;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private CopyAbilityData abilityData;
        public override IPowerUp CreatePowerUp() => new TransformationPowerUp(abilityData);
    }
}
