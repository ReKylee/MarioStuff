using System.Collections.Generic;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     The base animation state implementation that handles transitions
    /// </summary>
    public abstract class AnimationStateBase : IAnimationState
    {

        /// <summary>
        ///     List of possible transitions from this state
        /// </summary>
        private readonly List<AnimationTransition> _transitions = new();

        protected AnimationStateBase(string id, string animationName)
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
        public virtual string CheckTransitions(IAnimationContext context)
        {
            // Check all transitions in order
            foreach (AnimationTransition transition in _transitions)
            {
                if (transition.CanTransition(context))
                {
                    return transition.TargetStateId;
                }
            }

            // No valid transitions
            return null;
        }

        /// <summary>
        ///     Add a transition to this state
        /// </summary>
        public AnimationStateBase AddTransition(AnimationTransition transition)
        {
            _transitions.Add(transition);
            return this; // For method chaining
        }

        /// <summary>
        ///     Create and add a transition to another state
        /// </summary>
        public AnimationTransition TransitionTo(string targetStateId)
        {
            AnimationTransition transition = new(targetStateId);
            _transitions.Add(transition);
            return transition; // Return for adding conditions
        }
    }
}
