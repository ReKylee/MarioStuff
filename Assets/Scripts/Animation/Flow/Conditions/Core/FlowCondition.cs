using System;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Conditions.Core
{
    /// <summary>
    ///     Base class for all flow conditions
    /// </summary>
    [Serializable]
    public abstract class FlowCondition : ICondition
    {
        [SerializeField] protected string _name;
        [SerializeField] protected bool _isNegated;

        protected FlowCondition() { }

        protected FlowCondition(string name, bool isNegated = false)
        {
            _name = name;
            _isNegated = isNegated;
        }

        /// <summary>
        ///     Gets the name of the condition
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     Gets or sets whether the condition result should be negated
        /// </summary>
        public bool IsNegated
        {
            get => _isNegated;
            set => _isNegated = value;
        }

        /// <summary>
        ///     Gets the condition type
        /// </summary>
        public abstract ConditionType ConditionType { get; }

        /// <summary>
        ///     Evaluates the condition in the given context
        /// </summary>
        public bool Evaluate(IAnimationContext context)
        {
            bool result = EvaluateInternal(context);
            return _isNegated ? !result : result;
        }

        /// <summary>
        ///     Internal evaluation logic to be implemented by derived classes
        /// </summary>
        protected abstract bool EvaluateInternal(IAnimationContext context);

        /// <summary>
        ///     Creates a clone of this condition
        /// </summary>
        public abstract FlowCondition Clone();
    }
}
