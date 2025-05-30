using System.Collections.Generic;
using UnityEngine;

namespace Kirby.Interfaces
{
    /// <summary>
    /// Interface for Kirby's transformations
    /// </summary>
    public interface IKirbyTransformation
    {
        /// <summary>
        /// Name of the transformation
        /// </summary>
        string TransformationName { get; }
        
        /// <summary>
        /// Gets the animation set used by this transformation
        /// </summary>
        AnimationSet AnimationSet { get; }
        
        /// <summary>
        /// Gets the list of abilities available in this transformation
        /// </summary>
        List<IKirbyAbility> Abilities { get; }
        
        /// <summary>
        /// Gets whether this transformation can fly
        /// </summary>
        bool CanFly { get; }
        
        /// <summary>
        /// Gets whether this transformation can inhale
        /// </summary>
        bool CanInhale { get; }
        
        /// <summary>
        /// Gets whether this transformation can crouch
        /// </summary>
        bool CanCrouch { get; }
        
        /// <summary>
        /// Called when Kirby transforms into this form
        /// </summary>
        /// <param name="kirbyController">Reference to the main Kirby controller</param>
        void OnTransform(KirbyController kirbyController);
        
        /// <summary>
        /// Called when Kirby leaves this form
        /// </summary>
        /// <param name="kirbyController">Reference to the main Kirby controller</param>
        void OnRevert(KirbyController kirbyController);
        
        /// <summary>
        /// Gets movement parameter overrides for this transformation
        /// </summary>
        MovementParameters MovementOverrides { get; }
    }
}
