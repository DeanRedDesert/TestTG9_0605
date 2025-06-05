// -----------------------------------------------------------------------
// <copyright file = "GameLevelAwardService.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using Logging;

    /// <summary>
    /// This class implements <see cref="IReportingService"/> for Game Level Award reporting
    /// service, which provides game level progressive award data to be displayed in Chooser.
    /// </summary>
    internal class GameLevelAwardService : IReportingService
    {
        /// <summary>
        /// The service handler for Game Level Award reporting service.
        /// </summary>
        private readonly IGameLevelAwardServiceHandler gameLevelAwardServiceHandler;

        /// <summary>
        /// Instantiates a new <see cref="GameLevelAwardService"/>
        /// </summary>
        /// <param name="gameLevelAwardServiceHandler">
        /// Service handler for Game Level Award reporting service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLevelAwardServiceHandler"/> is null.
        /// </exception>
        public GameLevelAwardService(IGameLevelAwardServiceHandler gameLevelAwardServiceHandler)
        {
            this.gameLevelAwardServiceHandler = gameLevelAwardServiceHandler ?? throw new ArgumentNullException(nameof(gameLevelAwardServiceHandler));
        }

        #region IReportingServce Memebers

        /// <inheritdoc/>
        public ReportingServiceType ReportingServiceType => ReportingServiceType.GameLevelAward;

        /// <inheritdoc/>
        public void RegisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.InitializeGameLevelDataEvent += HandleInitializeGameLevelDataEvent;
            reportLib.GetGameLevelValuesEvent += HandleGetThemeBasedGameLevelValuesEvent;
            reportLib.InactivateContextEvent += HandleInactivateContextEvent;
        }

        /// <inheritdoc/>
        public void UnregisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.InitializeGameLevelDataEvent -= HandleInitializeGameLevelDataEvent;
            reportLib.GetGameLevelValuesEvent -= HandleGetThemeBasedGameLevelValuesEvent;
            reportLib.InactivateContextEvent -= HandleInactivateContextEvent;
        }

        /// <inheritdoc/>
        public void CleanUpResources(IReportLib reportLib)
        {
            gameLevelAwardServiceHandler.CleanUpResources(reportLib);

            if(gameLevelAwardServiceHandler is IDisposable disposableServiceHandler)
            {
                disposableServiceHandler.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Prepares data needed for game-level queries on the event of game level data initialized.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Arguments of the event.</param>
        private void HandleInitializeGameLevelDataEvent(object sender, InitializeGameLevelDataEventArgs eventArgs)
        {
            try
            {
                gameLevelAwardServiceHandler.InitializeForGameLevelQuery(eventArgs.ThemeIdentifier,
                                                                         eventArgs.PaytableDenominationInfos);
            }
            catch(Exception exception)
            {
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        /// <summary>
        /// Fill up the game-level values of themes when receives the message from Foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Arguments of the event.</param>
        private void HandleGetThemeBasedGameLevelValuesEvent(object sender, GetGameLevelValuesEventArgs eventArgs)
        {
            try
            {
                eventArgs.GameLevelValues =
                    gameLevelAwardServiceHandler.GetThemeBasedGameLevelValues(eventArgs.ThemeIdentifier,
                                                                              eventArgs.ProgressiveLevelValues);
            }
            catch(Exception exception)
            {
                eventArgs.GameLevelValues = null;
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        /// <summary>
        /// Clean up the game level progressive data when switching game context.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Payload of the event.</param>
        private void HandleInactivateContextEvent(object sender, InactivateContextEventArgs eventArgs)
        {
            gameLevelAwardServiceHandler.CleanUpData();
        }
    }
}