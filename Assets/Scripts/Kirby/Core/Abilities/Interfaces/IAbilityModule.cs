namespace Kirby.Abilities
{
    /// <summary>
    ///     Base interface for all ability components
    /// </summary>
    public interface IAbilityModule // Renamed from IAbility
    {
        /// <summary>
        ///     Unique identifier for this ability component
        /// </summary>
        string AbilityID { get; } // Property name can remain for now, or be ModuleID

        /// <summary>
        ///     Human-readable name for this ability component
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     Initialize the ability module with the controller reference
        /// </summary>
        void Initialize(KirbyController controller);

        /// <summary>
        ///     Called when the ability module becomes active
        /// </summary>
        void OnActivate();

        /// <summary>
        ///     Called when the ability module becomes inactive
        /// </summary>
        void OnDeactivate();

        /// <summary>
        ///     Update logic for the ability module
        /// </summary>
        void ProcessAbility(); // Method name can remain, or be ProcessModuleLogic etc.
    }
}
