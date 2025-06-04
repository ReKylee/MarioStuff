using System.Collections.Generic;
using Animation.Flow.Interfaces;
using Animation.Flow.States;

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
                    // Verify state exists before returning it
                    if (StateRegistry.StateExists(transition.TargetStateId))
                    {
                        return transition.TargetStateId;
                    }
                }
            }

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

        /// <summary>
        ///     Create and add a transition to a strongly-typed state
        /// </summary>
        public AnimationTransition TransitionTo<T>(string targetStateId) where T : class, IAnimationState
        {
            // Get the state type
            AnimationStateType stateType = AnimationStateType.OneTime;

            // Try to determine the actual type based on registered types
            if (typeof(T) == typeof(LoopingState))
            {
                stateType = AnimationStateType.Looping;
            }
            else if (typeof(T) == typeof(HoldFrameState))
            {
                stateType = AnimationStateType.HoldFrame;
            }
            else if (typeof(T) == typeof(OneTimeState))
            {
                stateType = AnimationStateType.OneTime;
            }

            AnimationTransition transition = new(targetStateId, stateType);
            _transitions.Add(transition);
            return transition;
        }
    }
}
