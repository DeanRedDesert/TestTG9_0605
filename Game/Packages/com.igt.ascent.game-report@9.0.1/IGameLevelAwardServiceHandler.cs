// -----------------------------------------------------------------------
// <copyright file = "IGameLevelAwardServiceHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;

    /// <summary>
    /// This interface defines the service handler for Game Level Award reporting service.
    /// </summary>
    public interface IGameLevelAwardServiceHandler : IReportResourceCleanUp
    {
        /// <summary>
        /// Initialize the game level progressive data of the specific theme for subsequent queries from Foundation.
        /// This callback is invoked with an open transaction.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="paytableDenominationInfos">The paytable-denomination pairs.</param>
        void InitializeForGameLevelQuery(string themeIdentifier,
            IEnumerable<PaytableDenominationInfo> paytableDenominationInfos);

        /// <summary>
        /// Acquire the game level progressive data for the specific theme.
        /// This callback is invoked WITHOUT an open transaction.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="rawProgressiveData">The raw progressive data per paytable-denomination.</param>
        /// <returns>
        /// The adjusted game-level progressive data per paytable-denomination.
        /// </returns>
        /// <remarks>
        /// DO NOT call any method that requires an open transaction in this method.
        /// </remarks>
        IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> GetThemeBasedGameLevelValues(
            string themeIdentifier,
            IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> rawProgressiveData);

        /// <summary>
        /// Clean up the out-dated game progressive level data when switching to a new game context.
        /// </summary>
        void CleanUpData();
    }
}