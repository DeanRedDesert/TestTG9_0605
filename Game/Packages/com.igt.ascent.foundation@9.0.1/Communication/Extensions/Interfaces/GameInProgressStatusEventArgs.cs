// -----------------------------------------------------------------------
// <copyright file = "GameInProgressStatusEventArgs.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event occurs when game-in-progress status is changed.
    /// </summary>
    [Serializable]
    public sealed class GameInProgressStatusEventArgs : NonTransactionalEventArgs
    {
        /// <summary>
        /// Gets the flag indicating whether or not any game is currently in progress.
        /// </summary>
        public bool GameInProgress { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="GameInProgressStatusEventArgs"/>.
        /// </summary>
        /// <param name="gameInProgress">Whether or not any game is currently in progress.</param>
        public GameInProgressStatusEventArgs(bool gameInProgress)
        {
            GameInProgress = gameInProgress;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("GameInProgressStatusEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t GameInProgress: " + GameInProgress);

            return builder.ToString();
        }

        #endregion
    }
}