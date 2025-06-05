using Animation.Flow.Core;
using Animation.Flow.Interfaces;

namespace Animation.Flow.States
{
    /// <summary>
    ///     State that holds on a specific frame of an animation
    /// </summary>
    public class HoldFrameState : FlowState
    {

        /// <summary>
        ///     Create a new hold frame animation state
        /// </summary>
        /// <param name="id">Unique identifier for this state</param>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="frameToHold">Frame index to hold on</param>
        public HoldFrameState(string id, string animationName, int frameToHold) : base(id, animationName)
        {
            FrameToHold = frameToHold;
            ShouldLoop = false;
        }

        /// <summary>
        ///     The frame index this state will hold on
        /// </summary>
        public int FrameToHold { get; }

        public override void OnEnter(IAnimationContext context)
        {
            // First play the animation without looping
            base.OnEnter(context);

            // Then immediately set it to the target frame
            if (context?.Animator != null)
            {
                context.Animator.SetCurrentFrame(FrameToHold);
                context.Animator.Pause();
            }
        }
    }
}
