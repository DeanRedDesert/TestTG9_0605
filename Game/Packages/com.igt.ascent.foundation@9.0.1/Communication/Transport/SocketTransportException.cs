//-----------------------------------------------------------------------
// <copyright file = "SocketTransportException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class for exceptions which originate from the socket transport. Used to add additional information about the
    /// connection to the original exception.
    /// </summary>
    [Serializable]
    public class SocketTransportException : Exception
    {
        /// <summary>
        /// The address of the connection the exception originated from.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The port of the connection the exception originated from.
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// Original exceptions
        /// </summary>
        public IList<Exception> InnerExceptions { get; private set; }

        /// <summary>
        /// Construct an instance of the exception with the given information.
        /// </summary>
        /// <param name="address">The address of the connection.</param>
        /// <param name="port">The port of the connection.</param>
        /// <param name="innerException">Original exception.</param>
        public SocketTransportException(string address, ushort port, Exception innerException)
            : this(address, port, innerException == null ? null : new []{ innerException })
        {
        }

        /// <summary>
        /// Construct an instance of the exception with the given information.
        /// </summary>
        /// <param name="address">The address of the connection.</param>
        /// <param name="port">The port of the connection.</param>
        /// <param name="innerExceptions">List of original exceptions.</param>
        public SocketTransportException(string address, ushort port, IList<Exception> innerExceptions)
            : base(BuildMessage(address, port, innerExceptions))
        {
            Address = address;
            Port = port;
            InnerExceptions = innerExceptions;
        }

        /// <summary>
        /// Build the message for the exception.  Output information on all inner exceptions.
        /// </summary>
        /// <param name="address">The address of the connection.</param>
        /// <param name="port">The port of the connection.</param>
        /// <param name="innerExceptions">List of original exceptions.</param>
        /// <returns>The message for the exception, including information on all inner exceptions.</returns>
        private static string BuildMessage(string address, ushort port, IList<Exception> innerExceptions)
        {
            var innerExceptionsCount = innerExceptions?.Count;
            var builder = new StringBuilder($"{innerExceptionsCount ?? 0} exception(s) from connection {address}:{port}");

            if(innerExceptionsCount > 0)
            {
                builder.Append(" --->");

                //inner exceptions
                foreach(var exception in innerExceptions)
                {
                    builder.Append(Environment.NewLine);
                    builder.Append(exception);
                }

                builder.Append(Environment.NewLine);
                builder.Append("--- End of inner exceptions stack trace ---");

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}
