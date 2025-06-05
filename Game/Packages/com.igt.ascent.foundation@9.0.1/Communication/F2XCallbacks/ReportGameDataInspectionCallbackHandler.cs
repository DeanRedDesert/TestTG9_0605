//-----------------------------------------------------------------------
// <copyright file = "ReportGameDataInspectionCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.ReportGameDataInspection;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;
    using GameReport.Interfaces;

    /// <summary>
    /// This class implements callback methods supported by the F2X Report Game Data Inspection API category.
    /// </summary>
    internal class ReportGameDataInspectionCallbackHandler : IReportGameDataInspectionCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="ReportGameDataInspectionCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        internal ReportGameDataInspectionCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IReportGameDataInspectionCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessGetInspectionReport(string culture,
                                                 ThemeIdentifier theme,
                                                 PayvarIdentifier payvar,
                                                 uint denomination,
                                                 out GetInspectionReportReplyContent callbackResult)
        {
            // Theme was added in 1.1.
            var generateInspectionReportEventArgs =
                new GenerateInspectionReportEventArgs( culture,
                                                      theme != null ? theme.Value : string.Empty,
                                                      payvar.Value,
                                                      denomination);

            // Post event for report object to handle.
            eventCallbacks.PostEvent(generateInspectionReportEventArgs);

            // Process the return value of the event handling.
            var generatedReport = generateInspectionReportEventArgs.GeneratedReport;
            var errorMessage = generateInspectionReportEventArgs.ErrorMessage;

            callbackResult = new GetInspectionReportReplyContent();

            if(generatedReport != null)
            {
                callbackResult.ReportGenerated = true;

                var reportData = generatedReport.StandardReportPart?.ToReportItems();

                var progressiveReportData = generatedReport.ProgressiveLevelParts?.Any() == true
                                                ? generatedReport.ProgressiveLevelParts.Select(part => part.ToProgressiveGameLevelData()).ToList()
                                                : null;
                
                callbackResult.InspectionReport =
                    new InspectionReport
                    {
                        ReportData = reportData,
                        ProgressiveReportData = progressiveReportData, 
                    };
            }
            else
            {
                callbackResult.ReportGenerated = false;
                callbackResult.InspectionReport = new InspectionReport();

                if(string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Failed to generate an inspection report. Ensure a valid paytable configuration" +
                                   " and that the specified report type is only Itemized.";
                }
            }

            return errorMessage;
        }

        /// <inheritdoc/>
        public string ProcessGetHtmlInspectionReport(string culture,
                                                     IEnumerable<ThemeInfo> themeInfoList, 
                                                     out GetHtmlInspectionReportReplyContent callbackResult)
        {
            var gameThemeInfoList = themeInfoList.ToThemeVsPaytableDenomDictionary();

            var generateInspectionReportEventArgs = new GenerateHtmlInspectionReportEventArgs(culture, gameThemeInfoList);

            // Post event for report object to handle.
            eventCallbacks.PostEvent(generateInspectionReportEventArgs);

            // Process the return value of the event handling.
            var generatedReport = generateInspectionReportEventArgs.GeneratedHtmlReport;
            var errorMessage = generateInspectionReportEventArgs.ErrorMessage;

            callbackResult = new GetHtmlInspectionReportReplyContent();

            if(generatedReport != null)
            {
                callbackResult.ReportGenerated = true;
                callbackResult.HtmlData = generatedReport.HtmlReport;
            }
            else
            {
                callbackResult.ReportGenerated = false;
                callbackResult.HtmlData = string.Empty;

                if(string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Failed to generate an inspection report.";
                }
            }

            return errorMessage;
        }

        /// <inheritdoc/>
        public string ProcessGetInspectionReportType(out InspectionReportType callbackResult)
        {
            // Post event for report object to handle.
            var getInspectionReportTypeEventArgs = new GetInspectionReportTypeEventArgs();
            eventCallbacks.PostEvent(getInspectionReportTypeEventArgs);
            var f2XInspectionReportType = InspectionReportType.Itemized;
            string errorMessage = null;

            switch(getInspectionReportTypeEventArgs.GameInspectionReportType)
            {
                case GameInspectionReportType.None: 
                    errorMessage = @"Error: No inspection report type has been set.";
                    break;
                case GameInspectionReportType.Itemized: 
                    f2XInspectionReportType = InspectionReportType.Itemized;
                    break;
                case GameInspectionReportType.Html:
                case GameInspectionReportType.All:
                    f2XInspectionReportType = InspectionReportType.Html;
                    break;
            }

            callbackResult = f2XInspectionReportType;
            return errorMessage;
        }

        public string ProcessGetMinimumPlayableCreditBalance(MinimumPlayableCreditBalanceRequestList minimumPlayableCreditBalanceRequestList,
                                                             out MinimumPlayableCreditBalanceResponseList callbackResult)
        {
            var requests = minimumPlayableCreditBalanceRequestList
                           .MinimumPlayableCreditBalanceRequest
                           .Select(request => request.ThemePayvarDenomSelector)
                           .Select(selector => new MinPlayableCreditBalanceRequest(selector.ThemeIdentifier.Value,
                                                                                   selector.PayvarIdentifier.Value,
                                                                                   selector.Denom))
                           .ToList();

            var getMinPlayableCreditBalanceEventArgs = new GetMinPlayableCreditBalanceEventArgs(requests);

            // Post event for report object to handle.
            eventCallbacks.PostEvent(getMinPlayableCreditBalanceEventArgs);

            callbackResult = new MinimumPlayableCreditBalanceResponseList();
            foreach(var response in getMinPlayableCreditBalanceEventArgs.Requests)
            {
                callbackResult.MinimumPlayableCreditBalanceResponse.Add(
                    new MinimumPlayableCreditBalanceResponse
                    {
                        ThemePayvarDenomSelector = new ThemePayvarDenomSelector
                                                   {
                                                       ThemeIdentifier = response.ThemeIdentifier.ToThemeIdentifier(),
                                                       PayvarIdentifier = response.PaytableIdentifier.ToPayvarIdentifier(),
                                                       Denom = response.Denomination
                                                   },
                        MinimumPlayableCreditBalance = response.MinPlayableCreditBalance
                    });
            }

            return null;
        }

        #endregion
    }
}