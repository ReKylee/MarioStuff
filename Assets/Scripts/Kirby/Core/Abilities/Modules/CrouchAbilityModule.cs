using UnityEngine;

namespace Kirby.Abilities
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    /// </summary>
    public class CrouchAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        [SerializeField] private bool test;
        public Vector2 ProcessMovement(
            Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            if (!Controller || Controller.Stats == null) return currentVelocity;


            float acceleration = isGrounded ? Controller.Stats.groundAcceleration : Controller.Stats.airAcceleration;
            float deceleration = isGrounded ? Controller.Stats.groundDeceleration : Controller.Stats.airDeceleration;

            float inputMagnitude = Mathf.Abs(inputContext.MoveInput.x);

            float moveSpeed;
            if (Mathf.Abs(Controller.Rigidbody.linearVelocity.x) > Controller.Stats.walkSpeed && inputMagnitude > 0.01f)
            {
                moveSpeed = Controller.Stats.runSpeed;
            }
            else
            {
                moveSpeed = Controller.Stats.walkSpeed;
            }

            currentVelocity.x = inputMagnitude > 0.01f
                ? Mathf.MoveTowards(currentVelocity.x, moveSpeed * Mathf.Sign(inputContext.MoveInput.x),
                    acceleration * Time.deltaTime)
                : Mathf.MoveTowards(currentVelocity.x, 0, deceleration * Time.deltaTime);

            return currentVelocity;
        }
    }
}
