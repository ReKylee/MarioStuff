using System.Collections.Generic;
using Animation.Flow.Core;
using UnityEngine;

namespace Animation.Flow.Nodes.Decorators
{
    /// <summary>
    ///     Base class for decorator nodes that have a single child
    /// </summary>
    public abstract class DecoratorNode : FlowNode
    {
        [SerializeField] protected FlowNode _child;

        /// <summary>
        ///     Gets or sets the child node
        /// </summary>
        public FlowNode Child
        {
            get => _child;
            set => _child = value;
        }

        /// <summary>
        ///     Gets all children of this node
        /// </summary>
        public override List<FlowNode> GetChildren()
        {
            var children = new List<FlowNode>();
            if (_child != null)
            {
                children.Add(_child);
            }
            return children;
        }

        /// <summary>
        ///     Resets the node to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _child?.Reset();
        }

        /// <summary>
        ///     Creates a clone of this node
        /// </summary>
        public override FlowNode Clone()
        {
            var clone = (DecoratorNode)base.Clone();

            // Clone child if it exists
            if (_child != null)
            {
                clone._child = _child.Clone();
            }

            return clone;
        }
    }
}
