//-----------------------------------------------------------------------
// <copyright file = "GameMessageReceivedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event arguments for a received game message.
    /// </summary>
    public class GameMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Serialized string of game message header data.
        /// </summary>
        public string GameMessageHeader { get; }

        /// <summary>
        /// Constructs a new GameMessageReceivedEventArgs.
        /// </summary>
        /// <param name="gameMessageHeader">Serialized string of game message header data.</param>
        public GameMessageReceivedEventArgs(string gameMessageHeader)
        {
            GameMessageHeader = gameMessageHeader;
        }
    }
}
