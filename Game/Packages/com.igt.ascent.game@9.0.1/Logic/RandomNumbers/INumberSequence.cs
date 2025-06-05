//-----------------------------------------------------------------------
// <copyright file = "INumberSequence.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface that provides support for inserting number sequences that may be used to fill random value requests.
    /// </summary>
    public interface INumberSequence
    {
        /// <summary>
        /// Add a sequence of numbers from which random value requests will be filled until the sequence has been 
        /// exhausted.
        /// </summary>
        /// <param name="sequence">Sequence to pull random numbers from.</param>
        /// <remarks>This function will extend any existing numbers already added to the sequence.</remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sequence"/> is <b>null</b>.
        /// </exception>
        void AddNumberSequence(IEnumerable<int> sequence);
    }
}