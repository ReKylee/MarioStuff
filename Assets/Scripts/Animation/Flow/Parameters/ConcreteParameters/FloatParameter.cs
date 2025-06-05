using System;
using UnityEngine;

namespace Animation.Flow.Parameters.ConcreteParameters
{
    /// <summary>
    ///     Float parameter implementation
    /// </summary>
    [Serializable]
    public class FloatParameter : FlowParameter<float>
    {
        [SerializeField] private float _minValue = float.MinValue;
        [SerializeField] private float _maxValue = float.MaxValue;

        public FloatParameter() { }

        public FloatParameter(string name, float defaultValue = 0f, string description = "",
            float minValue = float.MinValue, float maxValue = float.MaxValue) 
            : base(name, defaultValue, description)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        /// <summary>
        ///     Gets the minimum allowed value
        /// </summary>
        public float MinValue => _minValue;

        /// <summary>
        ///     Gets the maximum allowed value
        /// </summary>
        public float MaxValue => _maxValue;

        /// <summary>
        ///     Validates parameter settings
        /// </summary>
        public override bool Validate()
        {
            return base.Validate() && _minValue <= _maxValue;
        }
    }
}
