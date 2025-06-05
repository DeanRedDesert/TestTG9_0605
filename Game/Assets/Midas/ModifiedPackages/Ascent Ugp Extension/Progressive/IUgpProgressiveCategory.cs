//-----------------------------------------------------------------------
// <copyright file = "IUgpProgressiveCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines an interface that allows a package to retrieve information about progressive levels.
    /// </summary>
    public interface IUgpProgressiveCategory
    {
        /// <summary>
        /// Retrieves the progressive level of specified progressive ID.
        /// </summary>
        /// <param name="progressiveId">Specify the progressive ID to get.</param>
        /// <returns>The retrieved progressive level.</returns>
        ProgressiveLevelInfo GetProgressiveLevel(string progressiveId);

        /// <summary>
        /// Retrieves all the progressive levels.
        /// </summary>
        /// <returns>A list of all the progressive levels retrieved.</returns>
        IEnumerable<ProgressiveLevelInfo> GetAllProgressives();
    }
}
