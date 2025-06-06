using System;
using System.Collections;
using System.Linq;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Kirby.Core.Components
{
    public enum KirbyState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        JumpStart,
        Jumping,
        Falling,
        Flying,
        Inhaling,
        Full,
        Landing,
        BouncingOffFloor
    }

    public enum KirbyFullness
    {
        Normal,
        Full
    }


    [RequireComponent(typeof(SpriteAnimator))]
    public class KirbyAnimationController : MonoBehaviour
    {
        [SerializeField] private SpriteAnimationObject animationStates;
        [SerializeField] private float fallHeightThreshold = 5f;
        [SerializeField] private float bouncePreLandDistance = 1f;

        private SpriteAnimator _animator;
        private bool _canFly = true;

        // Animation tracking
        private Coroutine _currentAnimationCoroutine;
        private string _currentAnimationName;
        private float _fallStartTime;

        // State tracking
        private bool _hasBounced;
        private bool _isBouncingBackUp;
        private bool _isJumpHeld;
        private float _jumpStartHeight;
        private KirbyController _kirbyController;
        private bool _reachedApex;
        private SpriteRenderer _spriteRenderer;
        private KirbyGroundCheck.SurfaceType _surfaceType = KirbyGroundCheck.SurfaceType.Flat;
        private bool _wasInhalePressed;

        // Input tracking
        private bool _wasJumpPressed;
        private bool _wasSpitPressed;
        private bool _wasSwallowPressed;

        public KirbyState CurrentState { get; private set; } = KirbyState.Idle;

        public KirbyFullness Fullness { get; private set; } = KirbyFullness.Normal;

        public bool IsFlying { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<SpriteAnimator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _kirbyController = GetComponent<KirbyController>();
        }

        private void Start()
        {
            if (animationStates == null)
            {
                Debug.LogError("SpriteAnimationObject not assigned!");
                return;
            }

            // Start with idle animation
            PlayIdleAnimation();
        }

        private void Update()
        {
            if (animationStates == null || _kirbyController == null) return;

            UpdateInputTracking();
            UpdateKirbyGroundCheck();
            UpdateAnimationState();
            UpdateSpriteFlipping();
        }

        private void UpdateInputTracking()
        {
            InputContext input = _kirbyController.CurrentInput;

            bool jumpPressed = input.JumpPressed && !_wasJumpPressed;
            _wasJumpPressed = input.JumpPressed;
            _isJumpHeld = input.JumpHeld;

            bool inhalePressed = input.AttackHeld && !_wasInhalePressed;
            _wasInhalePressed = input.AttackHeld;

            bool spitPressed = input.AttackPressed && !_wasSpitPressed;
            _wasSpitPressed = input.AttackPressed;

            bool swallowPressed = input.CrouchPressed && !_wasSwallowPressed;
            _wasSwallowPressed = input.CrouchPressed;

            HandleInputActions(jumpPressed, inhalePressed, spitPressed, swallowPressed);
        }
        private void UpdateSpriteFlipping()
        {
            if (_spriteRenderer == null) return;

            InputContext input = _kirbyController.CurrentInput;

            // Handle movement-based flipping
            if (Mathf.Abs(input.WalkInput) > 0.1f)
            {
                _spriteRenderer.flipX = input.WalkInput < 0;
            }
        }

        private void UpdateKirbyGroundCheck()
        {

            _surfaceType = _kirbyController.GroundType;
        }

        private void HandleInputActions(bool jumpPressed, bool inhalePressed, bool spitPressed, bool swallowPressed)
        {
            InputContext input = _kirbyController.CurrentInput;

            // Handle flying exit (spit while flying)
            if (IsFlying && spitPressed)
            {
                ExitFlying();
                return;
            }

            // Handle fullness state changes
            if (Fullness == KirbyFullness.Full)
            {
                if (spitPressed)
                {
                    SetFullness(KirbyFullness.Normal);
                    PlaySpitAnimation();
                    return;
                }

                if (swallowPressed)
                {
                    SetFullness(KirbyFullness.Normal);
                    PlaySwallowAnimation();
                    return;
                }
            }

            // Handle inhaling (only when normal)
            if (Fullness == KirbyFullness.Normal && inhalePressed && CanInhale())
            {
                StartInhaling();
                return;
            }

            // Handle jumping
            if (jumpPressed && CanJump())
            {
                if (IsFlying)
                {
                    PlayFlyFlapAnimation();
                }
                else if (CurrentState == KirbyState.Falling && _canFly && Fullness == KirbyFullness.Normal &&
                         _hasBounced)
                {
                    // After bouncing, allow transition to fly
                    StartFlying();
                }
                else if (CurrentState == KirbyState.Jumping && _canFly && Fullness == KirbyFullness.Normal)
                {
                    StartFlying();
                }
                else if (_kirbyController.IsGrounded)
                {
                    StartJump();
                }
            }
        }

        private void UpdateAnimationState()
        {
            InputContext input = _kirbyController.CurrentInput;
            bool isGrounded = _kirbyController.IsGrounded;

            // Handle ground detection changes
            if (!isGrounded && CurrentState is KirbyState.Idle or KirbyState.Walking or KirbyState.Running)
            {
                if (CurrentState != KirbyState.JumpStart && CurrentState != KirbyState.Jumping)
                {
                    StartFalling();
                }
            }

            // Handle landing
            if (isGrounded && (CurrentState == KirbyState.Falling || CurrentState == KirbyState.BouncingOffFloor))
            {
                HandleLanding();
            }

            // Handle movement animations when grounded
            if (isGrounded && !IsFlying && CurrentState != KirbyState.Crouching && CurrentState != KirbyState.Inhaling)
            {
                float moveInput = Mathf.Abs(input.WalkInput);

                if (input.CrouchPressed)
                {
                    PlayCrouchAnimation();
                }
                else if (moveInput > 0.7f) // Running threshold
                {
                    PlayRunAnimation();
                }
                else if (moveInput > 0.1f) // Walking threshold
                {
                    PlayWalkAnimation();
                }
                else
                {
                    PlayIdleAnimation();
                }
            }

            // Handle jump apex detection - switch to Jump animation once apex is reached OR jump is released
            if (CurrentState is KirbyState.JumpStart or KirbyState.Jumping)
            {
                if (_kirbyController.Velocity.y <= 0 && !_reachedApex || !_isJumpHeld && !_reachedApex)
                {
                    _reachedApex = true;
                    if (!IsFlying)
                    {
                        // Switch to Jump animation at apex or when jump is released
                        CurrentState = KirbyState.Jumping;
                        string jumpAnimName = Fullness == KirbyFullness.Full ? "Jump_Full" : "Jump";
                        PlaySpriteAnimation(jumpAnimName, () =>
                        {
                            // After Jump animation completes, start falling
                            if (CurrentState == KirbyState.Jumping)
                            {
                                StartFalling();
                            }
                        });
                    }
                }
            }

            // Check for bounce condition when falling
            if (CurrentState == KirbyState.Falling && !_hasBounced)
            {
                float fallDistance = _jumpStartHeight - transform.position.y;
                float distanceToGround = GetDistanceToGround();

                // Start bounce animation if we've fallen more than threshold and are close to ground
                if (fallDistance > fallHeightThreshold && distanceToGround <= bouncePreLandDistance &&
                    distanceToGround > 0)
                {
                    StartBounceOffFloor();
                }
            }
        }
        private float GetDistanceToGround()
        {
            // Simple raycast down to detect ground distance
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f);
            if (hit.collider != null)
            {
                return hit.distance;
            }

            return float.MaxValue;
        }

        private bool CanJump() => _kirbyController.IsGrounded || CurrentState == KirbyState.Jumping ||
                                  CurrentState == KirbyState.Falling || IsFlying;

        private bool CanInhale() =>
            _kirbyController.IsGrounded &&
            CurrentState is KirbyState.Idle or KirbyState.Walking or KirbyState.Running;

        /// <summary>
        ///     Gets a SpriteAnimation by name from the animationStates object
        /// </summary>
        private SpriteAnimation GetAnimation(string animationName)
        {
            return animationStates.SpriteAnimations.FirstOrDefault(anim => anim.Name == animationName);
        }

        /// <summary>
        ///     Plays a sprite animation and optionally executes a callback when it completes
        /// </summary>
        private void PlaySpriteAnimation(string animationName, Action onComplete = null)
        {
            SpriteAnimation animation = GetAnimation(animationName);
            if (animation == null)
            {
                Debug.LogWarning($"Animation '{animationName}' not found in animation object");
                return;
            }

            // Stop current animation coroutine if running
            if (_currentAnimationCoroutine != null)
            {
                StopCoroutine(_currentAnimationCoroutine);
                _currentAnimationCoroutine = null;
            }

            _currentAnimationName = animationName;
            _animator.Play(animation);

            // Start coroutine to track animation completion for PlayOnce animations
            if (animation.SpriteAnimationType == SpriteAnimationType.PlayOnce && onComplete != null)
            {
                _currentAnimationCoroutine = StartCoroutine(WaitForAnimationComplete(animation, onComplete));
            }
        }

        /// <summary>
        ///     Coroutine that waits for a PlayOnce animation to complete
        /// </summary>
        private IEnumerator WaitForAnimationComplete(SpriteAnimation animation, Action onComplete)
        {
            if (animation.FPS <= 0) yield break;

            float animationDuration = animation.Frames.Count / (float)animation.FPS;
            yield return new WaitForSeconds(animationDuration);

            onComplete?.Invoke();
            _currentAnimationCoroutine = null;
        }

        private void StartJump()
        {
            CurrentState = KirbyState.JumpStart;
            _jumpStartHeight = transform.position.y;
            _hasBounced = false;
            _canFly = true;
            _reachedApex = false;

            // Play JumpStart (one frame when you hold the jump button)
            PlaySpriteAnimation("JumpStart");
        }

        private void StartFalling()
        {
            CurrentState = KirbyState.Falling;
            _fallStartTime = Time.time;

            // Play looping Fall animation (two frame loop)
            string fallAnimName = Fullness == KirbyFullness.Full ? "Fall_Full" : "Fall";
            PlaySpriteAnimation(fallAnimName);
        }

        private void StartFlying()
        {
            IsFlying = true;
            CurrentState = KirbyState.Flying;

            PlaySpriteAnimation("JumpToFly", () =>
            {
                if (IsFlying)
                {
                    PlaySpriteAnimation("Fly");
                }
            });
        }

        private void PlayFlyFlapAnimation()
        {
            if (IsFlying)
            {
                // For fly flap, we can play the JumpToFly animation again to simulate flapping
                PlaySpriteAnimation("JumpToFly", () =>
                {
                    if (IsFlying)
                    {
                        PlaySpriteAnimation("Fly");
                    }
                });
            }
        }

        private void ExitFlying()
        {
            IsFlying = false;
            _canFly = false;
            StartFalling();
        }

        private void StartBounceOffFloor()
        {
            CurrentState = KirbyState.BouncingOffFloor;
            _hasBounced = true;
            _canFly = true; // Allow flying again after bounce
            _isBouncingBackUp = true;

            // Play BounceOffFloor animation
            PlaySpriteAnimation("BounceOffFloor", () =>
            {
                if (CurrentState == KirbyState.BouncingOffFloor)
                {
                    // After bounce animation, continue to falling (looping two frame animation)
                    _isBouncingBackUp = false;
                    StartFalling();
                }
            });
        }

        private void HandleLanding()
        {
            CurrentState = KirbyState.Landing;
            IsFlying = false;
            _reachedApex = false;

            // Play two frames of crouching (using Idle_Crouched) then back to idle
            PlaySpriteAnimation("Idle_Crouched", () => { PlayIdleAnimation(); });
        }

        private void StartInhaling()
        {
            CurrentState = KirbyState.Inhaling;

            PlaySpriteAnimation("Inhale", () =>
            {
                if (CurrentState == KirbyState.Inhaling)
                {
                    SetFullness(KirbyFullness.Full);
                    PlayIdleAnimation();
                }
            });
        }

        private void PlaySpitAnimation()
        {
            PlaySpriteAnimation("Spit", () => { PlayIdleAnimation(); });
        }

        private void PlaySwallowAnimation()
        {
            PlaySpriteAnimation("Swallow", () => { PlayIdleAnimation(); });
        }

        private void SetFullness(KirbyFullness newFullness)
        {
            Fullness = newFullness;
            if (newFullness == KirbyFullness.Full)
            {
                _canFly = false; // Can't fly when full
            }
        }

        private void PlayIdleAnimation()
        {
            CurrentState = KirbyState.Idle;

            string idleAnimName;

            if (Fullness == KirbyFullness.Full)
            {
                // Handle full state with slopes
                switch (_surfaceType)
                {
                    case KirbyGroundCheck.SurfaceType.Slope:
                        // Determine slope direction based on ground normal
                        bool slopeLeft = _kirbyController.GroundNormal.x > 0;
                        idleAnimName = slopeLeft ? "Idle_Full_SlopeL" : "Idle_Full_SlopeR";
                        break;
                    case KirbyGroundCheck.SurfaceType.DeepSlope:
                        // Determine slope direction based on ground normal
                        bool deepSlopeLeft = _kirbyController.GroundNormal.x > 0;
                        idleAnimName = deepSlopeLeft ? "Idle_Full_DSlopeL" : "Idle_Full_DSlopeR";
                        break;
                    default:
                        idleAnimName = "Idle_Full";
                        break;
                }
            }
            else
            {
                // Handle normal state with slopes
                switch (_surfaceType)
                {
                    case KirbyGroundCheck.SurfaceType.Slope:
                        // Determine slope direction based on ground normal
                        bool slopeLeft = _kirbyController.GroundNormal.x > 0;
                        idleAnimName = slopeLeft ? "Idle_SlopeL" : "Idle_SlopeR";
                        break;
                    case KirbyGroundCheck.SurfaceType.DeepSlope:
                        // Determine slope direction based on ground normal
                        bool deepSlopeLeft = _kirbyController.GroundNormal.x > 0;
                        idleAnimName = deepSlopeLeft ? "Idle_DSlopeL" : "Idle_DSlopeR";
                        break;
                    default:
                        idleAnimName = "Idle";
                        break;
                }
            }

            PlaySpriteAnimation(idleAnimName);
        }

        private void PlayWalkAnimation()
        {
            if (CurrentState == KirbyState.Walking && _currentAnimationName == "Walk") return; // Already walking

            CurrentState = KirbyState.Walking;
            PlaySpriteAnimation("Walk");
        }

        private void PlayRunAnimation()
        {
            if (CurrentState == KirbyState.Running &&
                (_currentAnimationName == "Run" || _currentAnimationName == "Run_Full")) return; // Already running

            CurrentState = KirbyState.Running;
            string runAnimName = Fullness == KirbyFullness.Full ? "Run_Full" : "Run";
            PlaySpriteAnimation(runAnimName);
        }

        private void PlayCrouchAnimation()
        {
            if (CurrentState == KirbyState.Crouching && _currentAnimationName == "Idle_Crouched")
                return; // Already crouching

            CurrentState = KirbyState.Crouching;
            PlaySpriteAnimation("Idle_Crouched");
        }
    }
}
