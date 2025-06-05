using System;
using System.Collections.Generic;
using Animation.Flow.Parameters;
using UnityEngine;

namespace Animation.Flow.Interfaces
{
    /// <summary>
    ///     Interface for animation context that provides parameter values
    /// </summary>
    public interface IAnimationContext
    {
        /// <summary>
        ///     Gets the animator instance
        /// </summary>
        IAnimator Animator { get; }

        /// <summary>
        ///     Gets the entity this context is for
        /// </summary>
        GameObject Entity { get; }

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

    
    }
}
