// -----------------------------------------------------------------------
// <copyright file = "TransportConfiguration.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    using System;

    /// <summary>
    /// This class defines a transport configuration used for establishing connections with the Foundation.
    /// </summary>
    public class TransportConfiguration
    {
        #region Public Properties

        /// <summary>
        /// Gets the host address of a socket transport.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets the port of a socket transport.
        /// </summary>
        public ushort Port { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TransportConfiguration"/>l
        /// </summary>
        /// <param name="address">
        /// The host address of a socket transport.
        /// </param>
        /// <param name="port">
        /// The port of a socket transport.
        /// </param>
        public TransportConfiguration(string address, ushort port)
        {
            if(string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Host address cannot be null or empty.", address);
            }

            Address = address;
            Port = port;
        }

        #endregion
    }
}