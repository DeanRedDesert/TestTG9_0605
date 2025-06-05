// -----------------------------------------------------------------------
// <copyright file = "IProgressiveBroadcastData.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Interface representing progressive data which would be broadcast
    /// from a progressive controller to the presentation.
    /// </summary>
    public interface IProgressiveBroadcastData
    {
        /// <summary>
        /// Monetary value of a progressive in base units.
        /// </summary>
        long Amount { get; }

        /// <summary>
        /// Description of a progressive's non-monetary prize.
        /// </summary>
        string PrizeString { get; }
    }
}
