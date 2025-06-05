// -----------------------------------------------------------------------
// <copyright file = "TcpSocket.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Implements ISocket by exposing System.Net.Sockcets.Socket functionality.
    /// </summary>
    /// <seealso cref="IGT.Game.Utp.Framework.Communications.ISocket" />
    public class TcpSocket : ISocket
    {
        private readonly Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public TcpSocket(Socket socket)
        {
            if(socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            this.socket = socket;
        }

        /// <summary>
        /// Gets the network stream.
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetNetworkStream()
        {
            return new NetworkStream(socket);
        }

        public Stream GetStream()
        {
            return new NetworkStream(socket);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            socket.Close();
        }
    }
}
