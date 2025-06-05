using System;
using System.Collections.Generic;
using Animation.Flow.Conditions.Core;

namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Interface for animation context that provides parameter values
    /// </summary>
    public interface IAnimationContext
    {
        public IAnimator Animator { get; }
        /// <summary>
        ///     Check if a parameter exists in this context
        /// </summary>
        bool HasParameter(string parameterName);

        /// <summary>
        ///     Get a parameter value from this context
        /// </summary>
        T GetParameter<T>(string parameterName);

        /// <summary>
        ///     Set a parameter value in this context
        /// </summary>
        void SetParameter<T>(string parameterName, T value);

        /// <summary>
        ///     Get the parameter type
        /// </summary>
        Type GetParameterType(string name);

        /// <summary>
        ///     Get all parameter data for serialization or editor usage
        /// </summary>
        IEnumerable<ParameterData> GetAllParameters();
    }
}
