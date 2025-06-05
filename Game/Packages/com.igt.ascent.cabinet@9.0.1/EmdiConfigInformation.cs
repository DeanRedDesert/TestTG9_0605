// -----------------------------------------------------------------------
// <copyright file = "EmdiConfigInformation.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The XML socket EMDI port and web socket EMDI port of the EMDI configuration.
    /// </summary>
    public struct EmdiConfigInformation
    {
        /// <summary>
        /// The current port number for the EMDI XML socket interface.
        /// </summary>
        public ushort XmlSocketEmdiPort { get; }

        /// <summary>
        /// The current port number for the EMDI websocket interface.
        /// </summary>
        public ushort WebSocketEmdiPort { get; }

        /// <summary>
        /// The constructor for the <see cref="EmdiConfigInformation"/>.
        /// </summary>
        /// <param name="xmlSocketEmdiPort">The EMDI XML socket interface port number.</param>
        /// <param name="webSocketEmdiPort">The EMDI websocket port number.</param>
        public EmdiConfigInformation(ushort xmlSocketEmdiPort, ushort webSocketEmdiPort) : this()
        {
            XmlSocketEmdiPort = xmlSocketEmdiPort;
            WebSocketEmdiPort = webSocketEmdiPort;
        }
    }
}