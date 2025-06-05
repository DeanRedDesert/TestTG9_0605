// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceService.cs" company = "IGT">
//     Copyright (c) 2016 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using GamePerformance;
    using Interfaces;
    using Logging;

    /// <summary>
    /// This class implements the <see cref="IReportingService"/> for Game Performance reporting
    /// service, which provides a report per payvar on demand from the foundation.
    /// </summary>
    internal class GamePerformanceService : IReportingService
    {
        #region Private Fields

        /// <summary>
        /// The service handler for Game Performance reporting service.
        /// </summary>
        private readonly IGamePerformanceServiceHandler gamePerformanceServiceHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="GamePerformanceService"/>.
        /// </summary>
        /// <param name="gamePerformanceServiceHandler">
        /// Service handler for Game Performance reporting service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gamePerformanceServiceHandler"/> is null.
        /// </exception>
        public GamePerformanceService(IGamePerformanceServiceHandler gamePerformanceServiceHandler)
        {
            this.gamePerformanceServiceHandler = gamePerformanceServiceHandler ?? throw new ArgumentNullException(nameof(gamePerformanceServiceHandler));
        }

        #endregion

        #region IReportingService Members

        /// <inheritdoc/>
        public ReportingServiceType ReportingServiceType => ReportingServiceType.GamePerformance;

        /// <inheritdoc/>
        public void RegisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateGamePerformanceReportEvent += HandleGenerateGamePerformanceReportEvent;
        }

        /// <inheritdoc/>
        public void UnregisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.GenerateGamePerformanceReportEvent -= HandleGenerateGamePerformanceReportEvent;
        }

        /// <inheritdoc/>
        public void CleanUpResources(IReportLib reportLib)
        {
            gamePerformanceServiceHandler.CleanUpResources(reportLib);

            if(gamePerformanceServiceHandler is IDisposable disposableServiceHandler)
            {
                disposableServiceHandler.Dispose();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generates a game performance report per payvar when requested by the Foundation.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The generate game performance report event arguments.</param>
        private void HandleGenerateGamePerformanceReportEvent(
                                        object sender,
                                        GenerateGamePerformanceReportEventArgs eventArgs)
        {
            try
            {
                // Retrieve the game performance data through the service handler.
                var report = gamePerformanceServiceHandler.GenerateGamePerformanceReport(
                                    new GamePerformanceReportContext(eventArgs.ThemeIdentifier,
                                                                     eventArgs.PaytableIdentifier));

                // Generate the report in XML string, where XML declaration must be omitted.
                var builder = new StringBuilder();
                using(var writer = XmlWriter.Create(builder,
                                                    new XmlWriterSettings
                                                    {
                                                        OmitXmlDeclaration = true,
                                                        Indent = true
                                                    }))
                {
                    new XmlSerializer(typeof(GamePerformanceReport)).Serialize(writer, report.ToInternal());
                }

                eventArgs.GeneratedReport = builder.ToString();
            }
            catch(Exception exception)
            {
                eventArgs.GeneratedReport = null;
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        #endregion
    }
}
