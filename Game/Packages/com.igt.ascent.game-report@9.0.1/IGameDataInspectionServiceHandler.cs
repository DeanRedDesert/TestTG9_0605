// -----------------------------------------------------------------------
// <copyright file = "IGameDataInspectionServiceHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the service handler for Game Data Inspection reporting service.
    /// </summary>
    public interface IGameDataInspectionServiceHandler : IReportResourceCleanUp
    {
        /// <summary>
        /// Generates the report for Game Data Inspection reporting service.
        /// </summary>
        /// <param name="themeIdentifier">
        /// Identifier of theme used for generating the report.
        /// </param>
        /// <param name="paytableIdentifier">
        /// Identifier of paytable used for generating the report.
        /// </param>
        /// <param name="denomination">
        /// Denomination used for generating the report.
        /// </param>
        /// <param name="culture">
        /// Culture/language used for formatting report text.
        /// </param>
        /// <returns>
        /// Interface of Game Data Inspection report.
        /// </returns>
        IGameInspectionReport GenerateInspectionReport(string themeIdentifier, string paytableIdentifier,
                                                       long denomination, string culture);

        /// <summary>
        /// Gets the minimum player-wagerable credit balances, in base units, required to commit a new game-cycle
        /// for each of the given combinations of the theme, payvar and denomination.
        /// </summary>
        /// <remarks>
        /// The minimum player-wagerable credit balance should take into considerations of all current configuration
        /// settings including theme-specific custom configurations items, minimum selectable lines, etc.
        /// However it should NOT consider configuration setting related to Credit Playoff, including whether or not
        /// Credit Playoff is enabled.
        /// </remarks>
        /// <param name="requests">
        /// List of minimum playable credit balance requests.  Each request has an output field
        /// to hold the result of the minimum player-wagerable credit balance determined.
        /// </param>
        void GetMinPlayableCreditBalances(IList<MinPlayableCreditBalanceRequest> requests);
    }
}