//-----------------------------------------------------------------------
// <copyright file = "ICabinetCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Xml.Serialization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;

    /// <summary>
    /// Callback interface for classes which handle CSI functionality.
    /// </summary>
    public interface ICabinetCategory
    {
        /// <summary>
        /// The category enum.
        /// </summary>
        Category Category { get; }

        /// <summary>
        /// The major version of the category.
        /// </summary>
        ushort VersionMajor { get; }

        /// <summary>
        /// The minor version of the category.
        /// </summary>
        ushort VersionMinor { get; }

        /// <summary>
        /// Install the transport in the category.
        /// </summary>
        /// <param name="transport">Transport the category will use to communicate.</param>
        void InstallTransport(ICsiTransport transport);

        /// <summary>
        /// XmlSerializer for the object type supported by the callback handler.
        /// </summary>
        XmlSerializer MessageSerializer { get; }

        /// <summary>
        /// Handle an event from the CSI Manager.
        /// </summary>
        /// <param name="message">The event to handle.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the message type is not the expected type for the category.
        /// </exception>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the message does not contain a valid message body.
        /// </exception>
        /// <exception cref="UnhandledEventException">Thrown if the event is not handled by the category.</exception>
        void HandleEvent(object message);

        /// <summary>
        /// Handle a request from the CSI Manager.
        /// </summary>
        /// <param name="message">The request to handle.</param>
        /// <param name="requestId">The ID of the request.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the message type is not the expected type for the category.
        /// </exception>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the message does not contain a valid message body.
        /// </exception>
        /// <exception cref="UnhandledRequestException">Thrown if the request is not handled by the category.</exception>
        void HandleRequest(object message, ulong requestId);

        /// <summary>
        /// Handle a response from the CSI Manager.
        /// </summary>
        /// <param name="message">The response to handle.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <exception cref="UnexpectedReplyTypeException">Thrown when the type of the response was not expected.</exception>
        void HandleResponse(object message);
    }
}
