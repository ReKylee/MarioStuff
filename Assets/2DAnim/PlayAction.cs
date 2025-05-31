using System;
using GabrielBigardi.SpriteAnimator;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Play", story: "[SpriteAnimator] Play [Animation]", category: "Action",
    id: "ee52ba3967c36fa92b16d195c931da58")]
public class PlayAction : Action
{

    [SerializeReference] public BlackboardVariable<SpriteAnimator> SpriteAnimator;
    [SerializeReference] public BlackboardVariable<string> Animation;
    public bool animationCompleted { get; set; }

    protected override Status OnStart()
    {
        if (!SpriteAnimator.Value)
        {
            Debug.LogError("SpriteAnimator is not set.");
            return Status.Failure;
        }

        animationCompleted = false;
        SpriteAnimator.Value.PlayIfNotPlaying(Animation.Value);
        SpriteAnimator.Value.OnAnimationComplete += () => animationCompleted = true;
        return Status.Running;
    }

    protected override Status OnUpdate() => !animationCompleted ? Status.Running : Status.Success;

    protected override void OnEnd()
    {
    }
}
