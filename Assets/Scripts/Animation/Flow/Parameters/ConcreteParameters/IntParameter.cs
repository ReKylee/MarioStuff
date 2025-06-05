using System;
using UnityEngine;

namespace Animation.Flow.Parameters.ConcreteParameters
{
    /// <summary>
    ///     Integer parameter implementation
    /// </summary>
    [Serializable]
    public class IntParameter : FlowParameter<int>
    {
        [SerializeField] private int _minValue = int.MinValue;
        [SerializeField] private int _maxValue = int.MaxValue;

        public IntParameter() { }

        public IntParameter(string name, int defaultValue = 0, string description = "",
            int minValue = int.MinValue, int maxValue = int.MaxValue) 
            : base(name, defaultValue, description)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        /// <summary>
        ///     Gets the minimum allowed value
        /// </summary>
        public int MinValue => _minValue;

        /// <summary>
        ///     Gets the maximum allowed value
        /// </summary>
        public int MaxValue => _maxValue;

        /// <summary>
        ///     Validates parameter settings
        /// </summary>
        public override bool Validate()
        {
            return base.Validate() && _minValue <= _maxValue;
        }
    }
}
