using System;
using UnityEngine;

namespace Animation.Flow.Parameters.ConcreteParameters
{
    /// <summary>
    ///     String parameter implementation
    /// </summary>
    [Serializable]
    public class StringParameter : FlowParameter<string>
    {
        [SerializeField] private int _maxLength = 0; // 0 means no limit

        public StringParameter() { }

        public StringParameter(string name, string defaultValue = "", string description = "", int maxLength = 0) 
            : base(name, defaultValue, description)
        {
            _maxLength = maxLength;
        }

        /// <summary>
        ///     Gets the maximum length of the string (0 means no limit)
        /// </summary>
        public int MaxLength => _maxLength;

        /// <summary>
        ///     Validates parameter settings
        /// </summary>
        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (_maxLength > 0 && DefaultValue != null && DefaultValue.Length > _maxLength)
                return false;

            return true;
        }
    }
}
