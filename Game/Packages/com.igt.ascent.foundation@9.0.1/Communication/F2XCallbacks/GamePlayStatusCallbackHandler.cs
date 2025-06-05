// -----------------------------------------------------------------------
// <copyright file = "GamePlayStatusCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ShellLib.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XGameFocus = F2X.Schemas.Internal.GamePlayStatus.GameFocus;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="GamePlayStatusCategory"/>.
    /// </summary>
    internal class GamePlayStatusCallbackHandler : IGamePlayStatusCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="GamePlayStatusCallbackHandler"/>
        /// </summary>
        /// <param name="eventCallbacksInterface">The callback interface for handling transactional events.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventCallbacksInterface"/> is null.</exception>
        public GamePlayStatusCallbackHandler(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #endregion

        #region IGamePlayStatusCategory Callbacks

        /// <inheritdoc/>
        public string ProcessGameInProgressChanged(bool gameInProgress)
        {
            eventCallbacksInterface.PostEvent(new GameInProgressChangedEventArgs(gameInProgress));
            return null;
        }

        /// <inheritdoc/>
        public string ProcessGameFocusChanged(F2XGameFocus gameFocus)
        {
            var newGameFocus = gameFocus == null
                                   ? null
                                   : new GameFocus(gameFocus.Coplayer,
                                             gameFocus.ThemeSelector.ThemeIdentifier.ToToken(),
                                             gameFocus.ThemeSelector.Denom);

            eventCallbacksInterface.PostEvent(new GameFocusChangedEventArgs(newGameFocus));

            return null;
        }

        #endregion
    }
}