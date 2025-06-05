//-----------------------------------------------------------------------
// <copyright file = "WayTree.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// Tracks ways and performs lookups of exact matches, subsets, and supersets of a given way.
    /// </summary>
    /// <remarks>
    /// The <see cref="WayTree"/> is a modified radix tree or "trie" (think retrieval.) This implementation
    /// was derived from an implementation in the book "Algorithms, 4th ed." by Robert Sedgewick and Kevin Wayne.
    /// 
    /// The modifications disallow what would typically be substring or superstring keys to be stored in the same
    /// tree that already contains a given key. This is how the data structure supports detection of overlapping
    /// ways.
    /// </remarks>
    public class WayTree
    {
        private readonly int columnCount;
        private readonly int radix;
        private Node root;

        /// <summary>
        /// Represents a single node in a <see cref="WayTree"/>. Each node in the tree represents a <see cref="Way"/> in 
        /// a location that indicates how this way is related to the other ways in the tree.
        /// </summary>
        private class Node
        {
            /// <summary>
            /// An array of nodes where each slot represents a path to the child at that position.
            /// </summary>
            /// <remarks>
            /// Since each level of the tree represents a column in the symbol window, the node at
            /// position i represents the way passing through position (i + 1) in the column. The index
            /// 0 represents a way that does not have any symbols in this column (i.e. the way skips over
            /// this column.)
            /// </remarks>
            public Node[] Next { get; private set; }

            /// <summary>
            /// Initializes a new <see cref="Node"/>.
            /// </summary>
            /// <param name="tree">The <see cref="WayTree"/> that this node belongs to.</param>
            public Node(WayTree tree)
            {
                Next = new Node[tree.radix];
            }
        }

        /// <summary>
        /// Initializes a new way tree that can handle symbol windows with the given number of rows and columns.
        /// </summary>
        /// <param name="rowCount">The maximum number of rows in the symbol window.</param>
        /// <param name="columnCount">The maximum number of columns in the symbol window.</param>
        public WayTree(int rowCount, int columnCount)
        {
            radix = rowCount + 1; // one more to track ways with no cells in a given column
            this.columnCount = columnCount;
        }

        /// <summary>
        /// Attempts to add a new <see cref="Way"/> to the tree.
        /// </summary>
        /// <param name="way">The <see cref="Way"/> to add.</param>
        /// <returns>
        /// A <see cref="bool"/> that is <b>true</b> if the way was added and <b>false</b> otherwise.
        /// </returns>
        /// <remarks>
        /// If the tree already contains the way, or a subset or superset of the way, this method will not
        /// allow the way to be added.
        /// </remarks>
        public bool Add(Way way)
        {
            var added = false;
            var path = GetPath(way);
            var found = WayExists(root, path, 0) || SubsetExists(root, path, 0) || SupersetExists(root, path, 0);
            if(!found)
            {
                root = Add(root, path, 0);
                added = true;
            }
            return added;
        }

        /// <summary>
        /// Removes the given <see cref="Way"/> from the tree if it exists in the tree.
        /// </summary>
        /// <param name="way">The <see cref="Way"/> to remove.</param>
        /// <returns>
        /// A <see cref="bool"/> that is <b>true</b> if the way was removed and <b>false</b> if it was not removed.
        /// </returns>
        public bool Remove(Way way)
        {
            var removed = false;
            var path = GetPath(way);
            root = Remove(root, path, 0, ref removed);
            return removed;
        }

        /// <summary>
        /// Gets the path that indicates where in the tree the given <see cref="Way"/> would be/is stored.
        /// </summary>
        /// <param name="way">A <see cref="Way"/> object.</param>
        /// <returns>A list of integers, each representing a segment of the path.</returns>
        private IList<int> GetPath(Way way)
        {
            var path = new int[columnCount]; // columns without a cell are initialized to 0
            foreach(var outcomeCell in way.Cells)
            {
                // offset row index by 1 because we're tracking empty cells with 0
                path[(int)outcomeCell.Cell.column] = (int)outcomeCell.Cell.row + 1;
            }
            return path;
        }

        /// <summary>
        /// Adds the given <see cref="Way"/> to the tree at the position indicated by the path.
        /// </summary>
        /// <param name="current">The <see cref="Node"/> for the current position in the traversal.</param>
        /// <param name="path">The path indicating where the way should be added.</param>
        /// <param name="pathIndex">The current position in the traversal.</param>
        /// <returns>
        /// Returns either the current node or the newly added node if the current node is <b>null</b>.
        /// </returns>
        private Node Add(Node current, IList<int> path, int pathIndex)
        {
            if(current == null)
            {
                current = new Node(this);
            }
            if(path.Count == pathIndex)
            {
                return current;
            }
            var radixIndex = path[pathIndex];
            current.Next[radixIndex] = Add(current.Next[radixIndex], path, pathIndex + 1);
            return current;
        }

        /// <summary>
        /// Removes the <see cref="Way"/> stored at the given path in the tree.
        /// </summary>
        /// <param name="current">The node for the current position in the traversal.</param>
        /// <param name="path">The path indicating where the way to remove is stored.</param>
        /// <param name="pathIndex">The current position in the traversal.</param>
        /// <param name="removed">A flag which will be set to <b>true</b> if the way is removed.</param>
        /// <returns>
        /// Returns the current node if it has any non-null children or if it contains a way, or <b>null</b>
        /// if the current node has all <b>null</b> children and does not contain a <see cref="Way"/>.
        /// </returns>
        private static Node Remove(Node current, IList<int> path, int pathIndex, ref bool removed)
        {
            if (current == null)
            {
                return null;
            }
            if (pathIndex == path.Count)
            {
                removed = true;
            }
            else
            {
                var i = path[pathIndex];
                current.Next[i] = Remove(current.Next[i], path, pathIndex + 1, ref removed);
            }

            /* In a normal trie implementation we would also need to consider the case where the
             trie contains an element that is a substring of the one being removed. Since we do
             not allow subset/superset ways to be added, this case is not required.*/
            return current.Next.Any(t => t != null) ? current : null;
        }

        /// <summary>
        /// Searches the tree for a node at the given path.
        /// </summary>
        /// <param name="current">The node for the current position in the traversal.</param>
        /// <param name="path">The path indicating where to search.</param>
        /// <param name="pathIndex">The current position in the traversal.</param>
        /// <returns>
        /// Returns the node if it is found, or <b>null</b> if no node is stored at that location,
        /// or the node has no <see cref="Way"/> associated with it.
        /// </returns>
        /// <devdoc>
        /// This method has been optimized by unrolling the recursion into a while loop, since the
        /// recursive call was the last statement in the method.
        /// </devdoc>
        private static bool WayExists(Node current, IList<int> path, int pathIndex)
        {
            // tail call recursion optimized
            while(true)
            {
                if(current == null)
                {
                    return false;
                }
                if(path.Count == pathIndex)
                {
                    return true;
                }
                current = current.Next[path[pathIndex]];
                ++pathIndex;
            }
        }

        /// <summary>
        /// Performs a subset search, returning the first node that contains a way that is a
        /// subset of the way that would be stored at the given path.
        /// </summary>
        /// <param name="current">The node for the current position in the traversal.</param>
        /// <param name="path">The path indicating where to search.</param>
        /// <param name="pathIndex">The current position in the traversal.</param>
        /// <returns>The node of the first subset way, or <b>null</b> if no subset way exists.</returns>
        private static bool SubsetExists(Node current, IList<int> path, int pathIndex)
        {
            if(current == null)
            {
                return false;
            }
            if(path.Count == pathIndex)
            {
                return true;
            }
            var exists = SubsetExists(current.Next[path[pathIndex]], path, pathIndex + 1);
            if (!exists && path[pathIndex] > 0)
            {
                exists = SubsetExists(current.Next[0], path, pathIndex + 1);
            }
            return exists;
        }

        /// <summary>
        /// Performs a superset search, returning the first node that contains a way that is a
        /// superset of the way that would be stored at the given path.
        /// </summary>
        /// <param name="current">The node for the current position in the traversal.</param>
        /// <param name="path">The path indicating where to search.</param>
        /// <param name="pathIndex">The current position in the traversal.</param>
        /// <returns>The node of the first superset way, or <b>null</b> if no superset way exists.</returns>
        /// <devdoc>
        /// This method has been optimized by unrolling the recursion into a while loop, since the
        /// recursive call was the last statement in the method.
        /// </devdoc>
        private static bool SupersetExists(Node current, IList<int> path, int pathIndex)
        {
            // tail call recursion optimized
            while (true)
            {
                if(current == null)
                {
                    return false;
                }
                if(path.Count == pathIndex)
                {
                    return true;
                }
                if(path[pathIndex] == 0)
                {
                    var exists = false;
                    foreach(var nextNode in current.Next)
                    {
                        exists = SupersetExists(nextNode, path, pathIndex + 1);
                        if(exists)
                        {
                            break;
                        }
                    }
                    return exists;
                }
                current = current.Next[path[pathIndex]];
                ++pathIndex;
            }
        }
    }
}
