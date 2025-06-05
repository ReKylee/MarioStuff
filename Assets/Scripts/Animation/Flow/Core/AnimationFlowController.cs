using System.Collections.Generic;
using Animation.Flow.Interfaces;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Base controller for animation flow system
    ///     Handles execution of behavior trees and parameter management
    /// </summary>
    public abstract class AnimationFlowController : MonoBehaviour
    {
        [SerializeField] protected AnimationFlowAsset _flowAsset;

        // The current animation context
        protected AnimationContext _context;

        // The current animator interface
        protected IAnimator _animator;

        /// <summary>
        ///     The current animation flow asset
        /// </summary>
        public AnimationFlowAsset FlowAsset
        {
            get => _flowAsset;
            protected set => _flowAsset = value;
        }

        /// <summary>
        ///     Initialize the controller
        /// </summary>
        protected virtual void Awake()
        {
            // Create the animator interface
            _animator = CreateAnimator();

            if (_animator == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to create animator interface.", this);
                enabled = false;
                return;
            }

            // Create the animation context
            _context = new AnimationContext(_animator);

            // Initialize nodes if we have a flow asset
            if (_flowAsset != null && _flowAsset.RootNode != null)
            {
                InitializeNodes(_flowAsset.Nodes);
            }
        }

        /// <summary>
        ///     Update the animation state
        /// </summary>
        protected virtual void Update()
        {
            // Skip if we don't have a valid flow asset or root node
            if (_flowAsset == null || _flowAsset.RootNode == null || _context == null)
            {
                return;
            }

            // Execute the behavior tree
            _flowAsset.RootNode.Execute(_context);
        }

        /// <summary>
        ///     Called when the component is validated in the editor
        /// </summary>
        protected virtual void OnValidate()
        {
            // Nothing to do in the base class
        }

        /// <summary>
        ///     Creates the animator interface implementation
        /// </summary>
        /// <returns>An implementation of IAnimator</returns>
        protected abstract IAnimator CreateAnimator();

        /// <summary>
        ///     Sets a boolean parameter in the animation context
        /// </summary>
        public void SetParameter(string name, bool value)
        {
            if (_context != null)
            {
                _context.SetBool(name, value);
            }
        }

        /// <summary>
        ///     Sets an integer parameter in the animation context
        /// </summary>
        public void SetParameter(string name, int value)
        {
            if (_context != null)
            {
                _context.SetInt(name, value);
            }
        }

        /// <summary>
        ///     Sets a float parameter in the animation context
        /// </summary>
        public void SetParameter(string name, float value)
        {
            if (_context != null)
            {
                _context.SetFloat(name, value);
            }
        }

        /// <summary>
        ///     Sets a string parameter in the animation context
        /// </summary>
        public void SetParameter(string name, string value)
        {
            if (_context != null)
            {
                _context.SetString(name, value);
            }
        }

        /// <summary>
        ///     Initialize all nodes in the behavior tree
        /// </summary>
        protected virtual void InitializeNodes(List<FlowNode> nodes)
        {
            // Reset all nodes to their initial state
            foreach (var node in nodes)
            {
                node.Reset();
            }
        }
    }
}
