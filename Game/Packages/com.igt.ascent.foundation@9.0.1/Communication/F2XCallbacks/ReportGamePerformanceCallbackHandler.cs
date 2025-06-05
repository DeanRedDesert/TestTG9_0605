//-----------------------------------------------------------------------
// <copyright file = "ReportGamePerformanceCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Xml;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.ReportGamePerformance;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Report Game Performance API category.
    /// </summary>
    internal class ReportGamePerformanceCallbackHandler : IReportGamePerformanceCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="ReportGamePerformanceCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        internal ReportGamePerformanceCallbackHandler(IEventCallbacks eventCallbacks)
        {
            if(eventCallbacks == null)
            {
                throw new ArgumentNullException("eventCallbacks");
            }

            this.eventCallbacks = eventCallbacks;
        }

        #region IReportGamePerformanceCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetPerformanceReport(ThemeIdentifier theme,
                                                  PayvarIdentifier payvar,
                                                  out GetPerformanceReportReplyContent callbackResult)
        {
            var generateGamePerformanceReportEventArgs =
                new GenerateGamePerformanceReportEventArgs(theme.Value, payvar.Value);

            // Post event for report object to handle.
            eventCallbacks.PostEvent(generateGamePerformanceReportEventArgs);

            // Process the return value of the event handling.
            var generatedReport = generateGamePerformanceReportEventArgs.GeneratedReport;
            var errorMessage = generateGamePerformanceReportEventArgs.ErrorMessage;

            callbackResult = new GetPerformanceReportReplyContent();
            if(generatedReport != null)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(generatedReport);

                callbackResult.ReportGenerated = true;
                callbackResult.GamePerformanceReport = new GamePerformanceReport
                    {
                        Any = new[] { xmlDoc.DocumentElement }
                    };
            }
            else
            {
                callbackResult.ReportGenerated = false;
                callbackResult.GamePerformanceReport = new GamePerformanceReport();

                if(string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Failed to generate a game performance report.";
                }
            }

            return errorMessage;
        }

        #endregion
    }
}
