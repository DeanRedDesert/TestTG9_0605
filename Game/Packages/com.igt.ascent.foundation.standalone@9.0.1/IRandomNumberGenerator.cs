//-----------------------------------------------------------------------
// <copyright file = "IRandomNumberGenerator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An interface for controlling random number generation.
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Cycle the Rng a random number of times.
        /// </summary>
        void Cycle();

        /// <summary>
        /// Reseed the random number generator using the chaos method.
        /// </summary>
        void ReseedWithChaos();

        /// <summary>
        /// Get a set of random numbers with a single request.
        /// </summary>
        /// <param name="request">
        /// A request specifying the counts and ranges of the random numbers requested.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the passed request is null.
        /// </exception>
        /// <returns>The list of random numbers as requested.</returns>
        ICollection<int> GetRandomNumbers(RandomValueRequest request);
    }
}