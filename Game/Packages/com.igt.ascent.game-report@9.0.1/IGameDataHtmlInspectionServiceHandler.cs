// -----------------------------------------------------------------------
// <copyright file = "IGameDataHtmlInspectionServiceHandler.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using Interfaces;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;

    /// <summary>
    /// This interface defines the service handler for the Html Game Data Inspection reporting service.
    /// </summary>
    public interface IGameDataHtmlInspectionServiceHandler : IReportResourceCleanUp
    {
        /// <summary>
        /// Generates the report for the Html Game Data Inspection reporting service.
        /// </summary>
        /// <param name="culture">The specific culture to generate this report for.</param>
        /// <param name="gameReportContextInformation">
        /// A dictionary of theme IDs vs. a list of <see cref="PaytableDenominationInfo"/> .
        /// </param>
        /// <returns>
        /// A <see cref="IGameHtmlInspectionReport"/> containing Html report info.
        /// </returns>
        IGameHtmlInspectionReport GenerateHtmlInspectionReport(string culture, IDictionary<string, IList<PaytableDenominationInfo>> gameReportContextInformation);

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