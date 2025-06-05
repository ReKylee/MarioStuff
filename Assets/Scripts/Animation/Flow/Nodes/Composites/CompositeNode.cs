using System.Collections.Generic;
using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Composites
{
    /// <summary>
    ///     Base class for composite nodes that have multiple children
    /// </summary>
    public abstract class CompositeNode : FlowNode
    {
        [SerializeField] protected List<FlowNode> _children = new();

        /// <summary>
        ///     Gets the list of child nodes
        /// </summary>
        public List<FlowNode> Children => _children;

        /// <summary>
        ///     Adds a child node
        /// </summary>
        public void AddChild(FlowNode child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }

        /// <summary>
        ///     Removes a child node
        /// </summary>
        public void RemoveChild(FlowNode child)
        {
            _children.Remove(child);
        }

        /// <summary>
        ///     Gets all children of this node
        /// </summary>
        public override List<FlowNode> GetChildren()
        {
            return new List<FlowNode>(_children);
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            // Reset all children
            foreach (var child in _children)
            {
                child.Reset();
            }
        }

        /// <summary>
        ///     Creates a clone of this node
        /// </summary>
        public override FlowNode Clone()
        {
            var clone = (CompositeNode)base.Clone();
            clone._children = new List<FlowNode>();

            // Clone all children
            foreach (var child in _children)
            {
                var childClone = child.Clone();
                clone.AddChild(childClone);
            }

            return clone;
        }
    }
}
