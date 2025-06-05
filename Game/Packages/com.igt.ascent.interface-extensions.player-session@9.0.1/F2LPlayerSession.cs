// -----------------------------------------------------------------------
// <copyright file = "F2LPlayerSession.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended player session interface over F2L.
    /// </summary>
    internal class F2LPlayerSession : F2XPlayerSessionBase
    {
        /// <summary>
        /// The object for querying game mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2LPlayerSession"/>.
        /// </summary>
        /// <param name="playerSessionCategory">
        /// PlayerSession category instance used to communicate with the Foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
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
        public F2LPlayerSession(IPlayerSessionCategory playerSessionCategory,
                                IEventDispatcher transactionalEventDispatcher,
                                ILayeredContextActivationEventsDependency layeredContextActivationEvents, 
                                IGameModeQuery gameModeQuery):
                                base(playerSessionCategory, transactionalEventDispatcher)
        {
            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));
            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the activate theme context event to cache the player session status and foundation owned
        /// setting on session timer display being enabled or not.
        /// </summary>
        /// <param name="sender">
        /// The sender of this event.
        /// </param>
        /// <param name="eventArgs">
        /// The activate theme context event arguments.
        /// </param>
        private void HandleActivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                if(gameModeQuery.GameMode == GameMode.Play)
                {
                    SessionTimerDisplayEnabled = PlayerSessionCategory.GetConfigDataSessionTimerDisplayEnabled();
                    PlayerSessionStatus = PlayerSessionCategory.QueryPlayerSessionStatus().ToPublic();
                }
            }
        }

        #endregion
    }
}
