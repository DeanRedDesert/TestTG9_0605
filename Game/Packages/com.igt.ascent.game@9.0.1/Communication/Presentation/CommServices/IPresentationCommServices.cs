// -----------------------------------------------------------------------
// <copyright file = "IPresentationCommServices.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using CommunicationLib;

    /// <summary>
    /// This interface defines communication services used by the presentation.
    /// </summary>
    public interface IPresentationCommServices
    {
        /// <summary>
        /// Gets the object responsible for retrieving presentation messages from the presentation message queue.
        /// </summary>
        IPresentationServiceHost PresentationHost { get; }

        /// <summary>
        /// Gets the object responsible for adding logic messages to the logic message queue.
        /// </summary>
        IGameLogic GameLogicClient { get; }

        /// <summary>
        /// Gets the object responsible for adding player session related logic messages to the queue.
        /// </summary>
        IPlayerSessionLogic PlayerSessionLogicClient { get; }
    }
}