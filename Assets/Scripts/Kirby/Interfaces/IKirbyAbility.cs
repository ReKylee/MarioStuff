using System.Collections.Generic;
using UnityEngine;

namespace Kirby.Interfaces
{
    /// <summary>
    /// Interface for Kirby's abilities
    /// </summary>
    public interface IKirbyAbility
    {
        /// <summary>
        /// Name of the ability
        /// </summary>
        string AbilityName { get; }
        
        /// <summary>
        /// Execute the ability's primary action
        /// </summary>
        /// <param name="kirbyController">Reference to the main Kirby controller</param>
        void Execute(KirbyController kirbyController);
        
        /// <summary>
        /// Called when the ability is first acquired
        /// </summary>
        /// <param name="kirbyController">Reference to the main Kirby controller</param>
        void OnAcquire(KirbyController kirbyController);
        
        /// <summary>
        /// Called when the ability is removed
        /// </summary>
        /// <param name="kirbyController">Reference to the main Kirby controller</param>
        void OnRemove(KirbyController kirbyController);
    }
}
