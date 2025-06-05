// -----------------------------------------------------------------------
// <copyright file = "F2XPlayerSessionBase.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended player session interface over F2X.
    /// </summary>
    internal abstract class F2XPlayerSessionBase : IPlayerSession, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The PlayerSession category handler.
        /// </summary>
        protected readonly IPlayerSessionCategory PlayerSessionCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XPlayerSessionBase"/>.
        /// </summary>
        /// <param name="playerSessionCategory">
        /// PlayerSession category instance used to communicate with the Foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        protected F2XPlayerSessionBase(IPlayerSessionCategory playerSessionCategory,
                                       IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            PlayerSessionCategory = playerSessionCategory ?? throw new ArgumentNullException(nameof(playerSessionCategory));
            transactionalEventDispatcher.EventDispatchedEvent += HandlePlayerSessionStatusChangedEvent;
        }

        #endregion

        #region IPlayerSession Implementation

        /// <inheritdoc />
        public event EventHandler<PlayerSessionStatusChangedEventArgs> PlayerSessionStatusChangedEvent;

        /// <inheritdoc />
        public PlayerSessionStatus PlayerSessionStatus { get; protected set; }

        /// <inheritdoc />
        public bool SessionTimerDisplayEnabled { get; protected set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the dispatched event if the dispatched event is a PlayerSessionStatusChanged event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandlePlayerSessionStatusChangedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(PlayerSessionStatusChangedEventArgs))
            {
                if(dispatchedEventArgs.DispatchedEvent is PlayerSessionStatusChangedEventArgs eventArgs)
                {
                    PlayerSessionStatus = eventArgs.PlayerSessionStatus;

                    var handler = PlayerSessionStatusChangedEvent;
                    if(handler != null)
                    {
                        handler(this, eventArgs);

                        dispatchedEventArgs.IsHandled = true;
                    }
                }
            }
        }

        #endregion
    }
}