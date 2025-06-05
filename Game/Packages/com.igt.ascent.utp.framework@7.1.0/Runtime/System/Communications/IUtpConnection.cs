// -----------------------------------------------------------------------
// <copyright file = "IUtpConnection.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;

    /// <summary>
    /// Interface for a UTP connection.
    /// </summary>
    /// <seealso cref="IGT.Game.Utp.Framework.Communications.IUtpCommunication" />
    public interface IUtpConnection : IUtpCommunication
    {
        /// <summary>
        /// Establishes connection to UTP. 
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">Port number to attach with</param>
        /// <returns>Connection status</returns>
        bool Connect(string address, int port);

        /// <summary>
        /// Closes all host and client connections
        /// </summary>
        /// <returns>All sockets closed status</returns>
        bool Close();

        /// <summary>
        /// Checks if a host or client socket is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Occurs when [automation command received].
        /// </summary>
        event EventHandler<AutomationCommandArgs> AutomationCommandReceived;
    }
}