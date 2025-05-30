using System.Collections.Generic;
using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    /// Scriptable Object for defining a Kirby transformation
    /// </summary>
    [CreateAssetMenu(fileName = "NewKirbyTransformation", menuName = "Kirby/Transformation")]
    public class KirbyTransformation : ScriptableObject, IKirbyTransformation
    {
        [Header("Transformation Properties")]
        [SerializeField] private string transformationName = "New Transformation";
        [SerializeField] private AnimationSet animationSet;
        [SerializeField] private MovementParameters movementOverrides;
        
        [Header("Capabilities")]
        [SerializeField] private bool canFly = true;
        [SerializeField] private bool canInhale = true;
        [SerializeField] private bool canCrouch = true;
        
        [Header("Abilities")]
        [SerializeField] private List<ScriptableObject> abilityObjects = new List<ScriptableObject>();
        
        // Runtime list of ability instances
        private List<IKirbyAbility> abilities = new List<IKirbyAbility>();
        
        // IKirbyTransformation implementation
        public string TransformationName => transformationName;
        public AnimationSet AnimationSet => animationSet;
        public List<IKirbyAbility> Abilities => abilities;
        public bool CanFly => canFly;
        public bool CanInhale => canInhale;
        public bool CanCrouch => canCrouch;
        public MovementParameters MovementOverrides => movementOverrides;
        
        /// <summary>
        /// Called when Kirby transforms into this form
        /// </summary>
        public void OnTransform(KirbyController kirbyController)
        {
            // Initialize abilities
            abilities.Clear();
            foreach (var abilityObject in abilityObjects)
            {
                if (abilityObject is IKirbyAbility ability)
                {
                    abilities.Add(ability);
                    ability.OnAcquire(kirbyController);
                }
            }
            
            Debug.Log($"Transformed into {transformationName}");
        }
        
        /// <summary>
        /// Called when Kirby leaves this form
        /// </summary>
        public void OnRevert(KirbyController kirbyController)
        {
            // Clean up abilities
            foreach (var ability in abilities)
            {
                ability.OnRemove(kirbyController);
            }
            abilities.Clear();
            
            Debug.Log($"Reverting from {transformationName}");
        }
    }
}
