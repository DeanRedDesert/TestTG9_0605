//-----------------------------------------------------------------------
// <copyright file = "InfinitePickExtension.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Extension class to add an infinite pick sequence.
    /// </summary>
    public static class InfinitePickExtension
    {
        /// <summary>
        /// Creates an infinite pick sequence. Each item will not be a repeat
        /// of any of the adjacent items as long as the collection has at least
        /// three items. All the items will be used up before an item is repeated.
        /// </summary>
        /// <typeparam name="TObject">The type of the collection.</typeparam>
        /// <param name="collection">The collection to create the sequence from.</param>
        /// <returns>
        /// An infinitely enumerable sequence as long as the collection has at least one item.
        /// </returns>
        /// <remarks>
        /// The enumerator for this sequence will run forever so be very careful using
        /// a foreach loop to enumerate of the collection.
        /// </remarks>
        /// <example>
        /// var list = new List&lt;int&gt; { 1, 2, 3, 4, 5, 6, 7 };
        /// var sequence = list.InfinitePickSequence().GetEnumerator();
        /// sequence.MoveNext();
        /// var item = sequence.Current;
        /// </example>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<TObject> InfinitePickSequence<TObject>(
            this IEnumerable<TObject> collection)
        {
            var pickedItems = new List<TObject>();
            var exhausted = false;
            var rng = new Random();
            TObject lastPick = default;
            var lastPickHasValue = false;

            bool ExhaustiveSearch(TObject obj) => QuickPickExtension.ExhaustiveSearch(collection, pickedItems, obj);

            Func<TObject, bool> ExhaustiveSearchNonRepeating(TObject pick) =>
                obj =>
                    {
                        var collectionItemsAreAllEqual = collection.All(equalObj => equalObj.Equals(obj));

                        return collection.Count(collectionObj => collectionObj.Equals(obj)) !=
                                        pickedItems.Count(pickedObj => pickedObj.Equals(obj)) &&
                               (!obj.Equals(pick) || collectionItemsAreAllEqual || collection.Count() == 1);
                    };

            while(true)
            {
                TObject nextObj;
                do
                {
                    if(!collection.Any())
                    {
                        yield break;
                    }

                    var searchFunction = 
                        lastPickHasValue && exhausted
                            ? ExhaustiveSearchNonRepeating(lastPick) 
                            : ExhaustiveSearch;
                    nextObj = collection.OrderBy(x => rng.Next()).First(searchFunction);
                    pickedItems.Add(nextObj);

                    yield return nextObj;
                    exhausted = collection.All(obj => !ExhaustiveSearch(obj));
                } while(!exhausted);

                lastPick = nextObj;
                lastPickHasValue = true;

                pickedItems.Clear();
            }
        }
    }
}
