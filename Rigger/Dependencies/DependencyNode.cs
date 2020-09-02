using System;
using System.Collections.Generic;
using System.Linq;

namespace Rigger.Dependencies
{
   /// <summary>
   /// A node on the dependency tree.
   /// </summary>
    public class DependencyNode
    {
        /// <summary>
        /// The parent of this node.
        /// </summary>
        public DependencyNode Parent { get; set; }
        /// <summary>
        /// The type that this node is tracking
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// All children of this node.
        /// </summary>
        private readonly List<DependencyNode> _children = new List<DependencyNode>();

       /// <summary>
       /// Remove a child relationship from this parent.
       /// </summary>
       /// <param name="node">The node to remove. This method compares the type that the node contains for equivalence.</param>
       /// <returns>True if the type is found, false if it is not.</returns>
        public bool RemoveChild(DependencyNode node)
        {
            return _children.RemoveAll(p => p.Type == node.Type) > 0;
        }
        
        /// <summary>
        /// Add a dependency node to the tree for the following type.
        /// Returns the node that is created.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DependencyNode AddChild(Type type)
        {

            DependencyNode node = new DependencyNode {Type = type, Parent = this};

            _children.Add( node );

            return node;
        }
        /// <summary>
        /// Add a child node to the tree
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <returns>the node that was passed with a side effect of adding this node as the parent</returns>
        public DependencyNode AddChild(DependencyNode node)
        {
            node.Parent = this;

            _children.Add(node);

            return node;
        }

        /// <summary>
        /// FInd a node by type in the dependency tree using depth first search.
        /// </summary>
        /// <param name="type">The type to find</param>
        /// <returns>The first node that matches or null not found.</returns>
        public DependencyNode FindDependency(Type type)
        {
            return DepthFirst()
                .FirstOrDefault(o => o.Type == type);
        }

        /// <summary>
        /// Flattens this tree with a post-order depth first approach.
        /// </summary>
        /// <returns>A list of nodes in a post-order depth first enumeration</returns>
        public List<DependencyNode> DepthFirst()
        {
            List<DependencyNode> nodes = new List<DependencyNode>();

            _children
                .ForEach(t => nodes.AddRange(t.DepthFirst()));

            nodes.Add(this);

             return nodes;
        }
    }
}