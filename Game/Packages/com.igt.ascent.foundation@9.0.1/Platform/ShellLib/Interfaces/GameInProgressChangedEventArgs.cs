// -----------------------------------------------------------------------
// <copyright file = "GameInProgressChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event argument for when a game is in progress status is changed.
    /// </summary>
    [Serializable]
    public sealed class GameInProgressChangedEventArgs: TransactionalEventArgs
    {
        /// <summary>
        /// Gets the flag indicating whether or not any coplayer has a game currently in progress.
        /// </summary>
        public bool GameInProgress { get; private set; }

        /// <summary>
        /// Creates an instance of the <see cref="GameInProgressChangedEventArgs"/>.
        /// </summary>
        /// <param name="gameInProgress">Whether or not any coplayer has a game currently in progress.</param>
        public GameInProgressChangedEventArgs(bool gameInProgress)
        {
            GameInProgress = gameInProgress;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("GameInProgressChangedEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t GameInProgress: " + GameInProgress);

            return builder.ToString();
        }

        #endregion
    }
}