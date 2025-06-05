//-----------------------------------------------------------------------
// <copyright file = "LambdaComparer.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Generic class to allow for Lambda compares to be used with LINQ for functions like Distinct.
    /// </summary>
    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// Comparison function.
        /// </summary>
        private readonly Func<T, T, bool> lambdaComparer;

        /// <summary>
        /// Hash function.
        /// </summary>
        private readonly Func<T, int> lambdaHash;

        /// <summary>
        /// Constructor for Lambda comparer that only has comparison and no hash.
        /// </summary>
        /// <param name="lambdaComparer">The comparison function.</param>
        public LambdaComparer(Func<T, T, bool> lambdaComparer) :
            this(lambdaComparer, obj => 0) {}

        /// <summary>
        /// Constructor for Lambda comparer that has both comparison and hash.
        /// </summary>
        /// <param name="lambdaComparer">The equality function.</param>
        /// <param name="lambdaHash">The hash function.</param>
        /// <exception cref="ArgumentNullException">Thrown if lambdaComparer or lambdaHash is null.</exception>
        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            this.lambdaComparer = lambdaComparer ?? throw new ArgumentNullException(nameof(lambdaComparer));
            this.lambdaHash = lambdaHash ?? throw new ArgumentNullException(nameof(lambdaHash));
        }

        /// <summary>
        /// Equals implementation to use function passed in at creation.
        /// </summary>
        /// <param name="object1">First object for comparison.</param>
        /// <param name="object2">Second object for comparison.</param>
        /// <returns>Boolean result of lambdaComparer.</returns>
        public bool Equals(T object1, T object2)
        {
            return lambdaComparer(object1, object2);
        }

        /// <summary>
        /// Implements Hash function based on hash function passed into constructor.
        /// </summary>
        /// <param name="obj">Object to calculate hash for.</param>
        /// <returns>Integer result of lambdaHash.</returns>
        public int GetHashCode(T obj)
        {
            return lambdaHash(obj);
        }
    }
}
