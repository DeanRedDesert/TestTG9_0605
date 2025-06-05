// -----------------------------------------------------------------------
// <copyright file = "PlayerSessionStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for the player session status being changed.
    /// </summary>
    [Serializable]
    public class PlayerSessionStatusChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the information of the current player session.
        /// </summary>
        public PlayerSessionStatus PlayerSessionStatus { get; private set; }

        /// <summary>
        /// Instantiates a new <see cref="PlayerSessionStatusChangedEventArgs"/>.
        /// </summary>
        /// <param name="playerSessionStatus">
        /// The information of the player session that has been changed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="playerSessionStatus"/> is null.
        /// </exception>
        public PlayerSessionStatusChangedEventArgs(PlayerSessionStatus playerSessionStatus)
        {
            PlayerSessionStatus = playerSessionStatus ?? throw new ArgumentNullException(nameof(playerSessionStatus));
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(base.ToString());
            builder.AppendLine("Player Session Status: ");
            builder.AppendLine(PlayerSessionStatus.ToString());

            return builder.ToString();
        }
    }
}
