using System;
using System.Collections.Generic;
using System.Linq;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     The base animation state implementation that handles transitions
    /// </summary>
    public abstract class FlowState : IAnimationState
    {

        /// <summary>
        ///     List of possible transitions from this state
        /// </summary>
        private readonly List<FlowTransition> _transitions = new();

        protected FlowState(string id, string animationName)
        {
            Id = id;
            AnimationName = animationName;
        }

        /// <summary>
        ///     Unique identifier for this state
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Animation name to play
        /// </summary>
        public virtual string AnimationName { get; protected set; }

        /// <summary>
        ///     Whether this animation should loop
        /// </summary>
        public virtual bool ShouldLoop { get; protected set; } = true;

        /// <summary>
        ///     Called when entering this state
        /// </summary>
        public virtual void OnEnter(IAnimationContext context)
        {
            // Default implementation plays the animation
            if (context.Animator != null && !string.IsNullOrEmpty(AnimationName))
            {
                context.Animator.Play(AnimationName);
                context.Animator.SetLooping(ShouldLoop);
            }
        }

        /// <summary>
        ///     Called when exiting this state
        /// </summary>
        public virtual void OnExit(IAnimationContext context)
        {
            // Base implementation does nothing
        }

        /// <summary>
        ///     Called each frame while this state is active
        /// </summary>
        public virtual void OnUpdate(IAnimationContext context, float deltaTime)
        {
            // Base implementation does nothing
        }

        /// <summary>
        ///     Check for valid transitions and return the next state if a transition should occur
        /// </summary>
        public virtual string CheckTransitions(IAnimationContext context) =>
            // Check all transitions in order
            _transitions.Where(transition => transition.CanTransition(context))
                .Select(transition => transition.toStateId)
                .FirstOrDefault();
        public void AddTransition(FlowTransition transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException(nameof(transition), "Transition cannot be null");
            }

            _transitions.Add(transition);
        }
        public void Validate()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new ArgumentException("State ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(AnimationName))
            {
                throw new ArgumentException("Animation name cannot be null or empty");
            }

            foreach (FlowTransition transition in _transitions)
            {
                transition.Validate();
            }
        }
    }
}
