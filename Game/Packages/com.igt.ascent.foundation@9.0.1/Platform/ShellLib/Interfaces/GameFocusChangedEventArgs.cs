// -----------------------------------------------------------------------
// <copyright file = "GameFocusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for when the game focus is changed.
    /// </summary>
    [Serializable]
    public sealed class GameFocusChangedEventArgs: TransactionalEventArgs
    {
        /// <summary>
        /// Gets the new game focus.
        /// A null value indicates that no game is currently in focus.
        /// </summary>
        public GameFocus NewGameFocus{get; private set; }

        /// <summary>
        /// Constructs a new instance of the <see cref="GameFocusChangedEventArgs"/>.
        /// </summary>
        /// <param name="gameFocus">
        /// The current game in focus.  Null means no game is currently in focus.
        /// </param>
        public GameFocusChangedEventArgs(GameFocus gameFocus)
        {
            NewGameFocus = gameFocus;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("GameFocusChangedEventArgs -");
            builder.Append(base.ToString());
            if(NewGameFocus == null)
            {
                builder.AppendLine("\tNo game is in focus");
            }
            else
            {
                builder.Append("\t New" + NewGameFocus);
            }

            return builder.ToString();
        }

        #endregion
    }
}