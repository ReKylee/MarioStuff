using GabrielBigardi.SpriteAnimator;
using Kirby.Core.Components;
using UnityEngine;

namespace Kirby.Abilities.Animation
{
    /// <summary>
    ///     Base class for animation states in Kirby's animation state machine
    /// </summary>
    public abstract class KirbyAnimationState
    {

        // When the state was entered
        private float _stateEnteredTime;
        protected SpriteAnimator Animator { get; private set; }
        protected KirbyController Controller { get; private set; }

        // Animation name that this state will play
        public string AnimationName { get; protected set; }

        // Priority of this animation state (higher number = higher priority)
        public virtual float Priority { get; protected set; } = 1f;

        // Whether this state can transition to other states before completing
        public virtual bool CanBeInterrupted { get; protected set; } = true;

        // Whether this animation should loop
        public virtual bool ShouldLoop { get; protected set; } = true;

        // Track if this state is currently active
        public bool IsActive { get; private set; }

        // How long this state has been active
        public float TimeInState => IsActive ? Time.time - _stateEnteredTime : 0f;

        public void Initialize(KirbyController controller, SpriteAnimator animator)
        {
            Controller = controller;
            Animator = animator;
            OnInitialize();
        }

        // Custom initialization logic
        protected virtual void OnInitialize()
        {
        }

        // Called when the state is entered
        public virtual void Enter()
        {
            IsActive = true;
            _stateEnteredTime = Time.time;

            if (!string.IsNullOrEmpty(AnimationName) && Animator)
            {
                Animator.Play(AnimationName);
            }
        }

        // Called when the state is exited
        public virtual void Exit()
        {
            IsActive = false;
        }

        // Called to update the state's internal logic
        public virtual void Update(InputContext input)
        {
        }

        // Returns true if this state should be active based on current conditions
        public abstract bool ShouldBeActive(InputContext input);

        // Helper to check if animation has finished
        protected bool HasAnimationFinished()
        {
            if (!Animator) return true;

            return !ShouldLoop && Animator.AnimationCompleted;

        }
    }
}
