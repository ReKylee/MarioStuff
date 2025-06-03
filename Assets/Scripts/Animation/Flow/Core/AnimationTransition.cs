using Animation.Flow.Conditions;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Defines a transition between animation states
    /// </summary>
    public class AnimationTransition
    {
        /// <summary>
        ///     Root condition for this transition (usually a composite)
        /// </summary>
        private ICondition _rootCondition;

        /// <summary>
        ///     Create a new transition to the specified target state
        /// </summary>
        public AnimationTransition(string targetStateId)
        {
            TargetStateId = targetStateId;
            _rootCondition = new CompositeCondition(CompositeType.And);
        }

        /// <summary>
        ///     ID of the destination state for this transition
        /// </summary>
        public string TargetStateId { get; private set; }

        /// <summary>
        ///     Root condition for this transition
        /// </summary>
        public ICondition RootCondition => _rootCondition;

        /// <summary>
        ///     Sets the root condition for this transition
        /// </summary>
        public AnimationTransition SetRootCondition(ICondition condition)
        {
            _rootCondition = condition;
            return this;
        }

        /// <summary>
        ///     Add a condition to this transition
        /// </summary>
        public AnimationTransition AddCondition(ICondition condition)
        {
            // If we have a composite root condition, add to it
            if (_rootCondition is CompositeCondition composite)
            {
                composite.AddCondition(condition);
            }
            else
            {
                // If the root is not a composite, create a new And composite with the current root and new condition
                CompositeCondition newRoot = new(CompositeType.And);
                if (_rootCondition != null)
                {
                    newRoot.AddCondition(_rootCondition);
                }

                newRoot.AddCondition(condition);
                _rootCondition = newRoot;
            }

            return this; // For method chaining
        }

        /// <summary>
        ///     Create a new AND group and add it to this transition
        /// </summary>
        public CompositeCondition AddAndGroup()
        {
            CompositeCondition andGroup = new(CompositeType.And);
            AddCondition(andGroup);
            return andGroup;
        }

        /// <summary>
        ///     Create a new OR group and add it to this transition
        /// </summary>
        public CompositeCondition AddOrGroup()
        {
            CompositeCondition orGroup = new(CompositeType.Or);
            AddCondition(orGroup);
            return orGroup;
        }

        /// <summary>
        ///     Check if all conditions for this transition are satisfied
        /// </summary>
        public bool CanTransition(IAnimationContext context)
        {
            // If no root condition, transition is always valid
            if (_rootCondition == null)
                return true;

            // Evaluate the root condition
            return _rootCondition.Evaluate(context);
        }
    }

}
