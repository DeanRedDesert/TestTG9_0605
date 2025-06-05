//-----------------------------------------------------------------------
// <copyright file = "ConnectionException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// Exception thrown when there is a problem with the foundation connection.
    /// </summary>
    public class ConnectionException : Exception
    {
        /// <summary>
        /// The address of the connection associated with the exception.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The port of the connection associated with the exception.
        /// </summary>
        public ushort Port { get; private set; }

        private const string MessageFormat = "{0} Connection: {1}:{2}";

        /// <summary>
        /// Construct an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="address">Address associated with the connection.</param>
        /// <param name="port">Port associated with the connection.</param>
        public ConnectionException(string message, string address, ushort port)
            : base(string.Format(MessageFormat, message, address, port))
        {
            Address = address;
            Port = port;
        }

        /// <summary>
        /// Construct an instance of the exception with the given message and internal exception.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="address">Address associated with the connection.</param>
        /// <param name="port">Port associated with the connection.</param>
        /// <param name="innerException">
        /// An instance of the exception that describes the error that caused the current exception.
        /// </param>
        public ConnectionException(string message, string address, ushort port, Exception innerException)
            : base(string.Format(MessageFormat, message, address, port), innerException)
        {
            Address = address;
            Port = port;
        }
    }
}