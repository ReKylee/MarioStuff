using System;
using UnityEngine;

namespace Animation.Flow.Parameters
{
    /// <summary>
    ///     Generic implementation of FlowParameter for strongly-typed values
    /// </summary>
    [Serializable]
    public class FlowParameter<T> : FlowParameter
    {
        [SerializeField] private T defaultValue;

        public FlowParameter()
        {
        }

        public FlowParameter(string name, T defaultValue = default, string description = "")
            : base(name, description)
        {
            this.defaultValue = defaultValue;
        }

        /// <summary>
        ///     Gets the strongly-typed default value
        /// </summary>
        public T DefaultValue => defaultValue;

        /// <summary>
        ///     Gets the parameter type
        /// </summary>
        public override Type ParameterType => typeof(T);

        /// <summary>
        ///     Gets the default value as an object
        /// </summary>
        public override object GetDefaultValue() => defaultValue;

        /// <summary>
        ///     Creates a clone of this parameter
        /// </summary>
        public override FlowParameter Clone() => new FlowParameter<T>(Name, defaultValue, Description);
    }
}
