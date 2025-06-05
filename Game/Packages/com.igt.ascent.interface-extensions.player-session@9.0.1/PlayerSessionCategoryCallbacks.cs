//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using F2X;
    using F2X.Schemas.Internal.PlayerSession;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="PlayerSession"/> category.
    /// </summary>
    internal class PlayerSessionCategoryCallbacks : IPlayerSessionCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="PlayerSessionCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public PlayerSessionCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #endregion

        #region IPlayerSessionCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessStatusChanged(PlayerSessionStatusData playerSessionStatusData)
        {
            eventCallbacksInterface.PostEvent(
                new PlayerSessionStatusChangedEventArgs(playerSessionStatusData.ToPublic())
                );

            return null;
        }

        #endregion
    }
}
