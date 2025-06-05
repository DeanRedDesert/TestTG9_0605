// -----------------------------------------------------------------------
// <copyright file = "F2EPlayerSession.cs" company = "IGT">
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
    /// Implementation of the extended player session interface over F2E.
    /// </summary>
    internal class F2EPlayerSession : F2XPlayerSessionBase
    {
        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2EPlayerSession"/>.
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2EPlayerSession(IPlayerSessionCategory playerSessionCategory,
                                IEventDispatcher transactionalEventDispatcher,
                                ILayeredContextActivationEventsDependency layeredContextActivationEvents):
                                base (playerSessionCategory, transactionalEventDispatcher)
        {
            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateExtensionContextEvent;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the activate extension context event to cache the player session status.
        /// </summary>
        /// <param name="sender">
        /// The sender of this event.
        /// </param>
        /// <param name="eventArgs">
        /// The activate extension context event arguments.
        /// </param>
        private void HandleActivateExtensionContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle application level context activation events.
            if(eventArgs.ContextLayer == ContextLayer.Application)
            {
                PlayerSessionStatus = PlayerSessionCategory.QueryPlayerSessionStatus().ToPublic();
            }
        }

        #endregion
    }
}