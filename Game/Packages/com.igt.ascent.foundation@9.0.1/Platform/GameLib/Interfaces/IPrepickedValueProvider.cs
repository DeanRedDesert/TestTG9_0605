//-----------------------------------------------------------------------
// <copyright file = "IPrepickedValueProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface used to provide values to use in lieu of a random number generator.
    /// </summary>
    public interface IPrepickedValueProvider
    {
        /// <summary>
        /// Gets a either a collection of values for the given <see cref="RandomValueRequest"/> or a new request that can be
        /// passed to the random number generator.
        /// </summary>
        /// <remarks>
        /// Any pre-picked values contained in the request will appear at the front of the pre-picked values collection.
        /// After that, any additional values required to fill the request that were previously set on this provider will appear.
        /// 
        /// There may not be enough values to fill the request.  In that case, a new request will be provided on the result object.
        /// That new request can be passed on to the random number generator to fill the remaining spots with random values.
        /// </remarks>
        /// <param name="request">The <see cref="RandomValueRequest"/> to be filled.</param>
        /// <returns>A new collection of <see cref="PrepickResult"/> objects.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if any of the provided values are out of range for the original <see cref="RandomValueRequest"/>, or if the number
        /// of duplicates is exceeded.
        /// </exception>
        PrepickResult GetPrepickedValues(RandomValueRequest request);

        /// <summary>
        /// Gets a collection of <see cref="PrepickResult"/> objects, where each result contains either a collection of pre-picked
        /// values or a new request that can be forwarded to the random number generator if there were not enough values in
        /// this provider to fill the request.
        /// </summary>
        /// <param name="requests">A collection of <see cref="RandomValueRequest"/> objects to get values for.</param>
        /// <returns>
        /// A collection of <see cref="PrepickResult"/> objects.
        /// </returns>
        ICollection<PrepickResult> GetPrepickedValues(IEnumerable<RandomValueRequest> requests);

        /// <summary>
        /// Sets the given values on this provider, replacing any values that were previously set.
        /// </summary>
        /// <param name="values">The new set of values to provide.</param>
        void SetValues(IEnumerable<int> values);
    }
}
