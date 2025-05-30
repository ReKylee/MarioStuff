using System.Collections;
using UnityEngine;

/// <summary>
/// Ability for flying by flapping
/// </summary>
public class FlyAbility : MovementAbilityBase
{
    // Parameters
    private float _flyingFlapForce = 8f;
    private float _flyingHorizontalSpeed = 6f;
    private float _flyingMinDuration = 0.1f;
    private float _flyingMaxDuration = 0.5f;
    private float _jumpToFlyDuration = 0.25f;
    
    // State
    private bool _isFlying = false;
    private bool _isTransitioningToFly = false;
    private float _flyingTimeCounter = 0f;
    private bool _hasDoubleJumped = false;
    
    /// <summary>
    /// Priority of flying ability
    /// </summary>
    public override int Priority => 20;
    
    /// <summary>
    /// Availability check for flying
    /// </summary>
    public override bool IsAvailable => base.IsAvailable && _character.CurrentForm != CharacterForm.Full;
    
    /// <summary>
    /// Handle input for flying
    /// </summary>
    public override bool HandleInput(InputContext context)
    {
        // Can't fly in Full form
        if (_character.CurrentForm == CharacterForm.Full)
            return false;
        
        if (context.EventType == InputEventType.Pressed && context.JumpPressed)
        {
            // Start flying if in air and not already flying
            if (!_character.IsGrounded && !_isFlying && !_isTransitioningToFly)
            {
                if (!_hasDoubleJumped)
                {
                    // Double jump to transition to flying
                    StartDoubleJump();
                    return true;
                }
            }
            else if (_isFlying)
            {
                // Already flying - do a flap
                Flap();
                return true;
            }
        }
        else if (context.EventType == InputEventType.Update)
        {
            // Update flying timer
            if (_isFlying)
            {
                UpdateFlyingTimer(context.DeltaTime);
                return true;
            }
        }
        else if (context.EventType == InputEventType.Pressed && context.AbilityPressed)
        {
            // Cancel flying with ability button
            if (_isFlying)
            {
                StopFlying();
                return true;
            }
        }
        
        return _isFlying || _isTransitioningToFly;
    }
    
    /// <summary>
    /// Process movement for flying
    /// </summary>
    public override bool ProcessMovement(MovementContext context)
    {
        // Only process when flying
        if (!_isFlying && !_isTransitioningToFly)
            return false;
            
        // Get horizontal input
        float moveInput = context.Velocity.x;
        
        // Apply horizontal movement
        Vector2 velocity = context.Velocity;
        velocity.x = moveInput * _flyingHorizontalSpeed;
        
        // If transitioning, don't override vertical velocity
        if (!_isTransitioningToFly)
        {
            // Keep current vertical velocity (affected by flaps)
        }
        
        context.DesiredVelocity = velocity;
        return true;
    }
    
    /// <summary>
    /// Start double jump (transition to flying)
    /// </summary>
    private void StartDoubleJump()
    {
        _hasDoubleJumped = true;
        _isTransitioningToFly = true;
        
        // Apply initial jump force
        Vector2 velocity = _character.Rigidbody.linearVelocity;
        velocity.y = _flyingFlapForce * 0.8f; // Slightly lower than a full flap
        _character.Rigidbody.linearVelocity = velocity;
        
        // Update state for animation
        NotifyStateChanged(MovementStateType.JumpToFly);
        
        // Start coroutine to transition to flying after animation
        MonoBehaviour mono = _character as MonoBehaviour;
        if (mono != null)
        {
            mono.StartCoroutine(TransitionToFlyingAfterDelay());
        }
    }
    
    /// <summary>
    /// Transition to flying after jump-to-fly animation
    /// </summary>
    private IEnumerator TransitionToFlyingAfterDelay()
    {
        yield return new WaitForSeconds(_jumpToFlyDuration);
        
        _isTransitioningToFly = false;
        _isFlying = true;
        _flyingTimeCounter = _flyingMaxDuration;
        
        // Update state for animation
        NotifyStateChanged(MovementStateType.Fly);
        
        // Apply a small upward boost
        Vector2 velocity = _character.Rigidbody.linearVelocity;
        velocity.y = _flyingFlapForce * 0.5f;
        _character.Rigidbody.linearVelocity = velocity;
    }
    
    /// <summary>
    /// Perform a flying flap
    /// </summary>
    private void Flap()
    {
        if (_isFlying)
        {
            // Reset the flying timer
            _flyingTimeCounter = _flyingMaxDuration;
            
            // Apply upward force for flapping
            Vector2 velocity = _character.Rigidbody.linearVelocity;
            velocity.y = _flyingFlapForce;
            _character.Rigidbody.linearVelocity = velocity;
            
            // Animation is already Fly, no need to update
        }
    }
    
    /// <summary>
    /// Update flying timer
    /// </summary>
    private void UpdateFlyingTimer(float deltaTime)
    {
        if (_isFlying && _flyingTimeCounter > 0)
        {
            _flyingTimeCounter -= deltaTime;
            
            // Apply gentle falling after flap boost wears off
            if (_flyingTimeCounter <= 0 && _character.Rigidbody.linearVelocity.y > 0)
            {
                Vector2 velocity = _character.Rigidbody.linearVelocity;
                velocity.y = 0;
                _character.Rigidbody.linearVelocity = velocity;
            }
        }
    }
    
    /// <summary>
    /// Stop flying (transition to fall)
    /// </summary>
    private void StopFlying()
    {
        if (_isFlying)
        {
            _isFlying = false;
            
            // Update state for animation
            NotifyStateChanged(MovementStateType.FlyEnd);
            
            // Start coroutine to transition to falling after animation
            MonoBehaviour mono = _character as MonoBehaviour;
            if (mono != null)
            {
                mono.StartCoroutine(TransitionToFallingAfterDelay());
            }
        }
    }
    
    /// <summary>
    /// Transition to falling after fly-end animation
    /// </summary>
    private IEnumerator TransitionToFallingAfterDelay()
    {
        yield return new WaitForSeconds(0.3f); // Approximate time for FlyEnd animation
        
        NotifyStateChanged(MovementStateType.Fall);
    }
    
    /// <summary>
    /// Reset state when landing
    /// </summary>
    public void OnLanded()
    {
        _isFlying = false;
        _isTransitioningToFly = false;
        _hasDoubleJumped = false;
    }
    
    /// <summary>
    /// Handle initialization
    /// </summary>
    public override void Initialize(CharacterController2D character)
    {
        base.Initialize(character);
        
        // Subscribe to form changes
        if (character is KirbyController kirbyController)
        {
            kirbyController.OnFormChanged += OnFormChanged;
        }
    }
    
    /// <summary>
    /// Handle form changes
    /// </summary>
    private void OnFormChanged(CharacterForm form)
    {
        // If changing to Full form while flying, stop flying
        if (form == CharacterForm.Full && (_isFlying || _isTransitioningToFly))
        {
            StopFlying();
        }
    }
}
