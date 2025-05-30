using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Base scriptable object for Kirby abilities
    /// </summary>
    [CreateAssetMenu(fileName = "NewKirbyAbility", menuName = "Kirby/Ability")]
    public class KirbyAbility : ScriptableObject, IKirbyAbility
    {
        [SerializeField] private string abilityName = "New Ability";

        public string AbilityName => abilityName;

        /// <summary>
        ///     Execute the ability's primary action
        /// </summary>
        public virtual void Execute(KirbyController kirbyController)
        {
            Debug.Log($"Executing ability: {abilityName}");
        }

        /// <summary>
        ///     Called when the ability is first acquired
        /// </summary>
        public virtual void OnAcquire(KirbyController kirbyController)
        {
            Debug.Log($"Acquired ability: {abilityName}");
        }

        /// <summary>
        ///     Called when the ability is removed
        /// </summary>
        public virtual void OnRemove(KirbyController kirbyController)
        {
            Debug.Log($"Removed ability: {abilityName}");
        }
    }
}
