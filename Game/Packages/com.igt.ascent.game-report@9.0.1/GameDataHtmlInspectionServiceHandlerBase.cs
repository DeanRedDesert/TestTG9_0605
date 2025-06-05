// -----------------------------------------------------------------------
// <copyright file = "GameDataHtmlInspectionServiceHandlerBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using Logic.PaytableLoader.Interfaces;
    using Money;

    /// <summary>
    /// The implementation of <see cref="IGameDataHtmlInspectionServiceHandler"/>.
    /// </summary>
    /// <remarks>
    /// A custom game report needs to implement this abstract class if it will
    /// support the <see cref="ReportingServiceType.GameDataInspection"/>.
    /// </remarks>
    public abstract class GameDataHtmlInspectionServiceHandlerBase : IGameDataHtmlInspectionServiceHandler
    {
        /// <summary>
        /// Gets and sets the <see cref="IReportLib"/>interface.
        /// </summary>
        protected IReportLib ReportLib { get; }

        /// <summary>
        /// Gets/sets the paytable loader used to load format specific paytable data.
        /// </summary>
        protected IPaytableLoader PaytableLoader { get; }

        #region Abstract/Virtual Members

        /// <inheritdoc />
        public abstract void CleanUpResources(IReportLib reportLib);

        /// <summary>
        /// When implemented in a derived class, generates the actual Html to be used in a report.
        /// </summary>
        /// <param name="reportContexts">
        /// A list of <see cref="ReportContext"/> objects containing payvar and denom combinations for that theme.
        /// </param>
        /// <remarks>
        /// The <see cref="ReportContext.ThemeIdentifier"/> and <see cref="PaytableTag.PaytableIdentifier"/> values (for example, '020-00101S.AVV046811')
        /// included in <see cref="ReportContext"/> are sent from the Foundation and supposed to be used for querying information with Foundation only.
        /// They should not be displayed in any report. To identify a theme or a paytable in the report, please use the appropriate information pulled
        /// from the paytable and <see cref="IReportLib"/>.
        /// </remarks>
        /// <returns>
        /// The complete report in a single string format.
        /// </returns>
        protected abstract string CreateHtmlReport(IList<ReportContext> reportContexts);

        /// <summary>
        /// Gets the minimum playable credit balances, in base units, for all the given report contexts.
        /// </summary>
        /// <remarks>
        /// Default implementation is to return the multiplication of the denomination by the min bet credits in
        /// the generic paytable data (<see cref="IGenericPaytableData.MinBetCredits"/>) for each given
        /// <see cref="ReportContext"/>.
        /// 
        /// If desired, derived classes could override this method to return other appropriate values.
        ///
        /// Note that whether the generic paytable data has a valid MinBetCredits value is determined
        /// by the <see cref="IPaytableLoader"/> object and the paytable format used by the game.
        /// It is the  game's responsibility to ensure that this method is properly overridden as needed
        /// so that correct values are returned.
        /// </remarks>
        /// <param name="reportContexts">
        /// A list of <see cref="ReportContext"/> objects containing payvar and denom combinations for that theme.
        /// </param>
        /// <returns>
        /// The list of determined minimum playable credit balances (in base units), each corresponding to
        /// the item in the <paramref name="reportContexts"/> list of the same index.
        /// For example, returned[0] is the result for reportContexts[0], returned[1] is for reportContexts[1],
        /// and so on.
        /// </returns>
        protected virtual IList<long> GetAllMinPlayableCreditBalances(IList<ReportContext> reportContexts)
        {
            checked
            {
                return reportContexts.Select(context => context.PaytableData.MinBetCredits * context.Denomination)
                                     .ToList();
            }
        }

        #endregion

        /// <summary>
        /// Constructs the html report generation service handler.
        /// </summary>
        /// <param name="reportLib">
        /// The game report interface to the Foundation.
        /// </param>
        /// <param name="paytableLoader">
        /// The paytable loader interface that loads a specific type of paytable file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reportLib"/> is null.
        /// </exception>
        protected GameDataHtmlInspectionServiceHandlerBase(IReportLib reportLib, IPaytableLoader paytableLoader)
        {
            ReportLib = reportLib ?? throw new ArgumentNullException(nameof(reportLib));
            PaytableLoader = paytableLoader ?? throw new ArgumentNullException(nameof(paytableLoader));
        }

        /// <inheritdoc/>
        public IGameHtmlInspectionReport GenerateHtmlInspectionReport(string culture,
                                                                      IDictionary<string, IList<PaytableDenominationInfo>> gameThemeAndPayvarDictionary)
        {
            var reportContexts = new List<ReportContext>();
            foreach(var kvp in gameThemeAndPayvarDictionary)
            {
                var themeIdentifier = kvp.Key;
                foreach(var paytableDenomCombo in kvp.Value)
                {
                    var paytableIdentifier = paytableDenomCombo.PaytableIdentifier;
                    var denomination = paytableDenomCombo.Denomination;
                    var paytableTag = ReportLib.GameInformation.GetPaytableTag(themeIdentifier, paytableIdentifier);
                    var maxBet = ReportLib.GameInformation.GetMaxBet(themeIdentifier, paytableIdentifier, denomination);
                    var creditFormatter = ReportLib.LocalizationInformation.GetCreditFormatter();

                    var paytableLoadResult = PaytableLoader.LoadPaytable(paytableTag.PaytableFileName, paytableTag.PaytableName);
                    var genericPaytableData = paytableLoadResult?.GenericPaytableData;

                    var reportContext = new ReportContext(culture,
                                                          denomination,
                                                          themeIdentifier,
                                                          paytableTag,
                                                          maxBet,
                                                          creditFormatter,
                                                          genericPaytableData);

                    reportContexts.Add(reportContext);
                }
            }

            var htmlReport = CreateHtmlReport(reportContexts);
            var inspectionReport = new GameHtmlInspectionReport(htmlReport);
            return inspectionReport;
        }

        /// <inheritdoc />
        public void GetMinPlayableCreditBalances(IList<MinPlayableCreditBalanceRequest> requests)
        {
            var reportContexts = new List<ReportContext>();
            foreach(var request in requests)
            {
                var paytableTag = ReportLib.GameInformation.GetPaytableTag(request.ThemeIdentifier, request.PaytableIdentifier);

                var maxBet = ReportLib.GameInformation.GetMaxBet(request.ThemeIdentifier, request.PaytableIdentifier, request.Denomination);

                var paytableLoadResult = PaytableLoader.LoadPaytable(paytableTag.PaytableFileName, paytableTag.PaytableName);
                var genericPaytableData = paytableLoadResult?.GenericPaytableData;

                var context = new ReportContext("en-US",
                                                request.Denomination,
                                                request.ThemeIdentifier,
                                                paytableTag,
                                                maxBet,
                                                CreditFormatter.DefaultUS,
                                                genericPaytableData);

                reportContexts.Add(context);
            }

            var returned = GetAllMinPlayableCreditBalances(reportContexts);

            // Assign the returned values to the output fields of the original requests.
            // If less values returned than requested, the request's output field will be 0.
            if(returned != null)
            {
                var count = Math.Min(returned.Count, requests.Count);

                for(var i = 0; i < count; i++)
                {
                    requests[i].MinPlayableCreditBalance = returned[i];
                }
            }
        }
    }
}