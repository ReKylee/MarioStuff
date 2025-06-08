using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

[TaskCategory("Animation")]
[TaskDescription("Plays a SpriteAnimation using the SpriteAnimator component")]
public class Play : Action
{
    private SpriteAnimator _animator;
    private string _currentAnimationName;
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

        _currentAnimationName = AnimationName.Value;
        _isWaiting = WaitForCompletion.Value;

        _animator.PlayIfNotPlaying(_currentAnimationName);
        _animator.OnAnimationComplete += animatorOnOnAnimationComplete;
    }

    public override TaskStatus OnUpdate()
    {
        if (!_animator || string.IsNullOrEmpty(AnimationName.Value))
        {
            return TaskStatus.Failure;
        }

        // If we're not waiting for completion, return success immediately
        return _isWaiting ? TaskStatus.Running : TaskStatus.Success;
    }

    public override void OnEnd()
    {
        _animator.OnAnimationComplete -= animatorOnOnAnimationComplete;
    }
    private void animatorOnOnAnimationComplete()
    {
        _isWaiting = false;
        _animator.OnAnimationComplete -= animatorOnOnAnimationComplete;
    }

    public override void OnReset()
    {
        AnimationName = "";
        WaitForCompletion = true;
        _animator.OnAnimationComplete -= animatorOnOnAnimationComplete;

    }
}
