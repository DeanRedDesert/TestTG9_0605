//-----------------------------------------------------------------------
// <copyright file = "ITransport.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using Threading;

    /// <summary>
    /// Delegate used by transport clients to handle messages.
    /// </summary>
    /// <param name="messageReader">An object used to read the message.</param>
    public delegate void HandleMessageDelegate(IBinaryMessageReader messageReader);

    /// <summary>
    /// Interface to be used by base transport implementers. Provides basic functionality common among all transport
    /// types.
    /// </summary>
    public interface ITransport : IExceptionMonitor
    {
        /// <summary>
        /// Gets the major version of this transport.
        /// </summary>
        int MajorVersion { get; }

        /// <summary>
        /// Gets the minor version of this transport.
        /// </summary>
        int MinorVersion { get; }

        /// <summary>
        /// Initiate a connection with the foundation. This function will block until the connection has been
        /// established.
        /// </summary>
        /// <exception cref="ConnectionException">Thrown when there is an error connecting. </exception>
        void Connect();

        /// <summary>
        /// Disconnect from the foundation.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Prepare the transport to begin disconnecting; the transport may experience connection and message
        /// problems that should be ignored, if the server closes the socket connection before the client side does.
        /// </summary>
        void PrepareToDisconnect();

        /// <summary>
        /// Send a binary message to the foundation after prepending the transport header.
        /// </summary>
        /// <param name="message">The <see cref="IBinaryMessage"/> to send.</param>
        /// <exception cref="ConnectionException">Thrown when there is a problem sending a message.</exception>
        void SendMessage(IBinaryMessage message);

        /// <summary>
        /// Sets the message handling delegate. This delegate will be called whenever a message from the foundation is received.
        /// </summary>
        /// <param name="handler">The delegate to call. If null, this will clear the current handler.</param>
        void SetMessageHandler(HandleMessageDelegate handler);
    }
}
