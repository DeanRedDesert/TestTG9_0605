//-----------------------------------------------------------------------
// <copyright file = "Channel.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    /// <summary>
    /// Enumeration which defines the channels available to to the F2L.
    /// </summary>
    public enum Channel : byte 
    {
        /// <summary>
        /// Channel for foundation initiated communication. The foundation will use this channel for making requests
        /// of the game. If the foundation has sent an action response, then the game may use this channel to make
        /// requests for the duration of the action response.
        /// </summary>
        Foundation = 1,

        /// <summary>
        /// Channel for game initiated communication. The game may use this channel for performing action requests.
        /// </summary>
        Game = 2,

        /// <summary>
        /// Channel for foundation initiated non-transactional communication. The foundation will use this channel for
        /// making non-transactional, asynchronous requests of the game.
        /// </summary>
        FoundationNonTransactional = 3,
    }
}
