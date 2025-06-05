//-----------------------------------------------------------------------
// <copyright file = "ICsiTransport.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using CSI.Schemas.Internal;
    using Threading;

    /// <summary>
    /// Interface used for CSI transport implementations.
    /// </summary>
    public interface ICsiTransport
    {
        /// <summary>
        /// A monitor to monitor the exceptions on the communication thread.
        /// </summary>
        IExceptionMonitor TransportExceptionMonitor { get; }

        /// <summary>
        /// Initiate a connection with the CSI Manager. This function will block until the connection has been
        /// established.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there is not a valid ConnectHandler installed.
        /// </exception>
        void Connect();

        /// <summary>
        /// Disconnect from the CSI Manager.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send the given message as a response.
        /// </summary>
        /// <param name="response">Response to send.</param>
        /// <param name="csiManagerRequestNumber">The request ID for the request this is a response to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
        void SendResponse(Csi response, ulong csiManagerRequestNumber);

        /// <summary>
        /// Send the given message as a request.
        /// </summary>
        /// <param name="request">Request to send.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        void SendRequest(Csi request);

        /// <summary>
        /// Send the given CSI message.
        /// </summary>
        /// <param name="xmlMessage">The CSI message to send.</param>
        /// <exception cref="ArgumentNullException">Thrown when the xmlMessage parameter is null.</exception>
        void SendMessage(string xmlMessage);
    }
}
