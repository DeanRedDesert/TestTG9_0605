//-----------------------------------------------------------------------
// <copyright file = "IRandomNumbers.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// 
    /// </summary>
    public interface IRandomNumbers
    {
        /// <summary>
        /// Get a set of random numbers with a single request.
        /// </summary>
        /// <param name="request">
        /// A request specifying the counts and ranges of the random numbers requested.
        /// </param>
        /// <returns>The list of random numbers as requested.</returns>
        ICollection<int> GetRandomNumbers(RandomValueRequest request);

        /// <summary>
        /// Get a set of random numbers with a list of requests.
        /// </summary>
        /// <param name="requestList">
        /// A list of requests specifying the counts and ranges of the random
        /// numbers requested.  For instance, the client may request 3 values
        /// with ranges of 1-30 and 2 values with ranges of 5-10 in the same call.
        /// </param>
        /// <returns>The list of random numbers as requested.</returns>
        ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList);
    }
}
