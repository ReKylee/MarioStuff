using System;
using UnityEngine;

namespace Animation.Flow
{
    /// <summary>
    ///     Context interface that provides access to the animator and relevant data for state transitions
    /// </summary>
    public interface IAnimationContext
    {
        /// <summary>
        ///     Access to the animator component
        /// </summary>
        IAnimator Animator { get; }

        /// <summary>
        ///     The entity/object being animated
        /// </summary>
        GameObject Entity { get; }

        /// <summary>
        ///     Get a parameter value from the context
        /// </summary>
        T GetParameter<T>(string name);

        /// <summary>
        ///     Set a parameter value in the context
        /// </summary>
        void SetParameter<T>(string name, T value);

        /// <summary>
        ///     Check if a parameter exists
        /// </summary>
        bool HasParameter(string name);

        /// <summary>
        ///     Get the type of a parameter
        /// </summary>
        Type GetParameterType(string name);
    }
}
