// -----------------------------------------------------------------------
// <copyright file = "F2XRuntimeGameEvents.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended interface of runtime game events
    /// that is backed by the F2X.
    /// </summary>
    internal class F2XRuntimeGameEvents : IRuntimeGameEvents, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The runtime game events category instance
        /// used to communicate with the Foundation.
        /// </summary>
        private readonly IRuntimeGameEventsCategory runtimeGameEventsCategory;

        /// <summary>
        /// The interface for querying game mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XRuntimeGameEvents"/>.
        /// </summary>
        /// <param name="runtimeGameEventsCategory">
        /// Runtime game events category instance used to communicate with the Foundation.
        /// </param>
        /// <param name="layeredContextActivationEvents">
        /// The interface for providing context events.
        /// </param>
        /// <param name="gameModeQuery">
        /// The interface for querying game mode.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XRuntimeGameEvents(
            IRuntimeGameEventsCategory runtimeGameEventsCategory,
            ILayeredContextActivationEventsDependency layeredContextActivationEvents,
            IGameModeQuery gameModeQuery
        )
        {
            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            this.runtimeGameEventsCategory = runtimeGameEventsCategory ??
                                             throw new ArgumentNullException(nameof(runtimeGameEventsCategory));
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));

            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;

            RuntimeGameEventsConfiguration = new RuntimeGameEventsConfiguration();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the activate theme context event to cache the current runtime game events configuration.
        /// </summary>
        /// <param name="sender">
        /// The sender of this event.
        /// </param>
        /// <param name="eventArgs">
        /// The activate (theme) context event arguments.
        /// </param>
        private void HandleActivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                if(gameModeQuery.GameMode == GameMode.Play)
                {
                    var configuration = runtimeGameEventsCategory.GetRuntimeGameEventsConfiguration();
                    if(configuration != null)
                    {
                        RuntimeGameEventsConfiguration = new RuntimeGameEventsConfiguration(
                            configuration.WaitingForGenericInputStatusUpdateEnabled,
                            configuration.PlayerChoiceUpdateEnabled,
                            configuration.GameSelectionStatusUpdateEnabled,
                            configuration.GameBetMeterUpdateEnabled,
                            configuration.GameBetMeterKeysUpdateEnabled
                        );
                    }
                }
                else
                {
                    RuntimeGameEventsConfiguration = new RuntimeGameEventsConfiguration();
                }
            }
        }

        #endregion

        #region Implementation of IRuntimeGameEvents

        /// <inheritdoc />
        public RuntimeGameEventsConfiguration RuntimeGameEventsConfiguration { get; private set; }

        /// <inheritdoc />
        public void WaitingForGenericInputStarted()
        {
            runtimeGameEventsCategory.WaitingForGenericInputStatusUpdate(true);
        }

        /// <inheritdoc />
        public void WaitingForGenericInputEnded()
        {
            runtimeGameEventsCategory.WaitingForGenericInputStatusUpdate(false);
        }

        /// <inheritdoc />
        public void PlayerChoiceUpdate(uint playerChoiceIndex)
        {
            runtimeGameEventsCategory.PlayerChoiceUpdate(playerChoiceIndex);
        }

        /// <inheritdoc />
        public void GameSelectionEntered()
        {
            runtimeGameEventsCategory.GameSelectionStatusUpdate(true);
        }

        /// <inheritdoc />
        public void GameSelectionExited()
        {
            runtimeGameEventsCategory.GameSelectionStatusUpdate(false);
        }

        /// <inheritdoc />
        public void GameBetMeterUpdate(long betPerLine, uint selectedLines)
        {
            runtimeGameEventsCategory.GameBetMeterUpdate(new Amount(betPerLine), selectedLines);
        }

        /// <inheritdoc />
        public void GameBetMeterKeysUpdate(string horizontalKey, string verticalKey)
        {
            runtimeGameEventsCategory.GameBetMeterKeysUpdate(horizontalKey, verticalKey);
        }

        #endregion
    }
}
