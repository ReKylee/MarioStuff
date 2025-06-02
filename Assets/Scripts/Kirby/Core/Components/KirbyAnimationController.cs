using GabrielBigardi.SpriteAnimator;
using Kirby.Abilities;
using Kirby.Abilities.Animation;
using UnityEngine;

namespace Kirby.Core.Components
{
    /// <summary>
    ///     Component that manages Kirby's animation state machine
    ///     This handles the animation flow for different copy abilities
    /// </summary>
    public class KirbyAnimationController : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator spriteAnimator;
        [SerializeField] private CopyAbilityAnimationData defaultAnimationData;
        private CopyAbilityAnimationData _currentAnimationData;

        private KirbyController _kirbyController;
        private KirbyAnimationStateMachine _stateMachine;

        private void Awake()
        {
            _kirbyController = GetComponent<KirbyController>();

            if (!spriteAnimator)
            {
                spriteAnimator = GetComponent<SpriteAnimator>();
            }

            if (!spriteAnimator)
            {
                Debug.LogError("No SpriteAnimator found on or assigned to KirbyAnimationController.");
                enabled = false;
                return;
            }

            // Initialize with default animation data
            SetAnimationData(defaultAnimationData);
        }

        private void Update()
        {
            if (_stateMachine == null || !_kirbyController)
                return;

            // Update the animation state machine with the current input
            _stateMachine.Update(_kirbyController.CurrentInput);
        }

        private void LateUpdate()
        {
            // Apply the final animation state after all game logic is processed
            _stateMachine?.ApplyCurrentState();
        }

        /// <summary>
        ///     Set the animation data to use and initialize the state machine
        /// </summary>
        public void SetAnimationData(CopyAbilityAnimationData animationData)
        {
            if (animationData == null)
            {
                if (defaultAnimationData != null)
                {
                    animationData = defaultAnimationData;
                }
                else
                {
                    Debug.LogWarning("No animation data provided and no default set.");
                    return;
                }
            }

            _currentAnimationData = animationData;

            // Clean up previous state machine if it exists
            _stateMachine?.Cleanup();

            // Create new state machine with the provided animation data
            _stateMachine = _currentAnimationData.CreateStateMachine(_kirbyController, spriteAnimator);
        }

        /// <summary>
        ///     Set the direction the sprite should face
        /// </summary>
        public void SetDirection(int direction)
        {
            _stateMachine?.SetDirection(direction);
        }
    }
}
