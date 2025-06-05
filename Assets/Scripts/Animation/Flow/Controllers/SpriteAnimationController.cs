using Animation.Flow.Adapters;
using Animation.Flow.Core;
using Animation.Flow.Interfaces;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Animation.Flow.Controllers
{
    /// <summary>
    ///     Generic sprite animation controller that uses the animation flow system
    ///     Can be used as a base class for character-specific animation controllers
    /// </summary>
    [RequireComponent(typeof(SpriteAnimator))]
    public class SpriteAnimationController : AnimationFlowController
    {
        [Header("Animation Assets")] [SerializeField]
        protected AnimationFlowAsset _defaultAnimationFlow;

        [Header("Debug")] [SerializeField] protected bool _showDebugInfo;

        protected SpriteAnimatorAdapter _animatorAdapter;

        // Cached components
        protected SpriteAnimator _spriteAnimator;

        #region Public Methods

        /// <summary>
        ///     Set the animation flow asset
        /// </summary>
        public virtual void SetAnimationFlowAsset(AnimationFlowAsset flowAsset)
        {
            FlowAsset = flowAsset;
        }

        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            // Initialize components first
            InitializeComponents();

            // Apply default animation flow if provided
            if (_defaultAnimationFlow != null)
            {
                FlowAsset = _defaultAnimationFlow;
            }

            // Call base implementation
            base.Awake();
        }

        protected virtual void Start()
        {
            // Nothing to do in the base class
        }

        protected override void Update()
        {
            // Call base implementation to handle animation flow
            base.Update();

            // Debug information
            if (_showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // Initialize components if needed
            if (_spriteAnimator == null)
            {
                InitializeComponents();
            }

            // Call base implementation
            base.OnValidate();
        }
#endif

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Initialize required components, can be called from editor or runtime
        /// </summary>
        protected virtual void InitializeComponents()
        {
            // Get required sprite animator
            _spriteAnimator = GetComponent<SpriteAnimator>();
            if (_spriteAnimator == null)
            {
                Debug.LogError($"[{GetType().Name}] Missing SpriteAnimator component.", this);
                enabled = false;
            }
        }

        /// <summary>
        ///     Create the animator adapter for the animation flow system
        /// </summary>
        protected override IAnimator CreateAnimator()
        {
            // Make sure components are initialized
            if (!_spriteAnimator)
            {
                InitializeComponents();

                // If still null after initialization, we can't create an adapter
                if (!_spriteAnimator)
                {
                    return null;
                }
            }

            // Create and cache the adapter
            _animatorAdapter = new SpriteAnimatorAdapter(_spriteAnimator);
            return _animatorAdapter;
        }

        /// <summary>
        ///     Display debug information in the console
        /// </summary>
        protected virtual void DisplayDebugInfo()
        {
            if (_spriteAnimator != null)
            {
                Debug.Log($"[{GetType().Name}] Current Animation: {_spriteAnimator.CurrentAnimation.Name}, " +
                          $"Frame: {_spriteAnimator.CurrentFrame}");
            }
        }

        #endregion

    }
}
