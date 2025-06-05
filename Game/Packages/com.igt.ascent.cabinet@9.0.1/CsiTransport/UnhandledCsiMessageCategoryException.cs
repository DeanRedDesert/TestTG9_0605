//-----------------------------------------------------------------------
// <copyright file = "UnhandledCsiMessageCategoryException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Globalization;
    using System.Xml.Serialization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;

    /// <summary>
    /// Exception thrown when a received CSI message category is unhandled.
    /// </summary>
    [Serializable]
    public class UnhandledCsiMessageCategoryException : Exception
    {
        /// <summary>
        /// The outer XML message associated with the exception.
        /// </summary>
        public string OuterMessage { private set; get; }

        /// <summary>
        /// The category of the message.
        /// </summary>
        public Category MessageCategory { private set; get; }

        /// <summary>
        /// XmlSerializer used to turn the outer message into an xml string.
        /// </summary>
        private static readonly XmlSerializer CsiSerializer = new XmlSerializer(typeof(Csi));

        /// <summary>
        /// Message format for the exception.
        /// </summary>
        private const string MessageFormat = "No handler for CSI category: {0}, Message: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="outerMessage">Outer message which contained an invalid category.</param>
        /// <param name="innerException">Exception which triggered this exception.</param>
        public UnhandledCsiMessageCategoryException(Csi outerMessage, Exception innerException)
            : base(
                string.Format(CultureInfo.InvariantCulture, MessageFormat, outerMessage.Category,
                              GetCsiXmlString(outerMessage)), innerException)
        {
            OuterMessage = GetCsiXmlString(outerMessage);
            MessageCategory = outerMessage.Category;
        }

        /// <summary>
        /// Get an XML string of the CSI message.
        /// </summary>
        /// <param name="outerMessage">CSI message to get a string for.</param>
        /// <returns>XML string of the message.</returns>
        private static string GetCsiXmlString(Csi outerMessage)
        {
            try
            {
                return XmlHelpers.GetXmlString(outerMessage, CsiSerializer);
            }
            catch(Exception)
            {
                return "Could not serialize outer CSI message for exception.";
            }
        }
    }
}
