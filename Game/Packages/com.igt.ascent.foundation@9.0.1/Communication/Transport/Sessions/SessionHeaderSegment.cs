//-----------------------------------------------------------------------
// <copyright file = "SessionHeaderSegment.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport.Sessions
{
    using System;
    using System.Net;

    /// <summary>
    /// An implementation of <see cref="IBinaryMessageSegment"/> for a session header.
    /// </summary>
    internal class SessionHeaderSegment : IBinaryMessageSegment
    {
        /// <inheritdoc/>
        public int Size => sizeof(int);

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        internal int SessionId { get; set; }

        /// <inheritdoc/>
        public void Read(byte[] buffer, int startPosition)
        {
            SessionId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, startPosition));
        }

        /// <inheritdoc/>
        public void Write(byte[] buffer, int startPosition)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SessionId));
            Array.Copy(bytes, 0, buffer, startPosition, bytes.Length);
        }
    }
}
