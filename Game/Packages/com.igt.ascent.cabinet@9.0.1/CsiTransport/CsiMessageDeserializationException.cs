//---------------------------------------------------------------------------
// <copyright file = "CsiMessageDeserializationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when there is an issue deserializing a CSI message.
    /// </summary>
    [Serializable]
    public class CsiMessageDeserializationException : Exception
    {
        /// <summary>
        /// The XML message which could not be deserialized.
        /// </summary>
        public string XmlMessage { get; private set; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "Error deserializing CSI message. XmlMessage: {0}";

        /// <summary>
        /// Construct an instance of the exception with the given information.
        /// </summary>
        /// <param name="xmlMessage">The XML message being decoded when the error was encountered.</param>
        /// <param name="innerException">Exception encountered during deserialization.</param>
        public CsiMessageDeserializationException(string xmlMessage, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, xmlMessage), innerException)
        {
            XmlMessage = xmlMessage;
        }
    }
}
