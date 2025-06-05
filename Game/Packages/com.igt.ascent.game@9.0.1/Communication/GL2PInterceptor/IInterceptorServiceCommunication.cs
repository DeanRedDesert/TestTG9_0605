//-----------------------------------------------------------------------
// <copyright file = "IInterceptorServiceCommunication.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    /// <summary>
    /// Defines basic communication API for a service.
    /// </summary>
    public interface IInterceptorServiceCommunication
    {
        /// <summary>
        /// Allow clients to start connecting to service.
        /// </summary>
        void AcceptClientConnections();

        /// <summary>
        /// Close any current client connections and stop allowing new clients
        /// to connect.
        /// </summary>
        void CloseClientConnections();

        /// <summary>
        /// Gets a bool indicating if a client is currently connected to the
        /// service.
        /// </summary>
        bool IsClientConnected { get; }
    }
}
