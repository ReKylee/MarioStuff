using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Base class for all nodes in the animation flow behavior tree
    /// </summary>
    [Serializable]
    public abstract class FlowNode : ScriptableObject
    {
        [SerializeField] protected string _name;
        [SerializeField] protected string _description;
        [SerializeField] protected string _guid;
        [SerializeField] protected Vector2 _position;

        /// <summary>
        ///     Unique identifier for this node
        /// </summary>
        public string Guid => _guid;

        /// <summary>
        ///     Name of this node
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     Description of this node
        /// </summary>
        public string Description => _description;

        /// <summary>
        ///     Position of this node in the editor
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public virtual void Reset()
        {
            // Default implementation does nothing
        }

        /// <summary>
        ///     Initialize the node with a new GUID
        /// </summary>
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = GUID.Generate().ToString();
            }
        }

        /// <summary>
        ///     Executes the node's behavior and returns its status
        /// </summary>
        /// <param name="context">Current animation context</param>
        /// <returns>The execution status of the node</returns>
        public abstract NodeStatus Execute(AnimationContext context);

        /// <summary>
        ///     Creates a clone of this node
        /// </summary>
        public virtual FlowNode Clone()
        {
            FlowNode clone = Instantiate(this);
            clone._guid = GUID.Generate().ToString();
            return clone;
        }

        /// <summary>
        ///     Gets all children of this node
        /// </summary>
        /// <returns>A list of all child nodes</returns>
        public virtual List<FlowNode> GetChildren() => new();
    }

    /// <summary>
    ///     The execution status of a node
    /// </summary>
    public enum NodeStatus
    {
        Success,
        Failure,
        Running
    }
}
