//-----------------------------------------------------------------------
// <copyright file = "IUgpMessageStrip.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines an interface that allows the package to retrieve the messages that need to be
    /// displayed in the message strip.
    /// </summary>
    public interface IUgpMessageStrip
    {
        /// <summary>
        /// Event raised when message is added.
        /// </summary>
        event EventHandler<MessageAddedEventArgs> MessageAdded;

        /// <summary>
        /// Event raised when message is removed.
        /// </summary>
        event EventHandler<MessageRemovedEventArgs> MessageRemoved;

        /// <summary>
        /// Retrieve all the messages.
        /// </summary>
        /// <returns>A list of message string.</returns>
        IEnumerable<string> GetMessages();
    }
}
