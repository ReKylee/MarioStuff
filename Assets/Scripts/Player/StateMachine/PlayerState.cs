using UnityEngine;
using System;

namespace Player.StateMachine
{
    /// <summary>
    /// Base class for all player movement states
    /// </summary>
    public abstract class PlayerState
    {
        protected IStateMachineContext Context;

        public PlayerState(IStateMachineContext context)
        {
            Context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void HandleInput() { }
    }
}
