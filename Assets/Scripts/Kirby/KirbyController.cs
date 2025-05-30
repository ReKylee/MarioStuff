using Kirby.Interfaces;
using Kirby.States;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Main controller for Kirby character
    /// </summary>
    public class KirbyController : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Animator animator;

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Default Settings")] [SerializeField]
        private MovementParameters defaultMovementParameters;

        [SerializeField] private AnimationSet defaultAnimationSet;
        [SerializeField] private KirbyTransformation defaultTransformation;

        // Current state and transformation
        private IKirbyState currentState;

        // Services/Managers

        // Reference to swallowed object/enemy (for transformations)
        private GameObject swallowedObject;

        // Public accessors
        public IInputHandler InputHandler { get; private set; }

        public IMovementController MovementController { get; private set; }

        public IAnimationHandler AnimationHandler { get; private set; }

        public IKirbyTransformation CurrentTransformation { get; private set; }

        // State flags
        public bool IsFull { get; private set; }

        private void Awake()
        {
            // Initialize components if not assigned
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // Initialize handlers
            InputHandler = new KirbyInputHandler();
            MovementController = new KirbyMovementController(rb, defaultMovementParameters);
            AnimationHandler = new KirbyAnimationHandler(animator, defaultAnimationSet);

            // Set default transformation
            SetTransformation(defaultTransformation);
        }

        private void Start()
        {
            // Set initial state to idle
            currentState = new IdleState(this);
            currentState.EnterState();
        }

        private void Update()
        {
            // Update input
            InputHandler.UpdateInputs();

            // Update current state logic
            currentState.LogicUpdate();

            // Check for state transitions
            CheckStateTransitions();
        }

        private void FixedUpdate()
        {
            // Update physics in current state
            currentState.PhysicsUpdate();
        }

        /// <summary>
        ///     Checks if a state transition should occur
        /// </summary>
        private void CheckStateTransitions()
        {
            IKirbyState nextState = currentState.CheckTransitions();
            if (nextState != null && nextState != currentState)
            {
                TransitionToState(nextState);
            }
        }

        /// <summary>
        ///     Transitions to a new state
        /// </summary>
        public void TransitionToState(IKirbyState newState)
        {
            currentState.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        /// <summary>
        ///     Sets Kirby's current transformation
        /// </summary>
        public void SetTransformation(IKirbyTransformation transformation)
        {
            // Revert current transformation if one exists
            if (CurrentTransformation != null)
            {
                CurrentTransformation.OnRevert(this);
            }

            // Set new transformation
            CurrentTransformation = transformation;

            // Apply the new transformation
            if (CurrentTransformation != null)
            {
                CurrentTransformation.OnTransform(this);

                // Update animation handler with new animation set
                (AnimationHandler as KirbyAnimationHandler)?.SetAnimationSet(CurrentTransformation.AnimationSet);

                // Update movement controller with transformation-specific parameters
                (MovementController as KirbyMovementController)?.SetMovementParameters(CurrentTransformation
                    .MovementOverrides);
            }
        }

        /// <summary>
        ///     Sets Kirby's full state (mouth full)
        /// </summary>
        public void SetFullState(bool isFull, GameObject swallowed = null)
        {
            IsFull = isFull;
            swallowedObject = swallowed;

            // Update animation handler with full status
            (AnimationHandler as KirbyAnimationHandler)?.SetFullStatus(isFull);
        }

        /// <summary>
        ///     Makes Kirby swallow the currently held object, potentially transforming
        /// </summary>
        public void Swallow()
        {
            if (IsFull && swallowedObject != null)
            {
                // Check if the swallowed object grants a transformation
                ITransformationProvider transformationProvider =
                    swallowedObject.GetComponent<ITransformationProvider>();

                if (transformationProvider != null)
                {
                    // Get the transformation and apply it
                    IKirbyTransformation transformation = transformationProvider.GetTransformation();
                    if (transformation != null)
                    {
                        SetTransformation(transformation);
                    }
                }

                // Destroy the swallowed object
                Destroy(swallowedObject);
                swallowedObject = null;
            }

            // Kirby is no longer full after swallowing
            SetFullState(false);
        }

        /// <summary>
        ///     Makes Kirby spit out the currently held object
        /// </summary>
        public void Spit()
        {
            if (IsFull && swallowedObject != null)
            {
                // Implementation to spawn spit projectile
                // TODO: Add projectile spawn logic here

                // Destroy the swallowed object
                Destroy(swallowedObject);
                swallowedObject = null;
            }

            // Kirby is no longer full after spitting
            SetFullState(false);
        }

        /// <summary>
        ///     Makes Kirby inhale objects in front of him
        /// </summary>
        public void Inhale()
        {
            // Implementation for inhale mechanic
            // This would be implemented with physics and colliders
            // to pull objects toward Kirby

            // Create an inhale force zone in front of Kirby
            Vector2 inhaleDirection = IsFacingLeft() ? Vector2.left : Vector2.right;
            Vector2 inhalePosition = (Vector2)transform.position + inhaleDirection * 1.5f;

            // Detect objects in the inhale zone
            var colliders = Physics2D.OverlapCircleAll(inhalePosition, 1.5f);
            foreach (Collider2D collider in colliders)
            {
                // Skip Kirby's own colliders
                if (collider.gameObject == gameObject) continue;

                // Apply force to pull objects toward Kirby
                Rigidbody2D objRb = collider.GetComponent<Rigidbody2D>();
                if (objRb != null)
                {
                    Vector2 pullDirection =
                        ((Vector2)transform.position - (Vector2)collider.transform.position).normalized;

                    objRb.AddForce(pullDirection * 10f, ForceMode2D.Force);

                    // Check if object is close enough to be swallowed
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    if (distance < 1.0f && IsFull == false)
                    {
                        // Notify the inhale state that an object has been inhaled
                        if (currentState is InhaleState inhaleState)
                        {
                            inhaleState.OnObjectInhaled(collider.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Stops the inhale effect
        /// </summary>
        public void StopInhale()
        {
            // Clean up any ongoing inhale effects
            // This could include stopping particles, sounds, etc.

            // No specific implementation needed for now
        }

        /// <summary>
        ///     Sets the swallowed object reference
        /// </summary>
        public void SetSwallowedObject(GameObject obj)
        {
            swallowedObject = obj;
        }

        /// <summary>
        ///     Execute the current form's primary ability
        /// </summary>
        public void ExecuteAbility()
        {
            if (CurrentTransformation != null && CurrentTransformation.Abilities.Count > 0)
            {
                // Execute the first ability (could be extended to support multiple abilities)
                CurrentTransformation.Abilities[0].Execute(this);
            }
        }

        /// <summary>
        ///     Checks if Kirby is facing left
        /// </summary>
        public bool IsFacingLeft() => transform.localScale.x < 0;

        /// <summary>
        ///     Revert Kirby to his default form
        /// </summary>
        public void RevertToDefaultForm()
        {
            SetTransformation(defaultTransformation);
        }

        /// <summary>
        ///     Gets the name of the current state for debugging purposes
        /// </summary>
        public string GetCurrentStateName()
        {
            if (currentState == null) return "None";
            return currentState.GetType().Name;
        }
    }
}
