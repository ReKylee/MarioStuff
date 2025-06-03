using System;

namespace Animation.Flow.Conditions
{
    /// <summary>
    ///     Attribute for marking condition types to be automatically registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConditionAttribute : Attribute
    {
        /// <summary>
        ///     Create a new condition attribute with the specified display name
        /// </summary>
        /// <param name="displayName">Display name for the condition in UI</param>
        public ConditionAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        /// <summary>
        ///     Display name for the condition in UI
        /// </summary>
        public string DisplayName { get; }
    }
}
