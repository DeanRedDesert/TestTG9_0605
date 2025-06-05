//-----------------------------------------------------------------------
// <copyright file = "IInterceptorClientCommunication.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines basic communication API for a client service.
    /// </summary>
    public interface IInterceptorClientCommunication
    {
        /// <summary>
        /// Connect to the Interceptor Service.
        /// </summary>
        void Connect();

        /// <summary>
        /// Connect to the Interceptor Service.
        /// </summary>
        /// <param name="serviceIpEndPoint">Service EndPoint</param>
        void Connect(IPEndPoint serviceIpEndPoint);

        /// <summary>
        /// Disconnect from the Interceptor Service.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets a bool indicating if connected to the Interceptor Service.
        /// </summary>
        bool IsServiceConnected { get; }

        /// <summary>
        /// Event which is raised when a connected client becomes disconnected.
        /// </summary>
        event EventHandler DisconnectedFromService;

        /// <summary>
        /// Gets the service endpoint address.
        /// </summary>
        IPEndPoint ServiceIpEndPoint { get; }

        /// <summary>
        /// Event which is raised if an error during communications is detected.
        /// </summary>
        event EventHandler<CommunicationErrorEventArgs> CommunicationError;

        /// <summary>
        /// Allows setting the serialization binder to utilize.
        /// </summary>
        SerializationBinder SerializationBinder { get; set; }
    }
}
