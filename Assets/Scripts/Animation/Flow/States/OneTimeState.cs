using Animation.Flow.Core;
using Animation.Flow.Interfaces;

namespace Animation.Flow.States
{
    /// <summary>
    ///     State that plays an animation once and then waits
    /// </summary>
    public class OneTimeState : FlowState
    {
        /// <summary>
        ///     Create a new one-time animation state
        /// </summary>
        /// <param name="id">Unique identifier for this state</param>
        /// <param name="animationName">Name of the animation to play</param>
        public OneTimeState(string id, string animationName) : base(id, animationName)
        {
            ShouldLoop = false;
        }

        public override void OnEnter(IAnimationContext context)
        {
            // Play the animation without looping
            base.OnEnter(context);

            // Reset the animation complete flag if it exists
            context?.SetParameter("AnimationComplete", false);
        }

        public override void OnUpdate(IAnimationContext context, float deltaTime)
        {
            base.OnUpdate(context, deltaTime);

            // Check if animation has completed and update parameter
            if (context?.Animator != null && context.Animator.IsAnimationComplete)
            {
                context.SetParameter("AnimationComplete", true);
            }
        }
    }
}
