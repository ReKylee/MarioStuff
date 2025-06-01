using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Fly ability - allows Kirby to fly by repeatedly tapping jump or holding it
    ///     When jump is released, Kirby slowly floats down
    /// </summary>
    public class FlyAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private float _flapCooldown;
        private bool _isFlying;
        private bool _wasJumpHeld;

        public Vector2 ProcessMovement(Vector2 currentVelocity, bool isGrounded, InputContext inputContext)
        {
            // Reset flying state and cooldown when grounded
            if (isGrounded)
            {
                _flapCooldown = 0f;
                _wasJumpHeld = false;
                _isFlying = false;
                return currentVelocity;
            }

            // Update flap cooldown
            if (_flapCooldown > 0)
            {
                _flapCooldown -= Time.deltaTime;
            }

            // Start flying only when jump is pressed while in air
            if (inputContext.JumpPressed && !_isFlying)
            {
                _isFlying = true;
                currentVelocity.y = Controller.Stats.flapImpulse;
                _flapCooldown = 0.2f;
                _wasJumpHeld = true;
                return currentVelocity;
            }

            // Only process flying mechanics if we're already in flying mode
            if (_isFlying)
            {
                // Handle jump input for flying
                if (inputContext.JumpHeld)
                {
                    // Apply full flap impulse when jump is first pressed or when tapped again after releasing
                    if (!_wasJumpHeld && _flapCooldown <= 0)
                    {
                        currentVelocity.y = Controller.Stats.flapImpulse;
                        _flapCooldown = 0.2f; // Add a small cooldown between flaps
                    }
                    // Continue full rising when holding jump after initial flap
                    else if (_flapCooldown <= 0)
                    {
                        // Apply the same full impulse when holding as when tapping
                        currentVelocity.y = Controller.Stats.flapImpulse;
                        _flapCooldown = 0.2f; // Same cooldown as tapping
                    }

                    _wasJumpHeld = true;
                }
                else
                {
                    // Reset jump held state when button is released
                    _wasJumpHeld = false;

                    // Apply float descent when falling and jump is not held
                    if (currentVelocity.y < 0)
                    {
                        currentVelocity.y *= Controller.Stats.floatDescentSpeed;
                    }
                }
            }

            return currentVelocity;
        }
    }
}
