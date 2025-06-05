//-----------------------------------------------------------------------
// <copyright file = "QuickPickExtension.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Adds random exhaustive extensions to any IEnumerable.
    /// </summary>
    public static class QuickPickExtension
    {
        /// <summary>
        /// Returns an enumeration with the elements in the collection in random order.
        /// If the collection has more than 1 item, no two adjacent items will be the same.
        /// This function should NOT be used for evaluation.
        /// </summary>
        /// <typeparam name="TObject">The type of the collection.</typeparam>
        /// <param name="collection">The collection to use.</param>
        /// <returns>
        /// A sequence with the elements from the <paramref name="collection"/> randomized.
        /// </returns>
        public static IEnumerable<TObject> QuickPickSequence<TObject>(this IEnumerable<TObject> collection)
        {
            var enumerable = collection as IList<TObject> ?? collection.ToList();
            return QuickPickSequence(enumerable, enumerable.Count);
        }

        /// <summary>
        /// Returns an enumeration with the elements in the collection in random order.
        /// If the collection has more than 1 item, no two adjacent items will be the same.
        /// This function should NOT be used for evaluation.
        /// </summary>
        /// <typeparam name="TObject">The type of the collection.</typeparam>
        /// <param name="collection">The collection to use.</param>
        /// <param name="numberOfItemsToReturn">The number of items to place in the sequence.</param>
        /// <returns>
        /// A sequence with the elements from the <paramref name="collection"/> randomized.
        /// </returns>
        public static IEnumerable<TObject> QuickPickSequence<TObject>(this IEnumerable<TObject> collection, int numberOfItemsToReturn)
        {
            var pickedItems = new List<TObject>();
            var rng = new Random();
            var itemsReturned = 0;

            var enumerable = collection as IList<TObject> ?? collection.ToList();

            // If the collection is empty don't do anything.
            if(!enumerable.Any())
            {
                yield break;
            }

            while(itemsReturned < numberOfItemsToReturn)
            {
                bool exhausted;
                do
                {
                    var nextObj = enumerable.OrderBy(x => rng.Next())
                        .First(obj => ExhaustiveSearch(enumerable, pickedItems, obj));
                    pickedItems.Add(nextObj);
                    itemsReturned++;

                    yield return nextObj;
                    exhausted = enumerable
                        .All(obj => !ExhaustiveSearch(enumerable, pickedItems, obj));
                } while(!exhausted && itemsReturned < numberOfItemsToReturn);

                pickedItems.Clear();
            }
        }

        /// <summary>
        /// Preforms an search on a collection against a list of picked items
        /// and returns true if <paramref name="obj"/> has not been picked.
        /// </summary>
        /// <typeparam name="TObject">The type of the collection.</typeparam>
        /// <param name="collection">The list of items.</param>
        /// <param name="pickedItems">The list of picked items.</param>
        /// <param name="obj">The item to search for.</param>
        /// <returns>
        /// True if <paramref name="pickedItems"/> does not contain <paramref name="obj"/>.
        /// </returns>
        internal static bool ExhaustiveSearch<TObject>(IEnumerable<TObject> collection,
            IList<TObject> pickedItems, TObject obj)
        {
            return collection.Count(collectionObj => collectionObj.Equals(obj))
                != pickedItems.Count(pickedObj => pickedObj.Equals(obj));
        }
    }
}
