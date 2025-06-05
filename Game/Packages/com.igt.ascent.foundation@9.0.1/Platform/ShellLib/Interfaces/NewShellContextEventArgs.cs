// -----------------------------------------------------------------------
// <copyright file = "NewShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a new shell context is being switched to.
    /// </summary>
    [Serializable]
    public sealed class NewShellContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the game mode of the new shell context.
        /// </summary>
        public GameMode GameMode { get; private set; }

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="NewShellContextEventArgs"/>.
        /// </summary>
        /// <param name="gameMode">
        /// The game mode of the new shell context.
        /// </param>
        public NewShellContextEventArgs(GameMode gameMode)
        {
            GameMode = gameMode;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("NewShellContextEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t GameMode: " + GameMode);

            return builder.ToString();
        }

        #endregion
    }
}