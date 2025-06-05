//-----------------------------------------------------------------------
// <copyright file = "PrepickResult.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A result object returned by a <see cref="IPrepickedValueProvider"/> object.
    /// </summary>
    public class PrepickResult
    {
        /// <summary>
        /// Gets the new request to be forwarded to a random number generator if more
        /// values are needed to satisfy the request, or null if more values are not needed.
        /// </summary>
        public RandomValueRequest NewRandomValueRequest { get; }

        /// <summary>
        /// Gets a collection of pre-picked values if all requested values have been provided,
        /// or null if more values are needed.
        /// </summary>
        public ICollection<int> PrepickedValues { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrepickResult"/> class.
        /// </summary>
        /// <param name="originalRequest">The original <see cref="RandomValueRequest"/> object.</param>
        /// <param name="prepickedValues">
        /// A collection of pre-picked values that were provided to fill the original request.  There may be fewer values
        /// than were requested, but all values present must be within range for the request.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if any of the provided values are out of range for the original <see cref="RandomValueRequest"/>, or if the number
        /// of duplicates is exceeded.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
        public PrepickResult(RandomValueRequest originalRequest, ICollection<int> prepickedValues)
        {
            if(originalRequest == null)
            {
                throw new ArgumentNullException(nameof(originalRequest));
            }
            if(prepickedValues == null)
            {
                throw new ArgumentNullException(nameof(prepickedValues));
            }

            if(prepickedValues.Count == originalRequest.Count)
            {
                PrepickedValues = prepickedValues;
                NewRandomValueRequest = null;
            }
            else
            {
                PrepickedValues = null;
                NewRandomValueRequest = new RandomValueRequest(originalRequest.Count,
                                                               originalRequest.RangeMin,
                                                               originalRequest.RangeMax,
                                                               originalRequest.MaxDuplicates,
                                                               prepickedValues,
                                                               originalRequest.GeneratorName);
            }
        }
    }
}
