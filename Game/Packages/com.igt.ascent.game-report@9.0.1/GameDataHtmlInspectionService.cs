// -----------------------------------------------------------------------
// <copyright file = "GameDataHtmlInspectionService.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using Logging;

    /// <summary>
    /// This class implements the <see cref="IReportingService"/> for the Html based Game Data
    /// Inspection reporting service, which provides a report to be displayed in the Audit operator
    /// menu.
    /// </summary>
    internal class GameDataHtmlInspectionService : IReportingService
    {
        #region Private Fields

        /// <summary>
        /// The service handler for the Html Game Data Inspection reporting service.
        /// </summary>
        private readonly IGameDataHtmlInspectionServiceHandler gameDataHtmlInspectionServiceHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="GameDataHtmlInspectionService"/>.
        /// </summary>
        /// <param name="gameDataHtmlInspectionServiceHandler">
        /// Service handler for Game Data Inspection reporting service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameDataHtmlInspectionServiceHandler"/> is null.
        /// </exception>
        public GameDataHtmlInspectionService(IGameDataHtmlInspectionServiceHandler gameDataHtmlInspectionServiceHandler)
        {
            this.gameDataHtmlInspectionServiceHandler = gameDataHtmlInspectionServiceHandler ?? throw new ArgumentNullException(nameof(gameDataHtmlInspectionServiceHandler));
        }

        #endregion

        #region IReportingService Members

        /// <inheritdoc/>
        public ReportingServiceType ReportingServiceType => ReportingServiceType.GameDataHtmlInspection;

        /// <inheritdoc/>
        public void RegisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateHtmlInspectionReportEvent += HandleGenerateHtmlInspectionReportEvent;
            reportLib.GetInspectionReportTypeEvent += HandleGetInspectionReportTypeEvent;
            reportLib.GetMinPlayableCreditBalanceEvent += HandleGetMinPlayableCreditBalanceEvent;
        }

        /// <inheritdoc/>
        public void UnregisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateHtmlInspectionReportEvent -= HandleGenerateHtmlInspectionReportEvent;
            reportLib.GetInspectionReportTypeEvent -= HandleGetInspectionReportTypeEvent;
            reportLib.GetMinPlayableCreditBalanceEvent -= HandleGetMinPlayableCreditBalanceEvent;
        }

        /// <inheritdoc/>
        public void CleanUpResources(IReportLib reportLib)
        {
            gameDataHtmlInspectionServiceHandler.CleanUpResources(reportLib);

            if(gameDataHtmlInspectionServiceHandler is IDisposable disposableServiceHandler)
            {
                disposableServiceHandler.Dispose();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generates a <see cref="GameInspectionReport"/> when requested from the Foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The generate inspection report event data.</param>
        private void HandleGenerateHtmlInspectionReportEvent(object sender, GenerateHtmlInspectionReportEventArgs eventArgs)
        {
            try
            {
                eventArgs.GeneratedHtmlReport =
                    gameDataHtmlInspectionServiceHandler.GenerateHtmlInspectionReport(eventArgs.Culture, eventArgs.GameThemeAndPayvarDictionary);
            }
            catch(Exception exception)
            {
                eventArgs.GeneratedHtmlReport = null;
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        /// <summary>
        /// Gets the report type of <see cref="GameInspectionReportType"/> associated with this inspection service.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The get inspection report type event data.</param>
        private void HandleGetInspectionReportTypeEvent(object sender, GetInspectionReportTypeEventArgs eventArgs)
        {
            if(eventArgs == null)
            {
                eventArgs = new GetInspectionReportTypeEventArgs();
            }
            
            eventArgs.GameInspectionReportType |= GameInspectionReportType.Html;
        }

        /// <summary>
        /// Gets the minimum playable credit balance when requested by Foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The get minimum playable credit balance event data.</param>
        private void HandleGetMinPlayableCreditBalanceEvent(object sender, GetMinPlayableCreditBalanceEventArgs eventArgs)
        {
            try
            {
                gameDataHtmlInspectionServiceHandler.GetMinPlayableCreditBalances(eventArgs.Requests);
            }
            catch(Exception exception)
            {
                Log.WriteWarning(exception.ToString());
            }
        }

        #endregion
    }
}