using GabrielBigardi.SpriteAnimator;

namespace Animation.Flow.States
{
    /// <summary>
    ///     Specialized animation state that holds the first frame of an animation
    /// </summary>
    public class HoldFrameState : AnimationStateBase
    {
        private readonly int _frameToHold;

        public HoldFrameState(string id, string animationName, int frameToHold = 0)
            : base(id, animationName)
        {
            _frameToHold = frameToHold;
            ShouldLoop = false;
        }

        public override void OnEnter(IAnimationContext context)
        {
            base.OnEnter(context);

            // Hold on specific frame
            SpriteAnimator animator = context.Animator as SpriteAnimator;
            if (animator != null)
            {
                animator.SetCurrentFrame(_frameToHold);
                animator.Pause();
            }
        }

        public override void OnExit(IAnimationContext context)
        {
            base.OnExit(context);

            // Resume animation on exit
            SpriteAnimator animator = context.Animator as SpriteAnimator;
            if (animator != null)
            {
                animator.Resume();
            }
        }
    }

    /// <summary>
    ///     Specialized animation state that loops an animation
    /// </summary>
    public class LoopingState : AnimationStateBase
    {
        public LoopingState(string id, string animationName)
            : base(id, animationName)
        {
            ShouldLoop = true;
        }
    }

    /// <summary>
    ///     Specialized animation state that plays an animation once and tracks completion
    /// </summary>
    public class OneTimeState : AnimationStateBase
    {
        public OneTimeState(string id, string animationName)
            : base(id, animationName)
        {
            ShouldLoop = false;
        }

        public override void OnEnter(IAnimationContext context)
        {
            base.OnEnter(context);

            // Track that we're starting the animation
            context.SetParameter("animationComplete", false);

            // Register for animation complete event
            SpriteAnimator animator = context.Animator as SpriteAnimator;
            if (animator != null)
            {
                animator.OnAnimationComplete += HandleAnimationComplete;
            }
        }

        public override void OnExit(IAnimationContext context)
        {
            base.OnExit(context);

            // Unregister from animation complete event
            SpriteAnimator animator = context.Animator as SpriteAnimator;
            if (animator != null)
            {
                animator.OnAnimationComplete -= HandleAnimationComplete;
            }
        }

        private void HandleAnimationComplete()
        {
            // When we receive the completion callback, set the parameter
            // This will be picked up by AnimationCompleteCondition
        }

        public override void OnUpdate(IAnimationContext context, float deltaTime)
        {
            base.OnUpdate(context, deltaTime);

            // Check if animation is complete
            SpriteAnimator animator = context.Animator as SpriteAnimator;
            if (animator != null && animator.AnimationCompleted)
            {
                context.SetParameter("animationComplete", true);
            }
        }
    }
}
