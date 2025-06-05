// -----------------------------------------------------------------------
// <copyright file = "StateObjectEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// State object event definition.
    /// </summary>
    public class StateObjectEventArgs
    {
        /// <summary>
        /// Socket.
        /// </summary>
        public Socket SocketX { get; private set; }

        /// <summary>
        /// Size.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Data.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Data { get; private set; }

        /// <summary>
        /// Define state object event.
        /// </summary>
        /// <param name="socket">Socket.</param>
        /// <param name="size">Size.</param>
        /// <param name="data">Data.</param>
        public StateObjectEventArgs(Socket socket, int size, string data)
        {
            if(socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            
            SocketX = socket;
            Size = size;
            Data = data;
        }
    }
}