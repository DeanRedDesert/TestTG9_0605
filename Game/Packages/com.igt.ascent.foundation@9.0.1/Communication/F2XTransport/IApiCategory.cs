//-----------------------------------------------------------------------
// <copyright file = "IApiCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Xml.Serialization;
    using Transport;

    /// <summary>
    /// Interface for F2L API Categories.
    /// </summary>
    public interface IApiCategory
    {
        /// <summary>
        /// XmlSerializer for the object type supported by the category.
        /// </summary>
        XmlSerializer MessageSerializer { get; }

        /// <summary>
        /// The F2X message category.
        /// </summary>
        MessageCategory Category { get; }

        /// <summary>
        /// The major version supported by the category.
        /// </summary>
        uint MajorVersion { get; }

        /// <summary>
        /// The minor version supported by the category.
        /// </summary>
        uint MinorVersion { get; }

        /// <summary>
        /// Handle a message from the foundation.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <exception cref="ArgumentNullException">Thrown when the passed message is null.</exception>
        void HandleMessage(object message);

        /// <summary>
        /// Handle a reply from the foundation.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="channel">Channel which the message was on.</param>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the type of the reply was not expected. This is normally because the category does not have
        /// any reply pending.
        /// </exception>
        /// <exception cref="UnhandledReplyTypeException">
        /// Thrown when the reply type cannot be handled by the category.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the passed message is null.</exception>
        void HandleReply(object message, Channel channel);
    }
}