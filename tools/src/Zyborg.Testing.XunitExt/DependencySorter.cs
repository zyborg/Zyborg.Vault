using System;
using System.Collections.Generic;

namespace Zyborg.Testing.XunitExt
{
    internal static class DependencySorter
    {
        /// <summary>
        /// Sorts the specified list based on dependency order
        /// </summary>
        public static IEnumerable<T> Sort<T>(IEnumerable<T> enumerable) where T : class, IDependencyIndicator<T>
        {
            List<T> items = new List<T>(enumerable);

            DependencyChainValidator.Validate(items);

            // For each node, either find the previous or the next node in 
            // the list of dependencies. 
            LinkedList<T> listOfDependencies = new LinkedList<T>();
            foreach (T item in items)
            {
                // Find the dependency for the current node
                bool dependencyOrDependantFound = false;
                LinkedListNode<T> current = listOfDependencies.First;
                do
                {
                    if (current == null) continue;

                    if (item.IsDependencyOf(current.Value))
                    {
                        listOfDependencies.AddBefore(current, item);
                        dependencyOrDependantFound = true;
                        break;
                    }

                    if (current.Value.IsDependencyOf(item))
                    {
                        listOfDependencies.AddAfter(current, item);
                        dependencyOrDependantFound = true;
                        break;
                    }
                } while ((current = current?.Next) != null);

                if (!dependencyOrDependantFound)
                {
                    listOfDependencies.AddFirst(item);
                }
            }

            // At this point we have sorted the list of dependencies 
            // but dependency chains themselves have an undefined order, so:
            // A -> B \-> C    0 -> 1 -> 2
            //         -> D    
            //
            // May be sorted as
            // 0 -> 1 -> 2     A -> B \-> D    
            //                         -> C    
            //
            // Also, items without any dependencies have a undefined position
            // although the will most likely be at the start of the list.

            return listOfDependencies;
        }
    }

    internal interface IDependencyIndicator<T> : IEquatable<T>
    {
        /// <summary>
        /// Returns <c>true</c> if the current instance is a dependency of the other instance
        /// </summary>
        bool IsDependencyOf(T other);

        bool HasDependencies { get; }
    }
}