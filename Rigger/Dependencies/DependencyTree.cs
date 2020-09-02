using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.Dependencies
{
    public class DependencyTree
    {
        /// <summary>
        /// Root of the dependency tree. Not included in DFS as it would be
        /// blank.
        /// </summary>
        private readonly DependencyNode _root = new DependencyNode();

        /// <summary>
        /// Add a dependency to the dependency tree. If that type has a dependsOn attribute
        ///  the tree will be constructed to resolve that dependency first.
        /// </summary>
        public DependencyTree AddDependency(Type dependency)
        {
            BootstrapAttribute attribute = dependency.GetCustomAttribute<BootstrapAttribute>();

            if (attribute != null)
            {
                Type dependsOnType = attribute.DependsOn;

                DependencyNode parentNode = _root.FindDependency(dependency) // in the dependency tree, use that node
                                            ?? _root.AddChild(dependency); // not in the dependency tree, add it to the root

                if (dependsOnType != null)
                {
                    DependencyNode dependencyNode =
                        _root.FindDependency(dependsOnType) ?? new DependencyNode {Type = dependsOnType};

                    if (dependencyNode.Parent == _root)
                    {
                        _root.RemoveChild(dependencyNode);
                    }

                    parentNode.AddChild(dependencyNode);
                }
            }

            return this;
        }

        /// <summary>
        /// Resolve the dependencies in the graph using the
        /// provided resolver.
        /// </summary>
        /// <param name="resolver">Dependency resolver</param>
        public void Resolve(IDependencyResolver resolver)
        {
            _root.DepthFirst()
                .Map(o => o.Type)
                .ForEach(resolver.Resolve);
        }

        /// <summary>
        /// Flatten a dependency tree into a list with a post-order
        /// depth first search.
        /// </summary>
        /// <returns>The dependency nodes in a post-order depth first enumeration.</returns>
        public List<DependencyNode> DepthFirst()
        {
            return _root.DepthFirst();
        }
    }
}