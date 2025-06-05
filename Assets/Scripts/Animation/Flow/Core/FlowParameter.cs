using System;
using UnityEngine;

namespace Animation.Flow.Parameters
{
    /// <summary>
    ///     Base class for all flow parameters that can be used in animation conditions
    /// </summary>
    [Serializable]
    public abstract class FlowParameter
    {
        [SerializeField] private string name;
        [SerializeField] private string description;

        protected FlowParameter()
        {
        }

        protected FlowParameter(string name, string description = "")
        {
            this.name = name;
            this.description = description;
        }

        /// <summary>
        ///     Gets the name of the parameter
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        ///     Gets the description of the parameter
        /// </summary>
        public string Description => description;

        /// <summary>
        ///     Gets the parameter type
        /// </summary>
        public abstract Type ParameterType { get; }

        /// <summary>
        ///     Gets the default value as an object
        /// </summary>
        public abstract object GetDefaultValue();

        /// <summary>
        ///     Creates a clone of this parameter
        /// </summary>
        public abstract FlowParameter Clone();

        /// <summary>
        ///     Validates parameter settings
        /// </summary>
        public virtual bool Validate() => !string.IsNullOrEmpty(name);
    }
}
