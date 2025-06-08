namespace Kirby.Core.Abilities.Animation
{
    /// <summary>
    ///     Handles ability state tracking for Kirby
    /// </summary>
    public class KirbyAbilityStateHandler
    {
        private readonly AnimationStateTracker _stateTracker;

        public KirbyAbilityStateHandler(AnimationStateTracker stateTracker)
        {
            _stateTracker = stateTracker;
        }

        /// <summary>
        ///     Tracks inhaling state based on input
        /// </summary>
        public void TrackInhaleState(InputContext input)
        {
            if (input.AttackHeld && !_stateTracker.IsFull && !_stateTracker.IsFlying && !_stateTracker.IsFloating)
            {
                if (!_stateTracker.IsInhaling && _stateTracker.CurrentState != AnimState.Inhale)
                {
                    _stateTracker.ChangeState(AnimState.Inhale);
                    _stateTracker.IsInhaling = true;
                    _stateTracker.InhaleTimer = 0f;
                }
            }
            else if (_stateTracker.WasAttackHeld && !input.AttackHeld && _stateTracker.IsInhaling)
            {
                _stateTracker.IsInhaling = false;
                _stateTracker.WaitingForAnimComplete = false; // Allow immediate state transition

                // Return to appropriate state based on ground status
                if (_stateTracker.CurrentState == AnimState.Inhale)
                {
                    if (_stateTracker.WaitingForAnimComplete)
                    {
                        _stateTracker.WaitingForAnimComplete = false;
                    }

                    // Force transition to appropriate state
                    AnimState newState = _stateTracker.WasGrounded ? AnimState.Idle : AnimState.Fall;
                    _stateTracker.ChangeState(newState);
                }
            }

            _stateTracker.WasAttackHeld = input.AttackHeld;
        }
    }
}
