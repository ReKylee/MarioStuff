using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

[TaskCategory("Animation")]
[TaskDescription("Plays a SpriteAnimation using the SpriteAnimator component")]
public class Play : Action
{
    private bool _animationCompleted;

    private SpriteAnimator _animator;
    private bool _isWaiting;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The name of the animation to play")]
    public SharedString AnimationName;


    [BehaviorDesigner.Runtime.Tasks.Tooltip(
        "Wait for the animation to complete before returning Success (only works with PlayOnce animations)")]
    public SharedBool WaitForCompletion = true;

    public override void OnAwake()
    {
        _animator = GetComponent<SpriteAnimator>();

        if (!_animator)
        {
            Debug.LogError($"Play action: No SpriteAnimator component found on {gameObject.name}");
        }


    }

    public override void OnStart()
    {
        if (!_animator || string.IsNullOrEmpty(AnimationName.Value))
        {
            return;
        }


        // Reset completion flag
        _animationCompleted = false;

        // Subscribe to completion event if we need to wait
        if (WaitForCompletion.Value && _animator.CurrentAnimation.SpriteAnimationType == SpriteAnimationType.PlayOnce)
        {
            _animator.OnAnimationComplete += OnAnimationCompleted;
            _isWaiting = true;
        }
        else
        {
            _isWaiting = false;
        }

        _animator.Play(AnimationName.Value);

    }

    public override TaskStatus OnUpdate()
    {
        if (_animator == null || string.IsNullOrEmpty(AnimationName.Value))
        {
            return TaskStatus.Failure;
        }

        // If we're not waiting for completion, return success immediately
        if (!_isWaiting)
        {
            return TaskStatus.Success;
        }

        // Check if the animation has completed
        if (_animationCompleted)
        {
            return TaskStatus.Success;
        }

        // Still playing
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        // Always unsubscribe from the event to prevent memory leaks
        if (_animator)
        {
            _animator.OnAnimationComplete -= OnAnimationCompleted;
        }
    }

    private void OnAnimationCompleted()
    {
        _animationCompleted = true;
        if (_animator)
        {
            _animator.OnAnimationComplete -= OnAnimationCompleted;
        }
    }

    public override void OnReset()
    {
        AnimationName = "";
        WaitForCompletion = true;
    }
}
