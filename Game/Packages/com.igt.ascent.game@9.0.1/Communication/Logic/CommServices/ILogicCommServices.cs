// -----------------------------------------------------------------------
// <copyright file = "ILogicCommServices.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using Ascent.Restricted.EventManagement.Interfaces;
    using CommunicationLib;

    /// <summary>
    /// This interface defines communication services used by the logic.
    /// </summary>
    public interface ILogicCommServices
    {
        /// <summary>
        /// Gets the object that controls the logic comm host.
        /// </summary>
        ILogicHostControl LogicHostControl { get; }

        /// <summary>
        /// Gets the event queue of presentation events (logic messages sent by the presentation).
        /// </summary>
        IEventQueue PresentationEventQueue { get; }

        /// <summary>
        /// Gets the dispatcher of presentation events (logic messages sent by the presentation).
        /// </summary>
        IEventDispatcher PresentationEventDispatcher { get; }

        /// <summary>
        /// Gets the object responsible for adding presentation messages to the presentation message queue.
        /// </summary>
        IPresentation PresentationClient { get; }

        /// <summary>
        /// Gets the object responsible for adding presentation transition messages to the presentation message queue.
        /// </summary>
        IPresentationTransition PresentationTransition { get; }
    }
}