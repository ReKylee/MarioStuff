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
        [SerializeField] private string _name;
        [SerializeField] private string _description;

        protected FlowParameter() { }

        protected FlowParameter(string name, string description = "")
        {
            _name = name;
            _description = description;
        }

        /// <summary>
        ///     Gets the name of the parameter
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     Gets the description of the parameter
        /// </summary>
        public string Description => _description;

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
        public virtual bool Validate()
        {
            return !string.IsNullOrEmpty(_name);
        }
    }
}
