using Kirby.Abilities;
using Kirby.Core.Components;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationPowerUp : IPowerUp
    {
        private readonly CopyAbilityData _abilityData;
        public TransformationPowerUp(CopyAbilityData abilityData)
        {
            _abilityData = abilityData;
        }
        public void ApplyPowerUp(GameObject player)
        {
            player.GetComponent<KirbyController>().EquipAbility(_abilityData);
        }
    }
}
