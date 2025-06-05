//-----------------------------------------------------------------------
// <copyright file = "ConnectMessage.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{

    /// <summary>
    /// Binary transport message for connection requests and responses.
    /// </summary>
    public sealed class ConnectMessage
    {
        private readonly BinaryMessage messageBuilder;
        private readonly ConnectBinaryMessageSegment connectSegment = new ConnectBinaryMessageSegment();

        /// <summary>
        /// The size of the message.
        /// </summary>
        /// <devdoc>
        /// Must be the total size of the message content members. The sizes are retrieved in the order of the variable
        /// declarations to reduce the chance of error.
        /// </devdoc>
        private static readonly int Size = TransportHeaderSegment.SegmentSize + TransportBodyHeaderSegment.SegmentSize + 
            ConnectBinaryMessageSegment.SegmentSize;

        #region Constructors

        /// <summary>
        /// Create an instance of the connect message with the given information.
        /// </summary>
        /// <param name="versionMajor">Major protocol version number supported.</param>
        /// <param name="versionMinor">Minor protocol version number supported.</param>
        /// <param name="bodyType">Body type (request or accept) of the connection message.</param>
        private ConnectMessage(byte versionMajor, byte versionMinor, BodyType bodyType)
        {
            var transportHeaderSegment = new TransportHeaderSegment((uint)Size, MessageType.Transport);
            var transportBodyHeaderSegment = new TransportBodyHeaderSegment {BodyType = bodyType};
            messageBuilder = new BinaryMessage(transportHeaderSegment, transportBodyHeaderSegment, connectSegment);

            connectSegment.VersionMajor = versionMajor;
            connectSegment.VersionMinor = versionMinor;
        }

        #endregion

        /// <summary>
        /// Create an instance of the connect message with the given information.
        /// </summary>
        /// <param name="versionMajor">Major protocol version number supported.</param>
        /// <param name="versionMinor">Minor protocol version number supported.</param>
        /// <returns>New <see cref="ConnectMessage"/> instance.</returns>
        public static ConnectMessage CreateRequest(byte versionMajor, byte versionMinor)
        {
            return new ConnectMessage(versionMajor, versionMinor, BodyType.ConnectionRequested);
        }

        /// <summary>
        /// Create an instance of the connect message with the given information.
        /// </summary>
        /// <param name="versionMajor">Major protocol version number supported.</param>
        /// <param name="versionMinor">Minor protocol version number supported.</param>
        /// <returns>New <see cref="ConnectMessage"/> instance.</returns>
        public static ConnectMessage CreateAccept(byte versionMajor, byte versionMinor)
        {
            return new ConnectMessage(versionMajor, versionMinor, BodyType.ConnectionAccepted);
        }

        /// <summary>
        /// Gets a byte array with the contents of this message serialized into it.
        /// </summary>
        /// <returns>A byte array with the serialized message contents.</returns>
        public byte[] GetBytes()
        {
            var bytes = new byte[messageBuilder.Size];
            messageBuilder.Write(bytes, 0);
            return bytes;
        }

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns>String containing a human readable form of the message.</returns>
        public override string ToString()
        {
            return messageBuilder.ToString();
        }
    }
}