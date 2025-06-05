using System;
using UnityEngine;

namespace Animation.Flow.Parameters.ConcreteParameters
{
    /// <summary>
    ///     Boolean parameter implementation
    /// </summary>
    [Serializable]
    public class BoolParameter : FlowParameter<bool>
    {
        public BoolParameter() { }

        public BoolParameter(string name, bool defaultValue = false, string description = "") 
            : base(name, defaultValue, description)
        {
        }
    }
}
