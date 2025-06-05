//-----------------------------------------------------------------------
// <copyright file = "IPresentationNotify.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;

    /// <summary>
    /// Interface for notifications of messages sent to the presentation.
    /// </summary>
    /// <remarks>This interface is used for capturing messages for history.</remarks>
    public interface IPresentationNotify
    {
        /// <summary>
        /// Event which is fired whenever a StartState message is sent to the presentation.
        /// </summary>
        event EventHandler<PresentationNotificationEventArgs> StartStateSent;

        /// <summary>
        /// Event which is fired whenever an UpdateAsynchData message is sent to the presentation.
        /// </summary>
        event EventHandler<PresentationNotificationEventArgs> UpdateAsynchDataSent;
    }
}
