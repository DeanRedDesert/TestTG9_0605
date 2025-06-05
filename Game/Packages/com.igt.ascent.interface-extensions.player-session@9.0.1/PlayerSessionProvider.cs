//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionProvider.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Logic.Services;

    /// <summary>
    /// Core logic service provider for player session status.
    /// </summary>
    public class PlayerSessionProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Private Fields

        /// <summary>
        /// The cached reference of <see cref="IPlayerSession"/> instance.
        /// </summary>
        private readonly IPlayerSession playerSession;

        #endregion

        #region Public Fields

        /// <summary>
        /// Gets the flag indicating whether session timer display is enabled or not.
        /// </summary>
        [GameService]
        public bool SessionTimerDisplayEnabled => playerSession.SessionTimerDisplayEnabled;

        /// <summary>
        /// Gets the flag indicating whether there is a player session active.
        /// True means there is an active session. False means no active session.
        /// </summary>
        [AsynchronousGameService]
        public bool SessionActive => playerSession.PlayerSessionStatus.SessionActive;

        /// <summary>
        /// Gets the session start time of DateTime type.
        /// </summary>
        /// <remarks>
        /// This value is valid only when a player session is active.
        /// </remarks>
        [AsynchronousGameService]
        public DateTime SessionStartTime => playerSession.PlayerSessionStatus.SessionStartTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="PlayerSessionProvider"/>.
        /// </summary>
        /// <param name="gameLib">
        /// The <see cref="IGameLib"/> instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="gameLib"/> is null.
        /// </exception>
        public PlayerSessionProvider(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException(nameof(gameLib));
            }

            playerSession = gameLib.GetInterface<IPlayerSession>();
            if(playerSession != null)
            {
                playerSession.PlayerSessionStatusChangedEvent += HandlePlayerSessionStatusChangedEvent;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the player session status changed event.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="eventArgs">The event arguments for this event.</param>
        private void HandlePlayerSessionStatusChangedEvent(object sender, EventArgs eventArgs)
        {
            AsynchronousProviderChanged?.Invoke(this, new AsynchronousProviderChangedEventArgs(
                new List<ServiceSignature>
                {
                    new ServiceSignature("SessionActive"),
                    new ServiceSignature("SessionStartTime")
                }, true));
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Implementation

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLibInterface)
        {
            if(playerSession != null)
            {
                playerSession.PlayerSessionStatusChangedEvent -= HandlePlayerSessionStatusChangedEvent;
            }
        }

        #endregion
    }
}
