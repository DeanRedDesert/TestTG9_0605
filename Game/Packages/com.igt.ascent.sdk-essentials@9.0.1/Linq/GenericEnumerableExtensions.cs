//-----------------------------------------------------------------------
// <copyright file = "GenericEnumerableExtensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for the <see cref="IEnumerable{T}"/> type that are useful in LINQ queries.
    /// </summary>
    /// <remarks>
    /// In order to be useful for LINQ, the methods in this class should operate on objects of type
    /// <see cref="IEnumerable{T}"/> and also return objects of type <see cref="IEnumerable{T}"/>.
    /// 
    /// This allows them to be used with the LINQ standard query operators.
    /// </remarks>
    public static class GenericEnumerableExtensions
    {
        /// <summary>
        /// Gets the next permutation of a set using a lexicographical permutation algorithm.
        /// </summary>
        /// <param name="set">A set of <typeparam name="T"/> objects.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare the objects.</param>
        /// <returns>Either the next permutation of the set or <see langword="null"/> if there are no more permutations.</returns>
        /// <remarks>
        /// The lexicographical algorithm employed runs through the permutations by comparing the string elements of the set.
        /// This means that the first permutation is defined by the set containing all of the strings sorted in ascending order.
        /// In order to obtain all of the permutations of the set, you need to sort the set before calling this function the
        /// first time.
        /// 
        /// The algorithm is described in the Wikipedia article for "Permutation".
        /// </remarks>
        public static IEnumerable<T> NextPermutation<T>(this IEnumerable<T> set, IComparer<T> comparer)
        {
            // copy the set to a list
            var list = set.ToList();

            // find the largest index k for an item that is smaller than the next item
            var k = list.Count - 2;
            while(k >= 0 && comparer.Compare(list[k], list[k + 1]) >= 0)
            {
                k--;
            }

            // if there is no item that is smaller than the next item there are no more permutations
            if(k == -1)
            {
                return null;
            }

            // find the largest index l such that the item at index k is smaller than the item at the index
            var l = k + 1;
            for(var i = l + 1; i < list.Count; i++)
            {
                if(comparer.Compare(list[k], list[i]) < 0)
                {
                    l = i;
                }
            }

            // swap list[k] with list[l]
            var temp = list[l];
            list[l] = list[k];
            list[k] = temp;

            // reverse the elements from k + 1 on
            list.Reverse(k + 1, list.Count - (k + 1));

            // return the list
            return list;
        }

        /// <summary>
        /// Gets all of the possible permutations of the given set using the provided comparison method.
        /// </summary>
        /// <typeparam name="T">The type of objects contained in the set.</typeparam>
        /// <param name="set">A <see cref="IEnumerable{T}"/> containing a set of objects.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> to use when comparing elements in the set.</param>
        /// <returns>A collection containing all of the permutations of the set.</returns>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> set, IComparer<T> comparer)
        {
            var permutation = (IEnumerable<T>)set.OrderBy(x => x, comparer);
            do
            {
                var current = permutation as T[] ?? permutation.ToArray();
                yield return current;
                permutation = current.NextPermutation(comparer);
            } while(permutation != null);
        }

        /// <summary>
        /// Gets all of the possible permutations of the given set using the default comparison method.
        /// </summary>
        /// <typeparam name="T">The type of objects contained in the set.</typeparam>
        /// <param name="set">A <see cref="IEnumerable{T}"/> containing a set of objects.</param>
        /// <returns>A collection containing all of the permutations of the set.</returns>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> set)
        {
            return set.Permutations(Comparer<T>.Default);
        }

        /// <summary>
        /// Allows for quick iteration over <see name="IEnumerable" />.
        /// </summary>
        /// <typeparam name="T">The enumerable type.</typeparam>
        /// <param name="source">Source to iterate across.</param>
        /// <param name="action">Delegate to call on each element in the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="source"/> is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if(action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach(var element in source)
            {
                action(element);
            }
        }
    }
}
