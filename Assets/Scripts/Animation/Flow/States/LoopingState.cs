using Animation.Flow.Core;
using Animation.Flow.Interfaces;

namespace Animation.Flow.States
{
    /// <summary>
    ///     State that plays an animation in a loop
    /// </summary>
    public class LoopingState : FlowState
    {
        /// <summary>
        ///     Create a new looping animation state
        /// </summary>
        /// <param name="id">Unique identifier for this state</param>
        /// <param name="animationName">Name of the animation to play</param>
        public LoopingState(string id, string animationName) : base(id, animationName)
        {
            ShouldLoop = true;
        }

        public override void OnEnter(IAnimationContext context)
        {
            // Play the animation with looping
            base.OnEnter(context);
        }
    }
}
