using UnityEngine;

// Added for Debug.LogWarning

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

            // Hold on specific frame using IAnimator
            if (context.Animator != null)
            {
                context.Animator.SetCurrentFrame(_frameToHold);
                context.Animator.Pause();
            }
        }

        public override void OnExit(IAnimationContext context)
        {
            base.OnExit(context);

            // Resume animation on exit using IAnimator
            if (context.Animator != null)
            {
                context.Animator.Resume();
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
        private IAnimationContext _context; // Store context for the callback

        public OneTimeState(string id, string animationName)
            : base(id, animationName)
        {
            ShouldLoop = false;
        }

        public override void OnEnter(IAnimationContext context)
        {
            base.OnEnter(context);
            _context = context; // Store the context

            // Track that we're starting the animation
            context.SetParameter("animationComplete", false);

            // Register for animation complete event using IAnimator
            if (context.Animator != null)
            {
                context.Animator.RegisterAnimationCompleteCallback(HandleAnimationComplete);
            }
        }

        public override void OnExit(IAnimationContext context)
        {
            base.OnExit(context);

            // Unregister from animation complete event using IAnimator
            if (context.Animator != null)
            {
                context.Animator.UnregisterAnimationCompleteCallback(HandleAnimationComplete);
            }

            _context = null; // Clear the stored context
        }

        private void HandleAnimationComplete()
        {
            // Now we can access the stored context to set the parameter
            if (_context != null)
            {
                _context.SetParameter("animationComplete", true);
            }
            else
            {
                // This case should ideally not happen if OnEnter/OnExit are managed correctly
                Debug.LogWarning("HandleAnimationComplete called but context was null.");
            }
        }

        // OnUpdate polling can be kept as a fallback or removed if the callback is reliable enough.
        // For now, let's keep it to ensure completion is always caught.
        public override void OnUpdate(IAnimationContext context, float deltaTime)
        {
            base.OnUpdate(context, deltaTime);

            // Check if animation is complete using IAnimator
            // This also handles cases where the callback might not have fired or context was lost
            if (context.Animator != null && context.Animator.IsAnimationComplete)
            {
                if (!context.GetParameter<bool>("animationComplete")) // Only set if not already set by callback
                {
                    context.SetParameter("animationComplete", true);
                }
            }
        }
    }
}
