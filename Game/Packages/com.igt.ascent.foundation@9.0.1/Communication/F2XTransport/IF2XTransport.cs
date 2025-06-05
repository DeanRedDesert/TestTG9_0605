//-----------------------------------------------------------------------
// <copyright file = "IF2XTransport.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Collections.Generic;
    using Threading;
    using Transport;

    /// <summary>
    /// Interface for the F2X transport.
    /// </summary>
    /// <remarks>
    /// Implementers of this interface provide the low level mechanisms for connecting to the foundation and sending
    /// and receiving messages. The transport is not aware of the application types used within the messages.
    /// </remarks>
    public interface IF2XTransport
    {
        /// <summary>
        /// Most recent transaction id.
        /// </summary>
        uint TransactionId { get; }

        /// <summary>
        /// A monitor to monitor the exceptions on the communication thread.
        /// </summary>
        IExceptionMonitor TransportExceptionMonitor { get; }

        /// <summary>
        /// Send a request to the foundation on the foundation channel.
        /// </summary>
        /// <param name="xml">XML message content.</param>
        /// <param name="category">
        /// The F2X message category the message is intended for.
        /// </param>
        /// <remarks>Should be used when making a request during the processing of an action response.</remarks>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error sending a message or if the foundation is disconnected.
        /// </exception>
        void SendFoundationChannelRequest(string xml, MessageCategory category);

        /// <summary>
        /// Send a reply to the foundation on the foundation channel.
        /// </summary>
        /// <param name="xml">XML message content.</param>
        /// <param name="category">
        /// The F2X message category the message is intended for.
        /// </param>
        /// <remarks>
        /// This function should be used to reply to request which were made by the foundation on the foundation channel.
        /// </remarks>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error sending a message or if the foundation is disconnected.
        /// </exception>
        void SendFoundationChannelResponse(string xml, MessageCategory category);

        /// <summary>
        /// Send a request to the foundation on the game channel.
        /// </summary>
        /// <param name="xml">XML message content.</param>
        /// <param name="category">
        /// The F2X message category the message is intended for.
        /// </param>
        /// <remarks>
        /// Used for initiating communication with the foundation when not processing an action response. The primary
        /// purpose of this function is to send an action request.
        /// </remarks>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error sending a message or if the foundation is disconnected.
        /// </exception>
        void SendGameChannelRequest(string xml, MessageCategory category);

        /// <summary>
        /// Send a request to the foundation on the foundation non-transactional channel.
        /// </summary>
        /// <param name="xml">XML message content.</param>
        /// <param name="category">
        /// The F2X message category the message is intended for.
        /// </param>
        /// <remarks>
        /// This function should be used to send a request to the foundation on the foundation
        /// non-transactional channel when the game has control of the channel.
        /// </remarks>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error sending a message or if the foundation is disconnected.
        /// </exception>
        void SendFoundationNonTransactionalChannelRequest(string xml, MessageCategory category);

        /// <summary>
        /// Send a reply to the foundation on the foundation non-transactional channel.
        /// </summary>
        /// <param name="xml">XML message content.</param>
        /// <param name="category">
        /// The F2X message category the message is intended for.
        /// </param>
        /// <remarks>
        /// This function should be used to reply to request made by the foundation on
        /// the foundation non-transactional channel.
        /// </remarks>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error sending a message or if the foundation is disconnected.
        /// </exception>
        void SendFoundationNonTransactionalChannelResponse(string xml, MessageCategory category);

        /// <summary>
        /// Initiate a connection with the foundation. This function will block until the connection has been
        /// established and all of the API category versions have been negotiated.
        /// </summary>
        /// <exception cref="ConnectionException">Thrown when there is an error connecting. </exception>
        void Connect();
        
        /// <summary>
        /// Disconnect from the foundation.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Register a category handler with the F2X transport. This handler will be used to handle messages which
        /// are sent to the category enumeration contained in the handler. There can only be a single category handler
        /// for each category.
        /// </summary>
        /// <param name="categoryHandler">API category handler.</param>
        /// <exception cref="DuplicateCategoryException">
        /// Thrown when an attempt is made to install a duplicate category.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the passed category handler is null.</exception>
        void InstallCategoryHandler(IApiCategory categoryHandler);

        /// <summary>
        /// Unregister control level category handlers with the F2X transport,
        /// so that new handlers can be installed after a control level re-negotiation.
        /// A control level category is negotiated by one of the xxxControl categories,
        /// such as LinkControl, SystemApiControl, ThemeApiControl and TsmApiControl etc.
        /// </summary>
        /// <remarks>
        /// The handler of connection level categories, such as Connect and LinkControl,
        /// cannot be uninstalled.
        /// </remarks>
        void UninstallControlLevelCategoryHandlers();

        /// <summary>
        /// Unregister control level category handlers with the F2X transport,
        /// so that new handlers can be installed after a control level re-negotiation.
        /// A control level category is negotiated by one of the xxxControl categories,
        /// such as LinkControl, SystemApiControl, ThemeApiControl and TsmApiControl etc.
        /// </summary>
        /// <param name="categoriesToRemove">
        /// A collection of <see cref="MessageCategory"/> values specifying which
        /// categories to unregister.
        /// </param>
        /// <remarks>
        /// The handler of connection level categories, such as Connect and LinkControl,
        /// cannot be uninstalled.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if attempting to remove a connection level category handlers.
        /// </exception>
        void UninstallControlLevelCategoryHandlers(IEnumerable<MessageCategory> categoriesToRemove);

        /// <summary>
        /// Acquire control of the specified channel.
        /// </summary>
        /// <param name="channel">Channel to acquire control of.</param>
        /// <exception cref="InvalidChannelException">Thrown when the given channel is not recognized.</exception>
        void AcquireChannel(Channel channel);

        /// <summary>
        /// Release control of the specified channel.
        /// </summary>
        /// <param name="channel">The channel to release.</param>
        /// <exception cref="InvalidChannelException">Thrown when the given channel is not recognized.</exception>
        void ReleaseChannel(Channel channel);

        /// <summary>
        /// Sets the flag indicating a lightweight transaction.
        /// </summary>
        void SetLightweightTransaction();

        /// <summary>
        /// Clears the flag indicating a lightweight transaction.
        /// </summary>
        void ClearLightweightTransaction();

        /// <summary>
        /// Checks if current transaction weight is heavyweight.
        /// </summary>
        /// <exception cref="InvalidTransactionWeightException">
        /// Thrown when no heavyweight transaction is available.
        /// </exception>
        void MustHaveHeavyweightTransaction();
    }
}