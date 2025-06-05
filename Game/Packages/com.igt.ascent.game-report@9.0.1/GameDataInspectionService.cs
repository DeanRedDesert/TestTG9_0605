// -----------------------------------------------------------------------
// <copyright file = "GameDataInspectionService.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using Logging;

    /// <summary>
    /// This class implements the <see cref="IReportingService"/> for Game Data Inspection
    /// reporting service, which provides a report to be displayed in the Audit operator
    /// menu. Typically used for Macau jurisdiction.
    /// </summary>
    internal class GameDataInspectionService : IReportingService
    {
        #region Private Fields

        /// <summary>
        /// The service handler for Game Data Inspection reporting service.
        /// </summary>
        private readonly IGameDataInspectionServiceHandler gameDataInspectionServiceHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="GameDataInspectionService"/>.
        /// </summary>
        /// <param name="gameDataInspectionServiceHandler">
        /// Service handler for Game Data Inspection reporting service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameDataInspectionServiceHandler"/> is null.
        /// </exception>
        public GameDataInspectionService(IGameDataInspectionServiceHandler gameDataInspectionServiceHandler)
        {
            this.gameDataInspectionServiceHandler = gameDataInspectionServiceHandler ?? throw new ArgumentNullException(nameof(gameDataInspectionServiceHandler));
        }

        #endregion

        #region IReportingService Members

        /// <inheritdoc/>
        public ReportingServiceType ReportingServiceType => ReportingServiceType.GameDataInspection;

        /// <inheritdoc/>
        public void RegisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateInspectionReportEvent += HandleGenerateInspectionReportEvent;
            reportLib.GetInspectionReportTypeEvent += HandleGetInspectionReportTypeEvent;
            reportLib.GetMinPlayableCreditBalanceEvent += HandleGetMinPlayableCreditBalanceEvent;
        }

        /// <inheritdoc/>
        public void UnregisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateInspectionReportEvent -= HandleGenerateInspectionReportEvent;
            reportLib.GetInspectionReportTypeEvent -= HandleGetInspectionReportTypeEvent;
            reportLib.GetMinPlayableCreditBalanceEvent -= HandleGetMinPlayableCreditBalanceEvent;
        }

        /// <inheritdoc/>
        public void CleanUpResources(IReportLib reportLib)
        {
            gameDataInspectionServiceHandler.CleanUpResources(reportLib);

            if(gameDataInspectionServiceHandler is IDisposable disposableServiceHandler)
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
        /// <param name="eventArgs">Generate inspection report event data.</param>
        private void HandleGenerateInspectionReportEvent(object sender, GenerateInspectionReportEventArgs eventArgs)
        {
            try
            {
                eventArgs.GeneratedReport =
                    gameDataInspectionServiceHandler.GenerateInspectionReport(eventArgs.ThemeIdentifier,
                                                                              eventArgs.PaytableIdentifier,
                                                                              eventArgs.Denomination,
                                                                              eventArgs.Culture);
            }
            catch(Exception exception)
            {
                eventArgs.GeneratedReport = null;
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        /// <summary>
        /// Gets the report type of <see cref="GameInspectionReportType"/>​associated with this inspection service.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The get inspection report type event data.</param>
        private void HandleGetInspectionReportTypeEvent(object sender, GetInspectionReportTypeEventArgs eventArgs)
        {
            if(eventArgs == null)
            {
                eventArgs = new GetInspectionReportTypeEventArgs();
            }
            
            eventArgs.GameInspectionReportType |= GameInspectionReportType.Itemized;
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
                gameDataInspectionServiceHandler.GetMinPlayableCreditBalances(eventArgs.Requests);
            }
            catch(Exception exception)
            {
                Log.WriteWarning(exception.ToString());
            }
        }

        #endregion
    }
}