//-----------------------------------------------------------------------
// <copyright file = "CollectionExtension.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for the <see cref="ICollection{T}"/> interface.
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection and the range to add.</typeparam>
        /// <param name="collection">The <see cref="ICollection{T}"/> to add the items to.</param>
        /// <param name="items">The range of items to add to the collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="items"/> is <b>null</b>.
        /// </exception>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if(items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach(var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
