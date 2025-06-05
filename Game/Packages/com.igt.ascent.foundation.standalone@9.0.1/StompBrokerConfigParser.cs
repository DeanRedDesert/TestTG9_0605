//-----------------------------------------------------------------------
// <copyright file = "StompBrokerConfigParser.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// This class builds the STOMP broker configuration by parsing an xml
    /// element that contains the needed information for Standalone mode.
    /// </summary>
    internal class StompBrokerConfigParser
    {
        #region Private Fields

        /// <summary>
        /// The default host address is a local IP address.
        /// </summary>
        private const string DefaultHostname = "127.0.0.1";

        /// <summary>
        /// The default port is set to a port used to connect to the MiniMOM STOMP broker on the Foundation.
        /// </summary>
        private const int DefaultPort = 61613;

        #endregion

        #region Public Properties

        /// <summary>
        /// The hostname used to connect to the STOMP broker.
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// The port number used to connect to the STOMP broker.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The <see cref="StompVersion"/> that indicates the specific foundation behavior of the STOMP server
        /// and G2S implementation.
        /// </summary>
        public Version StompVersion { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// The class constructor that initializes a new instance of a STOMP broker configuration
        /// parser using an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="stompBrokerConfigElement">
        /// An xml element that contains the STOMP broker configuration.
        /// </param>
        public StompBrokerConfigParser(XContainer stompBrokerConfigElement)
        {
            Hostname = DefaultHostname;

            Port = DefaultPort;

            StompVersion = new Version(1, 0);

            if(stompBrokerConfigElement != null)
            {
                var element = stompBrokerConfigElement.Element("Hostname");
                if(element != null)
                {
                    Hostname = (string)element;
                }

                element = stompBrokerConfigElement.Element("Port");
                if(element != null)
                {
                    Port = (int)element;
                }

                element = stompBrokerConfigElement.Element("Version");
                if(element != null)
                {
                    var majorVersion = element.Element("Major");
                    var minorVersion = element.Element("Minor");

                    if(majorVersion != null && minorVersion != null)
                    {
                        StompVersion = new Version((int)majorVersion, (int)minorVersion);
                    }
                }
            }
        }

        #endregion
    }
}
